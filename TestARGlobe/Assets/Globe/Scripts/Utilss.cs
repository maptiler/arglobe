using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Map;
using UnityEngine;

public static class Utilss
{
    public static UnwrappedTileId Super(this UnwrappedTileId tile)
    {
        return new UnwrappedTileId(tile.Z - 1, tile.X / 2, tile.Y / 2);
    }

    public static List<UnwrappedTileId> AllChildren(this UnwrappedTileId tile)
    {
        List<UnwrappedTileId> list = new List<UnwrappedTileId>();
        var z = tile.Z + 1;
        var x = tile.X * 2;
        var y = tile.Y * 2;
        list.Add(new UnwrappedTileId(z, x, y));
        list.Add(new UnwrappedTileId(z, x + 1, y));
        list.Add(new UnwrappedTileId(z, x, y + 1));
        list.Add(new UnwrappedTileId(z, x + 1, y + 1));

        // max zoom based on distance is currently 6, we don't need tiles for 7
        if (z < 6) {
            list.AddRange(new UnwrappedTileId(z, x, y).AllChildren());
            list.AddRange(new UnwrappedTileId(z, x + 1, y).AllChildren());
            list.AddRange(new UnwrappedTileId(z, x, y + 1).AllChildren());
            list.AddRange(new UnwrappedTileId(z, x + 1, y + 1).AllChildren());
        }

        return list;
    }

	public static List<UnwrappedTileId> SimpleChildren(this UnwrappedTileId tile)
	{
		List<UnwrappedTileId> list = new List<UnwrappedTileId>();
		var z = tile.Z + 1;
		var x = tile.X * 2;
		var y = tile.Y * 2;
		list.Add(new UnwrappedTileId(z, x, y));
		list.Add(new UnwrappedTileId(z, x + 1, y));
		list.Add(new UnwrappedTileId(z, x, y + 1));
		list.Add(new UnwrappedTileId(z, x + 1, y + 1));

		return list;
	}

    public static UnwrappedTileId topTile(this UnwrappedTileId tile) {

        if (tile.Z <= 3) {
            return tile;
        }

        return tile.Super().topTile();
    }

    public static List<UnwrappedTileId> parentTiles(this UnwrappedTileId tile) {

		List<UnwrappedTileId> list = new List<UnwrappedTileId>();

        var t = tile;

        while (t.Z > 3) {
            var super = t.Super();
            list.Add(super);
            t = super;
        }

		return list;
    }

    public static double DegreeToRadian(double angle)
    {
        return Math.PI * angle / 180.0;
    }
}