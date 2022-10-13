﻿namespace Mapbox.Unity.Map
{
	using System;
	using Mapbox.Map;
    using UnityEngine;

	public interface ITileProvider
	{
		event Action<UnwrappedTileId, Color[]> OnTileAdded;
		event Action<UnwrappedTileId> OnTileRemoved;

		// TODO: add cancel event?
		// Alternatively, give mapvisualizer an object recycling strategy that can separately determine when to change gameobjects.
		// This removal would essentially lead to a cancel request and nothing more.

		void Initialize(IMap map);

		// TODO: add reset/clear method?
	}

	public class TileStateChangedEventArgs : EventArgs
	{
		public UnwrappedTileId TileId;
	}
}
