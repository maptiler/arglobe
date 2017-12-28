﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mapbox.Utils;
using Mapbox.Map;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Enums;
using Mapbox.Unity.Utilities;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;
using System.Text;

public class GlobeTileProviderUpdated : AbstractTileProvider
{
    public int MinZoom = 3;
    public int MaxZoom = 8;

    public LayerMask TileMask;
    public float Radius;

    private readonly Dictionary<GameObject, RaycastHit> _casts = new Dictionary<GameObject, RaycastHit>();
    private readonly Dictionary<UnwrappedTileId, String> _zoomedTilesToRemove = new Dictionary<UnwrappedTileId, String>();
    private readonly HashSet<UnwrappedTileId> _zoomedTiles = new HashSet<UnwrappedTileId>();

    private float elapsedTime = 0.0f;

    private HashSet<UnwrappedTileId> topTilesDestroyed = new HashSet<UnwrappedTileId>();

    private AbstractMap map;

    internal override void OnInitialized()
    {
        map = _map as AbstractMap;
        _map.Zoom = Math.Max(MinZoom, Math.Min(MaxZoom, _map.Zoom));
        map.OnTileDataChanged += OnTileDataChanged;
      
        var tileCover = TileCover.Get(Vector2dBounds.World(), _map.Zoom);
        foreach (var tile in tileCover)
        {
            AddTile(new UnwrappedTileId(tile.Z, tile.X, tile.Y));
        }
    }

    private void OnTileDataChanged(UnityTile unityTile)
    {
        if (unityTile.RasterDataState == TilePropertyState.Loaded)
        {
            //StringBuilder sb = new StringBuilder();

            foreach (var tileId in unityTile.UnwrappedTileId.AllChildren())
            {
                //sb.Append("Removing tileId " + tileId + "\n");
                RemoveTile(tileId);
            }

            topTilesDestroyed.Remove(unityTile.UnwrappedTileId);
            //Debug.Log(sb.ToString());
        }
    }

    void FixedUpdate()
    {
        //mb use this for performance, hack to call the code only 5 times per second
        elapsedTime += Time.deltaTime;

        if (elapsedTime <= 0.1f) {
            return;
        }

        elapsedTime = 0;

        _casts.Clear();
		for (float x = 0.1f; x <= 1.0f; x += 0.4f)
		{
			for (float y = 0.1f; y <= 1.0f; y += 0.4f)
			{
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(x, y));
                RaycastHit hit = new RaycastHit();

                Debug.DrawRay(ray.origin, ray.direction * 100);
                if (!Physics.Raycast(ray, out hit, float.PositiveInfinity, TileMask.value)) continue;

                var obj = hit.transform.gameObject;
                if (_casts.ContainsKey(obj))
                {
                    if (_casts[obj].distance < hit.distance)
                    {
                        _casts[obj] = hit;
                    }
                }
                else
                {
                    _casts[obj] = hit;
                }
            }
        }

        Vector3 cameraPos = Camera.main.gameObject.transform.position;

        // Retrieved from: https://stackoverflow.com/questions/3717226/radius-of-projected-sphere
        float vFov = Camera.main.fieldOfView * Mathf.Deg2Rad;
        //float hFov = 2 * Mathf.Atan(Mathf.Tan(vFov / 2) * Camera.main.aspect);
        //float fov = Math.Min(hFov, vFov);

        // Debug.Log("VFOV : " + vFov + " HFOV : " + hFov);


        var scaledRadius = Radius * transform.lossyScale.x;

        float dist = Vector3.Distance(cameraPos, transform.position);
        float projectedRadius = (float) (scaledRadius / (Math.Tan(vFov / 2) * dist));
        float sphereSurface = Mathf.Pow(projectedRadius, 4) * 100;

		//var random = new HashSet<UnwrappedTileId>(_zoomedTiles);

		//foreach (var obj in _casts.Keys)
		//{
		//	var tile = obj.GetComponent<UnityTile>().UnwrappedTileId;
		//	var topTile = tile.topTile();
		//	if (random.Contains(topTile))
		//	{
		//		random.Remove(topTile);
		//	}
		//}

		//foreach (var tileid in random)
		//{
		//	AddTile(tileid);
		//	_zoomedTiles.Remove(tileid);
		//}

        if (dist > scaledRadius * 1.10)
        {
			foreach (var obj in _casts.Keys)
			{
				var tile = obj.GetComponent<UnityTile>();
				double tileFraction = sphereSurface / (Math.Pow(2, tile.CanonicalTileId.Z));

                if (tileFraction > 10)
				{
					var z = tile.CanonicalTileId.Z + 1;
					var x = tile.CanonicalTileId.X * 2;
					var y = tile.CanonicalTileId.Y * 2;

                    if (z <= MaxZoom && tile.RasterDataState == TilePropertyState.Loaded)
					{
						Texture2D t = tile.GetRasterData();

                        if (t != null)
                        {
                            var topLeft = new UnwrappedTileId(z, x, y);
                            var topRight = new UnwrappedTileId(z, x + 1, y);
                            var bottomLeft = new UnwrappedTileId(z, x, y + 1);
                            var bottomRight = new UnwrappedTileId(z, x + 1, y + 1);

                            //_zoomedTiles.Add(tile.UnwrappedTileId.topTile());

                            int width = t.width / 2;
                            int height = t.height / 2;

							AddTile(bottomLeft, t.GetPixels(0, 0, width, height));
							AddTile(bottomRight, t.GetPixels(width, 0, width, height));
                            AddTile(topLeft, t.GetPixels(0, height, width, height));
							AddTile(topRight, t.GetPixels(width, height, width, height));

							RemoveTile(tile.UnwrappedTileId);
						}
					}
				}
			}
        }

		foreach (var coord in _activeTiles.ToArray())
		{
			double tileFraction = sphereSurface / (Math.Pow(2, coord.Z));

			if (tileFraction < 3.52)
			{
				var z = coord.Z;
				var x = (coord.X / 2) * 2;
				var y = (coord.Y / 2) * 2;

				if (z > MinZoom)
				{
                    var uid = new UnwrappedTileId(z - 1, x / 2, y / 2);

                    if (uid.Z == MinZoom) {
                        topTilesDestroyed.Add(uid);
                        AddTile(uid);
                        return;
                    }

                    var t = topTilesDestroyed.Intersect(uid.parentTiles()).Any();

                    if (t == false && topTilesDestroyed.Contains(uid) == false) {
                        topTilesDestroyed.Add(uid);
                        AddTile(uid);
                    }

     //               if (t == false && topTilesDestroyed.Contains(uid) == false) {

					//	Color[][] pixels = new Color[4][];

					//	var children = uid.SimpleChildren();
     //                   var noTextureDetected = false;

					//	for (int i = 0; i < children.Count; i++)
					//	{
					//		var child = children[i];
					//		var childTile = map.mapVisualizer.Tiles[child];
					//		var text = childTile.GetRasterData();

     //                       if (text != null) {

					//			Texture2D scaled;

     //                           if (text.width > 128 || text.width < 128)
					//			{
					//				scaled = TextureScaler.scaled(text, 128, 128);
					//			}
					//			else
					//			{
					//				scaled = text;
					//			}

     //                           pixels[i] = scaled.GetPixels();
     //                       }
     //                       else {
     //                           //Debug.Log("No texture detected");
     //                           noTextureDetected = true;
     //                           break;
     //                       }
					//	}

     //                   if (noTextureDetected == false) {
					//		topTilesDestroyed.Add(uid);
					//		AddTile(uid, pixels);
     //                   }
					//}
				}
			}
		}
    }
}