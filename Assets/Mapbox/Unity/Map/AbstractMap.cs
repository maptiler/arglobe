﻿using UnityEngine.Profiling;
using System.Collections.Generic;
using Mapbox.Unity.MeshGeneration.Data;

namespace Mapbox.Unity.Map
{
    using System;
    using Mapbox.Unity.MeshGeneration;
    using Mapbox.Unity.Utilities;
    using Utils;
    using UnityEngine;
    using Mapbox.Map;

    // TODO: make abstract! For example: MapFromFile, MapFromLocationProvider, etc.
    public class AbstractMap : MonoBehaviour, IMap
    {
        [Geocode] [SerializeField] string _latitudeLongitudeString;

        [SerializeField] [Range(0, 22)] int _zoom;

        public int Zoom
        {
            get { return _zoom; }
            set { _zoom = value; }
        }

        [SerializeField] Transform _root;

        public Transform Root
        {
            get { return _root; }
        }

        [SerializeField] AbstractTileProvider _tileProvider;

        [SerializeField] MapVisualizer _mapVisualizer;


        [HideInInspector] public MapVisualizer mapVisualizer {
            get { return _mapVisualizer; }
        }

        [SerializeField] float _unityTileSize = 100;
        [SerializeField] bool _snapMapHeightToZero = true;

        MapboxAccess _fileSouce;

        Vector2d _mapCenterLatitudeLongitude;

        public Vector2d CenterLatitudeLongitude
        {
            get { return _mapCenterLatitudeLongitude; }
            set
            {
                _latitudeLongitudeString = string.Format("{0}, {1}", value.x, value.y);
                _mapCenterLatitudeLongitude = value;
            }
        }

        Vector2d _mapCenterMercator;

        public Vector2d CenterMercator
        {
            get { return _mapCenterMercator; }
        }

        float _worldRelativeScale;

        public float WorldRelativeScale
        {
            get { return _worldRelativeScale; }
        }

        bool _worldHeightFixed = false;
        public bool RescaleObject = false;

        public event Action OnInitialized = delegate { };
        public event Action<UnityTile> OnTileDataChanged = delegate { };

        protected virtual void Awake()
        {
            _worldHeightFixed = false;
            _fileSouce = MapboxAccess.Instance;
            _tileProvider.OnTileAdded += TileProvider_OnTileAdded;
            _tileProvider.OnTileRemoved += TileProvider_OnTileRemoved;
            _mapVisualizer.OnTileDataChanged += MapVisualiserTileDataChanged;
            if (!_root)
            {
                _root = transform;
            }
        }

        private void MapVisualiserTileDataChanged(UnityTile unityTile)
        {
            OnTileDataChanged(unityTile);
        }

        protected virtual void OnDestroy()
        {
            if (_tileProvider != null)
            {
                _tileProvider.OnTileAdded -= TileProvider_OnTileAdded;
                _tileProvider.OnTileRemoved -= TileProvider_OnTileRemoved;
            }

            _mapVisualizer.Destroy();
        }

        // This is the part that is abstract?
        protected virtual void Start()
        {
            var latLonSplit = _latitudeLongitudeString.Split(',');
            _mapCenterLatitudeLongitude = new Vector2d(double.Parse(latLonSplit[0]), double.Parse(latLonSplit[1]));

            var referenceTileRect =
                Conversions.TileBounds(TileCover.CoordinateToTileId(_mapCenterLatitudeLongitude, _zoom));
            _mapCenterMercator = referenceTileRect.Center;

            _worldRelativeScale = (float) (_unityTileSize / referenceTileRect.Size.x);
            if (RescaleObject)
            {
                Root.localScale = Vector3.one * _worldRelativeScale;
            }

            _mapVisualizer.Initialize(this, _fileSouce);
            _tileProvider.Initialize(this);

            OnInitialized();
        }

        public Dictionary<UnwrappedTileId, UnityTile> tiles = new Dictionary<UnwrappedTileId, UnityTile>();

        void TileProvider_OnTileAdded(UnwrappedTileId tileId, Color[] pixels)
        {
            UnityTile tile = null;
            if (_snapMapHeightToZero && !_worldHeightFixed)
            {
                _worldHeightFixed = true;
                tile = _mapVisualizer.LoadTile(tileId, pixels);
                if (tile.HeightDataState == MeshGeneration.Enums.TilePropertyState.Loaded)
                {
                    var h = tile.QueryHeightData(.5f, .5f);
                    Root.transform.position = new Vector3(
                        Root.transform.position.x,
                        -h * WorldRelativeScale,
                        Root.transform.position.z);
                }
                else
                {
                    tile.OnHeightDataChanged += (s) =>
                    {
                        var h = s.QueryHeightData(.5f, .5f);
                        Root.transform.position = new Vector3(
                            Root.transform.position.x,
                            -h * WorldRelativeScale,
                            Root.transform.position.z);
                    };
                }
            }
            else
            {
                //Profiler.BeginSample("MapVis");
                tile = _mapVisualizer.LoadTile(tileId, pixels);
                //Profiler.EndSample();
            }
            tiles.Add(tileId, tile);
        }

        void TileProvider_OnTileRemoved(UnwrappedTileId tileId)
        {
            //Profiler.BeginSample("MapVisDispose");
            _mapVisualizer.DisposeTile(tileId);
            tiles.Remove(tileId);
            //Profiler.EndSample();
        }
    }
}