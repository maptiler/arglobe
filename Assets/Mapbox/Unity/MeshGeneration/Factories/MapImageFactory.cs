using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace Mapbox.Unity.MeshGeneration.Factories
{
    using System;
    using Mapbox.Map;
    using UnityEngine;
    using Mapbox.Unity.MeshGeneration.Enums;
    using Mapbox.Unity.MeshGeneration.Data;
    using Mapbox.Unity.Utilities;

    public enum MapImageType
    {
        BasicMapboxStyle,
        Custom,
        None
    }

    /// <summary>
    /// Uses raster image services to create materials & textures for terrain
    /// </summary>
    [CreateAssetMenu(menuName = "Mapbox/Factories/Image Factory")]
    public class MapImageFactory : AbstractTileFactory
    {
        [SerializeField] MapImageType _mapIdType;

        [SerializeField] [StyleSearch] Style _customStyle;

        [SerializeField] string _mapId = "";

        [SerializeField] bool _useCompression = true;

        [SerializeField] bool _useMipMap = false;

        [SerializeField] bool _useRetina;

        // TODO: come back to this
        //public override void Update()
        //{
        //    base.Update();
        //    foreach (var tile in _tiles.Values)
        //    {
        //        Run(tile);
        //    }
        //}

        internal override void OnInitialized()
        {
        }

        internal override void OnRegistered(UnityTile tile)
        {
            ////Profiler.BeginSample("IMAGE");
            if (_mapIdType == MapImageType.None)
                return;

			RasterTile rasterTile;
            if (_mapId.StartsWith("mapbox://", StringComparison.Ordinal))
            {
                rasterTile = _useRetina ? new RetinaRasterTile() : new RasterTile();
            }
            else
            {
                rasterTile = _useRetina ? new ClassicRetinaRasterTile() : new ClassicRasterTile();
            }

            tile.RasterDataState = TilePropertyState.Loading;

            tile.AddTile(rasterTile);

            Progress++;

            if (tile.CanonicalTileId.Z <= 3) {
                rasterTile.InitializeCustom(tile.CanonicalTileId);

				Texture2D text = (Texture2D)Resources.Load("GlobeImages/" + GlobeControl.globeName + "/" + tile.CanonicalTileId.customToString());
                tile.StartSetTexture(Instantiate(text), _useCompression);
                Resources.UnloadAsset(text);
                return;
            }
	        
			rasterTile.Initialize(_fileSource, tile.CanonicalTileId, _mapId, () =>
				{

                    if (rasterTile.wasCancelled == true) {
                        Progress--;
                        return;
                    }

                    if (rasterTile.HasError)
					{
						tile.RasterDataState = TilePropertyState.Error;
						Progress--;
						// HACK: redownload the tile if time expired, otherwise it's probably wrong URL, so don't download it again
						if (rasterTile.repeatCall == true) {
							OnRegistered(tile);
						}
						return;
					}

					tile.StartSetRasterData(rasterTile.Data, _useMipMap, _useCompression);
					Progress--;
				});

            
            Profiler.EndSample();
        }

        internal override void OnUnregistered(UnityTile tile)
        {
        }
    }
}