using UnityEngine.Profiling;

namespace Mapbox.Unity.MeshGeneration.Factories
{
    using UnityEngine;
    using Mapbox.Unity.MeshGeneration.Data;
    using Mapbox.Unity.Utilities;
    using System.Collections.Generic;
    using Mapbox.Utils;

    [CreateAssetMenu(menuName = "Mapbox/Factories/Terrain Factory - Flat Sphere")]
    public class FlatSphereTerrainFactory : AbstractTileFactory
    {
        [SerializeField] private Material _baseMaterial;

        [SerializeField] private float _radius;

        [SerializeField] [Range(2, 256)] int _sampleCount;

		[SerializeField] private bool _inside = false;

        [SerializeField] private bool _addCollider = false;

        [SerializeField] private bool _addToLayer = false;

        [SerializeField] private int _layerId = 0;

        internal override void OnInitialized()
        {
        }

        internal override void OnRegistered(UnityTile tile)
        {
            if (_addToLayer && tile.gameObject.layer != _layerId)
            {
                //tile.gameObject.layer = _layerId;
            }

            if (tile.MeshRenderer == null)
            {
                var renderer = tile.gameObject.AddComponent<MeshRenderer>();
                renderer.material = _baseMaterial;
            }

            if (tile.MeshFilter == null)
            {
                tile.gameObject.AddComponent<MeshFilter>();
            }

            // HACK: This is here in to make the system trigger a finished state.
            Progress++;
            //Profiler.BeginSample("Terrain");
            GenerateTerrainMesh(tile);
            //Profiler.EndSample();
            Progress--;

			if (_addCollider) {

				BoxCollider collider;
				if (tile.Collider == null)
				{
					collider = tile.gameObject.AddComponent<BoxCollider>();
				}
				else
				{
					collider = (BoxCollider) tile.Collider;
				}

				var bounds = tile.MeshFilter.mesh.bounds;
				collider.center = bounds.center;
				collider.size = bounds.size;
			}
        }

        void GenerateTerrainMesh(UnityTile tile)
        {
            //Profiler.BeginSample("Vers");
            var verts = new List<Vector3>(_sampleCount * _sampleCount);


            float minX = (float) tile.Rect.Min.x;
            float maxX = (float) (minX + tile.Rect.Size.x);
            float minY = (float) tile.Rect.Max.y;
            float maxY = (float) (minY + tile.Rect.Size.y);

            for (float x = 0; x < _sampleCount; x++)
            {
                for (float y = 0; y < _sampleCount; y++)
                {
                    //Profiler.BeginSample("Calc");
                    var xx = Mathf.Lerp(minX, maxX, x / (_sampleCount - 1));
                    var yy = Mathf.Lerp(minY, maxY, y / (_sampleCount - 1));

                    var ll = Conversions.MetersToLatLon(new Vector2d(xx, yy));

                    var latitude = (float) (Mathf.Deg2Rad * ll.x);
                    var longitude = (float) (Mathf.Deg2Rad * ll.y);

                    float xPos = (_radius) * Mathf.Cos(latitude) * Mathf.Cos(longitude);
                    float zPos = (_radius) * Mathf.Cos(latitude) * Mathf.Sin(longitude);
                    float yPos = (_radius) * Mathf.Sin(latitude);
                    //Profiler.EndSample();


                    var pp = new Vector3(xPos, yPos, zPos);
                    verts.Add(pp);
                }
            }
            //Profiler.EndSample();

            var trilist = new List<int>((_sampleCount - 1) * (_sampleCount - 1) * 6);
            for (int y = 0; y < _sampleCount - 1; y++)
            {
                for (int x = 0; x < _sampleCount - 1; x++)
                {
                    trilist.Add((y * _sampleCount) + x);
                    trilist.Add((y * _sampleCount) + x + _sampleCount + 1);
                    trilist.Add((y * _sampleCount) + x + _sampleCount);

                    trilist.Add((y * _sampleCount) + x);
                    trilist.Add((y * _sampleCount) + x + 1);
                    trilist.Add((y * _sampleCount) + x + _sampleCount + 1);
                }
            }
			var uvlist = new List<Vector2>();
            var step = 1f / (_sampleCount - 1);
            for (int i = 0; i < _sampleCount; i++)
            {
                for (int j = 0; j < _sampleCount; j++)
                {
                    uvlist.Add(new Vector2(i * step, (j * step)));
                }
            }

			if (_inside == true) {
				trilist.Reverse ();
			}


            tile.MeshFilter.mesh.SetVertices(verts);
            tile.MeshFilter.mesh.SetTriangles(trilist, 0);
            tile.MeshFilter.mesh.SetUVs(0, uvlist);
            tile.MeshFilter.mesh.RecalculateBounds();
            tile.MeshFilter.mesh.RecalculateNormals();

            tile.transform.localPosition = Vector3.zero;
        }

        internal override void OnUnregistered(UnityTile tile)
        {
        }
    }
}