﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mapbox.Utils;
using Mapbox.Map;
using Mapbox.Unity.Map;
using Mapbox.Unity.Map.TileProviders;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Enums;
using UnityEngine;
using Mapbox.Unity.Map.Interfaces;
using System.Collections;

[System.Serializable]
public struct ZoomCoefficient
{
    public int zoomLevel;
    public float minDistance;
}

public class GlobeTileProviderUpdated : AbstractTileProvider
{
    public int MinZoom = 3;
    public int MaxZoom = 8;
    public ZoomCoefficient[] zooms;
    public GlobeData globeData;

    public LayerMask TileMask;
    public float Radius;

    private readonly Dictionary<GameObject, RaycastHit> _casts = new Dictionary<GameObject, RaycastHit>();
    //private readonly Dictionary<UnwrappedTileId, String> _zoomedTilesToRemove = new Dictionary<UnwrappedTileId, String>();
    //private readonly HashSet<UnwrappedTileId> _zoomedTiles = new HashSet<UnwrappedTileId>();

    private float elapsedTime = 0.0f;

    private HashSet<UnwrappedTileId> topTilesDestroyed = new HashSet<UnwrappedTileId>();

    private AbstractMap map;


    //public event Action<UnwrappedTileId, Color[]> OnTileAdded = delegate { };
    //public event Action<UnwrappedTileId> OnTileRemoved = delegate { };

    //protected HashSet<UnwrappedTileId> _activeTiles;

    public override void Initialize(IMap map)
    {
        _currentExtent.activeTiles = new HashSet<UnwrappedTileId>();
        base.Initialize(map);
    }

    protected void AddTile(UnwrappedTileId tile)//, Color[] pixels = null)
    {
        if (map.MapVisualizer.ActiveTiles.ContainsKey(tile))//_currentExtent.activeTiles.Contains(tile))
        {
            map.MapVisualizer.ActiveTiles[tile].gameObject.SetActive(true);
            return;
        }
        else
        {
            map.MapVisualizer.LoadTile(tile);
        }

        _currentExtent.activeTiles.Add(tile);
        //OnTileAdded(tile, pixels);
        //map.MapVisualizer.LoadTile(tile);
    }

    protected void RemoveTile(UnityTile tile)
    {
        if (!map.MapVisualizer.ActiveTiles.ContainsKey(tile.UnwrappedTileId))// _currentExtent.activeTiles.Contains(tile))
        {
            return;
        }

        tile.gameObject.SetActive(false);
        _currentExtent.activeTiles.Remove(tile.UnwrappedTileId);
        //OnTileRemoved(tile.UnwrappedTileId);
        map.MapVisualizer.DisposeTile(tile.UnwrappedTileId);
    }

    public void RemoveGlobe()
    {
        _currentExtent.activeTiles.Clear();
        map.MapVisualizer.ActiveTiles.Clear();
        Mapbox.Unity.MapboxAccess.Instance.Cache.Clear();
        Mapbox.Unity.MapboxAccess.Instance.Cache.ReInit();
        Destroy(gameObject);
    }

    public override void OnInitialized()
    {
        map = _map as AbstractMap;
        _map.SetZoom(Math.Max(MinZoom, Math.Min(MaxZoom, _map.Zoom)));
        map.OnTileFinished += OnTileDataChanged;
      
        var tileCover = TileCover.Get(Vector2dBounds.World(), _map.AbsoluteZoom);
        foreach (var tile in tileCover)
        {
            AddTile(new UnwrappedTileId(tile.Z, tile.X, tile.Y));
        }
        map.UpdateMap();
    }

    private void OnTileDataChanged(UnityTile unityTile)
    {
        //unityTile.gameObject.SetActive(true);
        if(Debug.isDebugBuild)
            print("Finished " + unityTile.name);
        if (unityTile.RasterDataState == TilePropertyState.Loaded)
        {
            //StringBuilder sb = new StringBuilder();
            foreach (var tileId in unityTile.UnwrappedTileId.AllChildren())
            {
                //sb.Append("Removing tileId " + tileId + "\n");
                //RemoveTile(tileId);
            }
            topTilesDestroyed.Remove(unityTile.UnwrappedTileId);
            //Debug.Log(sb.ToString());
        }
        _ = unityTile.Collider;
        //unityTile.ResetCollider();
    }

    void Update()
    {
        /*
        float dista = Vector3.Distance(Camera.main.transform.position, transform.position);

        foreach (var tile in map.MapVisualizer.ActiveTiles)
        {
            SetTileZoom(dista, tile.Value);
        }

        return; // forget the old solution, above is the faster one
        */


        //mb use this for performance, hack to call the code only 5 times per second
        elapsedTime += Time.deltaTime;

        if (elapsedTime <= 0.1f)
        {
            return;
        }

        elapsedTime = 0;

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }

        _casts.Clear();
        for (float x = 0.1f; x <= 1.0f; x += 0.4f)
        {
            for (float y = 0.1f; y <= 1.0f; y += 0.4f)
            {
                AddCast(x, y);
            }
        }
        AddCast(0.5f, 0.5f);

        Vector3 cameraPos = LevelManagerGlobe.Instance.cameraRef.transform.position;
        //Debug.Log(cameraPos);

        // Retrieved from: https://stackoverflow.com/questions/3717226/radius-of-projected-sphere
        float vFov = LevelManagerGlobe.Instance.cameraRef.fieldOfView * Mathf.Deg2Rad;

        var scaledRadius = Radius * transform.lossyScale.x;

        float dist = Vector3.Distance(cameraPos, transform.position);
        float projectedRadius = (float)(scaledRadius / (Math.Tan(vFov / 2) * dist));
        float sphereSurface = Mathf.Pow(projectedRadius, 4) * 100;

        //print("DIST: " + dist + " < " + scaledRadius * 1.10);
        //if (dist < scaledRadius * 1.10)
        //{
        foreach (var obj in _casts)
        {
            var tile = obj.Key.GetComponent<UnityTile>();
            double tileFraction = sphereSurface / (Math.Pow(2, tile.CanonicalTileId.Z));

            float distTemp = Vector3.Distance(obj.Value.point, LevelManagerGlobe.Instance.cameraRef.transform.position) * transform.lossyScale.x;

            if (distTemp < zooms[tile.CanonicalTileId.Z - MinZoom].minDistance * transform.lossyScale.x)
            {
                //if (tileFraction > 10)
                //{
                IncreaseTileZoom(tile);
                //}
            }
            else 
            {
                //if (tileFraction < 5)//3.52)
                //{
                if (tile.CanonicalTileId.Z - MinZoom - 1 < 0)
                {
                    return;
                }
                if (distTemp > zooms[tile.CanonicalTileId.Z - MinZoom - 1].minDistance * transform.lossyScale.x)
                {
                    DecreaseTileZoom(tile);
                }
                //}
            }


            /*
            foreach (var coord in _currentExtent.activeTiles.ToArray())
            {
                //Debug.Log(coord);
                double tileFraction = sphereSurface / (Math.Pow(2, coord.Z));

                if (tileFraction < 3.52)
                {
                    var z = coord.Z;
                    var x = (coord.X / 2) * 2;
                    var y = (coord.Y / 2) * 2;

                    if (z > MinZoom)
                    {
                        var uid = new UnwrappedTileId(z - 1, x / 2, y / 2);



                        //if (uid.Z == MinZoom) {
                          //  topTilesDestroyed.Add(uid);

                            z = uid.Z + 1;
                            x = uid.X * 2;
                            y = uid.Y * 2;

                            var topLeft = new UnwrappedTileId(z, x, y);
                            var topRight = new UnwrappedTileId(z, x + 1, y);
                            var bottomLeft = new UnwrappedTileId(z, x, y + 1);
                            var bottomRight = new UnwrappedTileId(z, x + 1, y + 1);

                            RemoveTile(map.MapVisualizer.ActiveTiles[topLeft]);
                            RemoveTile(map.MapVisualizer.ActiveTiles[topRight]);
                            RemoveTile(map.MapVisualizer.ActiveTiles[bottomLeft]);
                            RemoveTile(map.MapVisualizer.ActiveTiles[bottomRight]);

                            AddTile(uid);
                            map.UpdateMap();
                            return;
                        //}

                        var t = topTilesDestroyed.Intersect(uid.parentTiles()).Any();

                        if (t == false && topTilesDestroyed.Contains(uid) == false) {
                            topTilesDestroyed.Add(uid);
                            AddTile(uid);
                        }
                    }
                }
            }*/
        }
    }

    private void IncreaseTileZoom(UnityTile tile)
    {
        StartCoroutine(IncreaseTileZoomSequence(tile));
    }

    private IEnumerator IncreaseTileZoomSequence(UnityTile tile)
    {
        var z = tile.CanonicalTileId.Z + 1;
        var x = tile.CanonicalTileId.X * 2;
        var y = tile.CanonicalTileId.Y * 2;

        if (z <= MaxZoom && tile.RasterDataState == TilePropertyState.Loaded)
        {
            //Texture2D t = tile.GetRasterData();

            //if (t != null)
            //{
            var topLeft = new UnwrappedTileId(z, x, y);
            var topRight = new UnwrappedTileId(z, x + 1, y);
            var bottomLeft = new UnwrappedTileId(z, x, y + 1);
            var bottomRight = new UnwrappedTileId(z, x + 1, y + 1);

            //_zoomedTiles.Add(tile.UnwrappedTileId.topTile());

            //int width = t.width / 2;
            //int height = t.height / 2;

            AddTile(bottomLeft);//, t.GetPixels(0, 0, width, height));
            AddTile(bottomRight);//, t.GetPixels(width, 0, width, height));
            AddTile(topLeft);//, t.GetPixels(0, height, width, height));
            AddTile(topRight);//, t.GetPixels(width, height, width, height));

            map.MapVisualizer.ActiveTiles[bottomLeft].gameObject.SetActive(false);
            map.MapVisualizer.ActiveTiles[bottomRight].gameObject.SetActive(false);
            map.MapVisualizer.ActiveTiles[topLeft].gameObject.SetActive(false);
            map.MapVisualizer.ActiveTiles[topRight].gameObject.SetActive(false);

            while (
                map.MapVisualizer.ActiveTiles[bottomLeft].RasterDataState != TilePropertyState.Loaded ||
                map.MapVisualizer.ActiveTiles[bottomRight].RasterDataState != TilePropertyState.Loaded ||
                map.MapVisualizer.ActiveTiles[topLeft].RasterDataState != TilePropertyState.Loaded ||
                map.MapVisualizer.ActiveTiles[topRight].RasterDataState != TilePropertyState.Loaded)
                yield return null;
            
            map.MapVisualizer.ActiveTiles[bottomLeft].gameObject.SetActive(true);
            map.MapVisualizer.ActiveTiles[bottomRight].gameObject.SetActive(true);
            map.MapVisualizer.ActiveTiles[topLeft].gameObject.SetActive(true);
            map.MapVisualizer.ActiveTiles[topRight].gameObject.SetActive(true);

            RemoveTile(tile);
            map.UpdateMap();
            //}
        }
    }

    private void DecreaseTileZoom(UnityTile tile)
    {
        var z = tile.CanonicalTileId.Z;
        var x = (tile.CanonicalTileId.X / 2) * 2;
        var y = (tile.CanonicalTileId.Y / 2) * 2;

        if (z > MinZoom)
        {
            var uid = new UnwrappedTileId(z - 1, x / 2, y / 2);

            z = uid.Z + 1;
            x = uid.X * 2;
            y = uid.Y * 2;

            var topLeft = new UnwrappedTileId(z, x, y);
            var topRight = new UnwrappedTileId(z, x + 1, y);
            var bottomLeft = new UnwrappedTileId(z, x, y + 1);
            var bottomRight = new UnwrappedTileId(z, x + 1, y + 1);

            RemoveTile(map.MapVisualizer.ActiveTiles[topLeft]);
            RemoveTile(map.MapVisualizer.ActiveTiles[topRight]);
            RemoveTile(map.MapVisualizer.ActiveTiles[bottomLeft]);
            RemoveTile(map.MapVisualizer.ActiveTiles[bottomRight]);

            AddTile(uid);
            map.MapVisualizer.ActiveTiles[uid].gameObject.SetActive(true);
            map.UpdateMap();
        }
    }

    private void AddCast(float x, float y)
    {
        Ray ray = LevelManagerGlobe.Instance.cameraRef.ViewportPointToRay(new Vector3(x, y));
        RaycastHit hit = new RaycastHit();

        Debug.DrawRay(ray.origin, ray.direction * 100);
        if (!Physics.Raycast(ray, out hit, float.PositiveInfinity, TileMask.value)) return;

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

    public override void UpdateTileExtent()
    {
        // HACK: don't allow too many tiles to be requested.
        if (_map.AbsoluteZoom > 5)
        {
            throw new System.Exception("Too many tiles! Use a lower zoom level!");
        }

        var tileCover = TileCover.Get(Vector2dBounds.World(), _map.AbsoluteZoom);
        foreach (var tile in tileCover)
        {
            _currentExtent.activeTiles.Add(new UnwrappedTileId(tile.Z, tile.X, tile.Y));
        }
        OnExtentChanged();
    }

    public override bool Cleanup(UnwrappedTileId tile)
    {
        return (!_currentExtent.activeTiles.Contains(tile));
    }
}