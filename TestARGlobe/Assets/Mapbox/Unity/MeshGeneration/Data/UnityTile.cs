using System.Collections;
using UnityEngine.Profiling;

namespace Mapbox.Unity.MeshGeneration.Data
{
    using UnityEngine;
    using Mapbox.Unity.MeshGeneration.Enums;
    using Mapbox.Unity.Utilities;
    using Utils;
    using Mapbox.Map;
    using System;
    using Mapbox.Unity.Map;
    using System.Collections.Generic;
    using System.IO;

    public class UnityTile : MonoBehaviour
    {
        [SerializeField] Texture2D _rasterData;

        float[] _heightData;

        float _relativeScale;

        Texture2D _heightTexture;

        List<Tile> _tiles = new List<Tile>();

        MeshRenderer _meshRenderer;

        public MeshRenderer MeshRenderer
        {
            get
            {
                if (_meshRenderer == null)
                {
                    _meshRenderer = GetComponent<MeshRenderer>();
                }
                return _meshRenderer;
            }
        }

        private MeshFilter _meshFilter;

        public MeshFilter MeshFilter
        {
            get
            {
                if (_meshFilter == null)
                {
                    _meshFilter = GetComponent<MeshFilter>();
                }
                return _meshFilter;
            }
        }

        private Collider _collider;

        public Collider Collider
        {
            get
            {
                if (_collider == null)
                {
                    _collider = GetComponent<Collider>();
                }
                return _collider;
            }
        }

        // TODO: should this be a string???
        string _vectorData;

        public string VectorData
        {
            get { return _vectorData; }
            set
            {
                _vectorData = value;
                OnVectorDataChanged(this);
            }
        }

        RectD _rect;

        public RectD Rect
        {
            get { return _rect; }
        }

        UnwrappedTileId _unwrappedTileId;

        public UnwrappedTileId UnwrappedTileId
        {
            get { return _unwrappedTileId; }
        }

        CanonicalTileId _canonicalTileId;

        public CanonicalTileId CanonicalTileId
        {
            get { return _canonicalTileId; }
        }

        public TilePropertyState RasterDataState;
        public TilePropertyState HeightDataState;
        public TilePropertyState VectorDataState;

        public event Action<UnityTile> OnHeightDataChanged = delegate { };
        public event Action<UnityTile> OnRasterDataChanged = delegate { };
        public event Action<UnityTile> OnVectorDataChanged = delegate { };

        public bool settingRasterData = false;

        internal void Initialize(IMap map, UnwrappedTileId tileId)
        {
            _relativeScale = 1 / Mathf.Cos(Mathf.Deg2Rad * (float) map.CenterLatitudeLongitude.x);
            _rect = Conversions.TileBounds(tileId);
            _unwrappedTileId = tileId;
            _canonicalTileId = tileId.Canonical;
            gameObject.name = _canonicalTileId.ToString();
            var position = new Vector3((float) (_rect.Center.x - map.CenterMercator.x), 0,
                (float) (_rect.Center.y - map.CenterMercator.y));
            transform.localPosition = position;
            //gameObject.SetActive(true);
            //Debug.Log("Activating tile: " + _unwrappedTileId.ToString());
        }

        internal void Recycle()
        {
            // TODO: to hide potential visual artifacts, use placeholder mesh / texture?

            gameObject.layer = 4;
            gameObject.SetActive(false);

            // Reset internal state.
            RasterDataState = TilePropertyState.None;
            HeightDataState = TilePropertyState.None;
            VectorDataState = TilePropertyState.None;

            OnHeightDataChanged = delegate { };
            OnRasterDataChanged = delegate { };
            OnVectorDataChanged = delegate { };

            Cancel();
            _tiles.Clear();

            // HACK: this is for vector layer features and such.
            // It's slow and wasteful, but a better solution will be difficult.
            var childCount = transform.childCount;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }
        }

        internal void SetHeightData(byte[] data, float heightMultiplier = 1f, bool useRelative = false)
        {
            // HACK: compute height values for terrain. We could probably do this without a texture2d.
            if (_heightTexture == null)
            {
                _heightTexture = new Texture2D(0, 0);
            }

            _heightTexture.LoadImage(data);
            byte[] rgbData = _heightTexture.GetRawTextureData();

            // Get rid of this temporary texture. We don't need to bloat memory.
            _heightTexture.LoadImage(null);

            if (_heightData == null)
            {
                _heightData = new float[256 * 256];
            }

            var relativeScale = useRelative ? _relativeScale : 1f;
            for (int xx = 0; xx < 256; ++xx)
            {
                for (int yy = 0; yy < 256; ++yy)
                {
                    float r = rgbData[(xx * 256 + yy) * 4 + 1];
                    float g = rgbData[(xx * 256 + yy) * 4 + 2];
                    float b = rgbData[(xx * 256 + yy) * 4 + 3];
                    _heightData[xx * 256 + yy] = relativeScale * heightMultiplier *
                                                 Conversions.GetAbsoluteHeightFromColor(r, g, b);
                }
            }

            HeightDataState = TilePropertyState.Loaded;
            OnHeightDataChanged(this);
        }

        public float QueryHeightData(float x, float y)
        {
            if (_heightData != null)
            {
                var intX = (int) Mathf.Clamp(x * 256, 0, 255);
                var intY = (int) Mathf.Clamp(y * 256, 0, 255);
                return _heightData[intY * 256 + intX];
            }

            return 0;
        }

        public void StartSetRasterData(byte[] data, bool mipmap, bool compress)
        {
			gameObject.SetActive(true);
            StartCoroutine(SetRasterData(data, mipmap, compress));
            //SetRasterData(data, mipmap, compress);
        }

        public void StartSetTexture(Texture2D texture, bool useCompresion) {

            if (settingRasterData == false && RasterDataState != TilePropertyState.Loaded) {
				gameObject.SetActive(true);
				gameObject.layer = 11;
				SetTexture(texture, useCompresion);
            }
        }

        private static int _lastFrameUsed;
        private static GameObject _currentObj;

		public bool ByteArrayToFile(string fileName, byte[] byteArray)
		{
			try
			{
				using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
				{
					fs.Write(byteArray, 0, byteArray.Length);
					return true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Exception caught in process: {0}", ex);
				return false;
			}
		}

        //public void SetRasterData(byte[] data, bool useMipMap, bool useCompression)
        public IEnumerator SetRasterData(byte[] data, bool useMipMap, bool useCompression)
        {
            //ByteArrayToFile(_canonicalTileId.Z.ToString() + _canonicalTileId.X.ToString() + _canonicalTileId.Y.ToString(), data);

            while (true)
            {
                _currentObj = gameObject;

                if (_currentObj == gameObject && _lastFrameUsed != Time.renderedFrameCount)
                {
                    break;
                }
                yield return null;
            }

            settingRasterData = true;

            _lastFrameUsed = Time.renderedFrameCount;

            if (_rasterData == null)
            {
                _rasterData = new Texture2D(0, 0, TextureFormat.RGB565, false);
                _rasterData.wrapMode = TextureWrapMode.Clamp;
            }

            //MeshRenderer.material.mainTexture = _rasterData;

			MeshRenderer.material.SetTexture("_MainTex", _rasterData);

            _rasterData.LoadImage(data, false);
            gameObject.layer = 11;

            //Debug.Log("Did set raster data");
            ////Profiler.BeginSample("Load Tex");

			////Profiler.EndSample();

            //if (useCompression)
            //{
            //    _rasterData.Compress(false);
            //}

            //MeshRenderer.material.SetColor("_Diffusecolor", Color.white);

			RasterDataState = TilePropertyState.Loaded;
			OnRasterDataChanged(this);
            _currentObj = null;
            settingRasterData = false;
        }

        public void SetTexture(Texture2D texture, bool useCompression)
		{
            _rasterData = texture;

			MeshRenderer.material.SetTexture("_MainTex", _rasterData);

			//if (useCompression)
			//{
			//	_rasterData.Compress(false);
			//}

            //MeshRenderer.material.SetColor("_Diffusecolor", Color.white);

			RasterDataState = TilePropertyState.Loaded;
			OnRasterDataChanged(this);

			//_currentObj = null;
		}

        protected virtual void OnDestroy()
        {
            if (_rasterData != null)
            {
                Destroy(_rasterData);
                _rasterData = null;
                Destroy(MeshRenderer.material.mainTexture);
                MeshRenderer.material.mainTexture = null;
            }
            if (MeshFilter != null)
            {
                if (MeshFilter.mesh != null)
                {
                    Destroy(MeshFilter.mesh);
                    MeshFilter.mesh = null;
                }
                Destroy(MeshFilter);
            }
            if (MeshRenderer != null)
            {
                Destroy(MeshRenderer);
            }
        }

        public void ClearTexture()
        {
			if (_rasterData != null)
			{
				Destroy(_rasterData);
				_rasterData = null;
                Destroy(MeshRenderer.material.GetTexture("_MainTex"));
				MeshRenderer.material.mainTexture = null;
			}

            //MeshRenderer.material.SetColor("_Diffusecolor", new Color(1f, 1f, 1f, 0f));
        }

        public Texture2D GetRasterData()
        {
            return _rasterData;
        }

        internal void AddTile(Tile tile)
        {
            _tiles.Add(tile);
        }

        public void Cancel()
        {
            for (int i = 0, _tilesCount = _tiles.Count; i < _tilesCount; i++)
            {
                _tiles[i].Cancel();
            }
            StopAllCoroutines();
        }
    }
}