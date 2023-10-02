﻿namespace Mapbox.Platform.Cache
{

	using Mapbox.Map;
	using Mapbox.Utils;
	//using SQLite4Unity3d;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using UnityEngine;


	//public class SQLTileWorkaround
	//{
	//	public int tile_set;
	//	public int zoom_level;
	//	public int tile_column;
	//	public int tile_row;
	//	public byte[] tile_data;
	//	public int timestamp;
	//	public string etag;
	//}

	public class SQLiteConnectionWorkaround
	{
		public Dictionary<string, tiles> tiles;
		public Dictionary<string, tilesets> tilesets;

		public SQLiteConnectionWorkaround()
		{
			tiles = new Dictionary<string, tiles>();
			tilesets = new Dictionary<string, tilesets>();
		}

		public void AddTile(string tileKey, tiles newTile)
		{
			tiles foundTile;
			if (tiles.TryGetValue(tileKey, out foundTile))
			{
				tiles[tileKey] = newTile;
			}
			else
			{
				tiles.Add(tileKey, newTile);
			}
		}

		public void AddTileset(string tilesetKey, tilesets newTileset)
		{
			tilesets.Add(tilesetKey, newTileset);
		}
	}


	public class SQLiteCache : ICache, IDisposable
	{


		/// <summary>
		/// maximum number tiles that get cached
		/// </summary>
		public uint MaxCacheSize { get { return _maxTileCount; } }


		/// <summary>
		/// Check cache size every n inserts
		/// </summary>
		public uint PruneCacheDelta { get { return _pruneCacheDelta; } }


#if MAPBOX_DEBUG_CACHE
		private string _className;
#endif
		private bool _disposed;
		private string _dbName;
		private string _dbPath;
		//private SQLiteConnection _sqlite;
		private SQLiteConnectionWorkaround _sqliteWorkaround;
		private readonly uint _maxTileCount;
		/// <summary>check cache size only every '_pruneCacheDelta' calls to 'Add()' to avoid being too chatty with the database</summary>
		private const int _pruneCacheDelta = 20;
		/// <summary>counter to keep track of calls to `Add()`</summary>
		private int _pruneCacheCounter = 0;
		private object _lock = new object();


		public SQLiteCache(uint? maxTileCount = null, string dbName = "cache.db")
		{
			_maxTileCount = maxTileCount ?? 3000;
			_dbName = dbName;
			init();
		}


		#region idisposable


		~SQLiteCache()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposeManagedResources)
		{
			if (!_disposed)
			{
				/*if (disposeManagedResources)
				{
					if (null != _sqlite)
					{
						_sqlite.Execute("VACUUM;"); // compact db to keep file size small
						_sqlite.Close();
						_sqlite.Dispose();
						_sqlite = null;
					}
				}*/
				_sqliteWorkaround = null;
				_disposed = true;
			}
		}


		#endregion


		private void init()
		{

#if MAPBOX_DEBUG_CACHE
			_className = this.GetType().Name;
#endif
			openOrCreateDb(_dbName);

			//hrmpf: multiple PKs not supported by sqlite.net
			//https://github.com/praeclarum/sqlite-net/issues/282
			//do it via plain SQL
			/*
			List<SQLiteConnection.ColumnInfo> colInfoTileset = _sqlite.GetTableInfo(typeof(tilesets).Name);
			if (0 == colInfoTileset.Count)
			{
				string cmdCreateTableTilesets = @"CREATE TABLE tilesets(
id    INTEGER PRIMARY KEY ASC AUTOINCREMENT NOT NULL UNIQUE,
name  STRING  NOT NULL
);";
				_sqlite.Execute(cmdCreateTableTilesets);
				string cmdCreateIdxNames = @"CREATE UNIQUE INDEX idx_names ON tilesets (name ASC);";
				_sqlite.Execute(cmdCreateIdxNames);
			}

			List<SQLiteConnection.ColumnInfo> colInfoTiles = _sqlite.GetTableInfo(typeof(tiles).Name);
			if (0 == colInfoTiles.Count)
			{

				string cmdCreateTableTiles = @"CREATE TABLE tiles(
tile_set     INTEGER REFERENCES tilesets (id) ON DELETE CASCADE ON UPDATE CASCADE,
zoom_level   INTEGER NOT NULL,
tile_column  BIGINT  NOT NULL,
tile_row     BIGINT  NOT NULL,
tile_data    BLOB    NOT NULL,
timestamp    INTEGER NOT NULL,
etag         TEXT,
lastmodified INTEGER,
	PRIMARY KEY(
		tile_set ASC,
		zoom_level ASC,
		tile_column ASC,
		tile_row ASC
	)
);";
				_sqlite.Execute(cmdCreateTableTiles);

				string cmdIdxTileset = "CREATE INDEX idx_tileset ON tiles (tile_set ASC);";
				_sqlite.Execute(cmdIdxTileset);
				string cmdIdxTimestamp = "CREATE INDEX idx_timestamp ON tiles (timestamp ASC);";
				_sqlite.Execute(cmdIdxTimestamp);
			}


			// some pragmas to speed things up a bit :-)
			// inserting 1,000 tiles takes 1-2 sec as opposed to ~20 sec
			string[] cmds = new string[]
			{
				"PRAGMA synchronous=OFF",
				"PRAGMA count_changes=OFF",
				"PRAGMA journal_mode=MEMORY",
				"PRAGMA temp_store=MEMORY"
			};
			foreach (var cmd in cmds)
			{
				try
				{
					_sqlite.Execute(cmd);
				}
				catch (SQLiteException ex)
				{
					// workaround for sqlite.net's exeception:
					// https://stackoverflow.com/a/23839503
					if (ex.Result != SQLite3.Result.Row)
					{
						UnityEngine.Debug.LogErrorFormat("{0}: {1}", cmd, ex);
						// TODO: when mapbox-sdk-cs gets backported to its own repo -> throw
						//throw; // to throw or not to throw???
					}
				}
			}*/
		}


		private void openOrCreateDb(string dbName)
		{
			/*_dbPath = GetFullDbPath(dbName);
			_sqlite = new SQLiteConnection(_dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);*/
			_sqliteWorkaround = new SQLiteConnectionWorkaround();
		}


		/// <summary>
		/// <para>Reinitialize cache.</para>
		/// <para>This is needed after 'Clear()' to recreate the cache database.</para>
		/// <para>And has been implemented on purpose to not hold on to any references to the cache directory after 'Clear()'</para>
		/// </summary>
		public void ReInit()
		{
			/*if (null != _sqlite)
			{
				_sqlite.Dispose();
				_sqlite = null;
			}*/

			init();
		}


		public static string GetFullDbPath(string dbName)
		{
			string dbPath = Path.Combine(Application.persistentDataPath, "cache");
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_WSA
			dbPath = Path.GetFullPath(dbPath);
#endif
			if (!Directory.Exists(dbPath)) { Directory.CreateDirectory(dbPath); }
			dbPath = Path.Combine(dbPath, dbName);

			return dbPath;
		}



		public void Add(string tilesetName, CanonicalTileId tileId, CacheItem item, bool forceInsert = false)
		{

#if MAPBOX_DEBUG_CACHE
			string methodName = _className + "." + new System.Diagnostics.StackFrame().GetMethod().Name;
			UnityEngine.Debug.LogFormat("{0} {1} {2} forceInsert:{3}", methodName, tileset, tileId, forceInsert);
#endif
			try
			{
				// tile exists and we don't want to overwrite -> exit early
				if (
					TileExists(tilesetName, tileId)
					&& !forceInsert
				)
				{
					return;
				}

				int? tilesetId = null;
				lock (_lock)
				{
					tilesetId = getTilesetId(tilesetName);
					if (!tilesetId.HasValue)
					{
						tilesetId = insertTileset(tilesetName);
					}
				}

				if (tilesetId < 0)
				{
					Debug.LogErrorFormat("could not get tilesetID for [{0}] tile: {1}", tilesetName, tileId);
					return;
				}

				string newKey = tileId.ToString();
				if(Debug.isDebugBuild)
					Debug.Log("Adding: " + newKey);
				tiles newValue = new tiles();
				newValue.tile_set = tilesetId.Value;
				newValue.zoom_level = tileId.Z;
				newValue.tile_column = tileId.X;
				newValue.tile_row = tileId.Y;
				newValue.tile_data = item.Data;
				newValue.timestamp = (int)UnixTimestampUtils.To(DateTime.Now);
				newValue.etag = item.ETag;
				_sqliteWorkaround.AddTile(newKey, newValue);
				/*int rowsAffected = _sqlite.InsertOrReplace(new tiles
				{
					tile_set = tilesetId.Value,
					zoom_level = tileId.Z,
					tile_column = tileId.X,
					tile_row = tileId.Y,
					tile_data = item.Data,
					timestamp = (int)UnixTimestampUtils.To(DateTime.Now),
					etag = item.ETag
				});
				if (1 != rowsAffected)
				{
					throw new Exception(string.Format("tile [{0} / {1}] was not inserted, rows affected:{2}", tilesetName, tileId, rowsAffected));
				}*/
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Error inserting {0} {1} {2} ", tilesetName, tileId, ex);
			}

			// update counter only when new tile gets inserted
			if (!forceInsert)
			{
				_pruneCacheCounter++;
			}
			if (0 == _pruneCacheCounter % _pruneCacheDelta)
			{
				_pruneCacheCounter = 0;
				//prune();
			}
		}


		/*private void prune()
		{

			long tileCnt = _sqlite.ExecuteScalar<long>("SELECT COUNT(zoom_level) FROM tiles");

			if (tileCnt < _maxTileCount) { return; }

			long toDelete = tileCnt - _maxTileCount;

#if MAPBOX_DEBUG_CACHE
			string methodName = _className + "." + new System.Diagnostics.StackFrame().GetMethod().Name;
			Debug.LogFormat("{0} {1} about to prune()", methodName, _tileset);
#endif

			try
			{
				// no 'ORDER BY' or 'LIMIT' possible if sqlite hasn't been compiled with 'SQLITE_ENABLE_UPDATE_DELETE_LIMIT'
				// https://sqlite.org/compile.html#enable_update_delete_limit
				_sqlite.Execute("DELETE FROM tiles WHERE rowid IN ( SELECT rowid FROM tiles ORDER BY timestamp ASC LIMIT ? );", toDelete);
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("error pruning: {0}", ex);
			}
		}*/


		/// <summary>
		/// Returns the tile data, otherwise null
		/// </summary>
		/// <param name="tileId">Canonical tile id to identify the tile</param>
		/// <returns>tile data as byte[], if tile is not cached returns null</returns>
		public CacheItem Get(string tilesetName, CanonicalTileId tileId)
		{
#if MAPBOX_DEBUG_CACHE
			string methodName = _className + "." + new System.Diagnostics.StackFrame().GetMethod().Name;
			Debug.LogFormat("{0} {1} {2}", methodName, _tileset, tileId);
#endif
			tiles tile = null;

			try
			{
				int? tilesetId = getTilesetId(tilesetName);
				if (!tilesetId.HasValue)
				{
					return null;
				}

				_sqliteWorkaround.tiles.TryGetValue(tileId.ToString(), out tile);
				if (tile == null)
					return null;
				/*tile = _sqlite
					.Table<tiles>()
					.Where(t =>
						t.tile_set == tilesetId.Value
						&& t.zoom_level == tileId.Z
						&& t.tile_column == tileId.X
						&& t.tile_row == tileId.Y
						)
					.FirstOrDefault();*/
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("error getting tile {1} {2} from cache{0}{3}", Environment.NewLine, tilesetName, tileId, ex);
				return null;
			}
			if (null == tile)
			{
				return null;
			}

			DateTime? lastModified = null;
			if (tile.lastmodified.HasValue) { lastModified = UnixTimestampUtils.From((double)tile.lastmodified.Value); }

			return new CacheItem()
			{
				Data = tile.tile_data,
				AddedToCacheTicksUtc = tile.timestamp,
				ETag = tile.etag,
				LastModified = lastModified
			};
		}


		/// <summary>
		/// Check if tile exists
		/// </summary>
		/// <param name="tileId">Canonical tile id</param>
		/// <returns>True if tile exists</returns>
		public bool TileExists(string tilesetName, CanonicalTileId tileId)
		{
			return _sqliteWorkaround.tiles.ContainsKey(tileId.ToString());
			/*int? tilesetId = getTilesetId(tilesetName);
			if (!tilesetId.HasValue)
			{
				return false;
			}

			return null != _sqlite
				.Table<tiles>()
				.Where(t =>
					t.tile_set == tilesetId.Value
					&& t.zoom_level == tileId.Z
					&& t.tile_column == tileId.X
					&& t.tile_row == tileId.Y
					)
				.FirstOrDefault();*/
		}


		private int insertTileset(string tilesetName)
		{
			tilesets newTileset = new tilesets { name = tilesetName };
			_sqliteWorkaround.AddTileset(tilesetName, newTileset);
			return newTileset.id;
			/*try
			{
				_sqlite.BeginTransaction(true);
				tilesets newTileset = new tilesets { name = tilesetName };
				int rowsAffected = _sqlite.Insert(newTileset);
				if (1 != rowsAffected)
				{
					throw new Exception(string.Format("tileset [{0}] was not inserted, rows affected:{1}", tilesetName, rowsAffected));
				}
				return newTileset.id;
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("could not insert tileset [{0}]: {1}", tilesetName, ex);
				return -1;
			}
			finally
			{
				_sqlite.Commit();
			}*/
		}


		private int? getTilesetId(string tilesetName)
		{
			tilesets tileset;
			_sqliteWorkaround.tilesets.TryGetValue(tilesetName, out tileset);
			return null == tileset ? (int?)null : tileset.id;

			/*tilesets tileset = _sqlite
				.Table<tilesets>()
				.Where(ts => ts.name.Equals(tilesetName))
				.FirstOrDefault();
			return null == tileset ? (int?)null : tileset.id;*/
		}


		/// <summary>
		/// FOR INTERNAL DEBUGGING ONLY - DON'T RELY ON IN PRODUCTION
		/// </summary>
		/// <param name="tilesetName"></param>
		/// <returns></returns>
		public long TileCount(string tilesetName)
		{
			int? tilesetId = getTilesetId(tilesetName);
			if (!tilesetId.HasValue) { return 0; }

			return _sqliteWorkaround.tiles.Where(t => t.Value.tile_set == tilesetId.Value).LongCount();

			/*return _sqlite
				.Table<tiles>()
				.Where(t => t.tile_set == tilesetId.Value)
				.LongCount();*/
		}


		/// <summary>
		/// Clear cache for one tile set
		/// </summary>
		/// <param name="tilesetName"></param>
		public void Clear(string tilesetName)
		{
			int? tilesetId = getTilesetId(tilesetName);
			if (!tilesetId.HasValue) { return; }
			_sqliteWorkaround.tilesets.Remove(tilesetName);

			//just delete on table 'tilesets', we've setup cascading which should take care of tabls 'tiles'
			//_sqlite.Delete<tilesets>(tilesetId.Value);
		}


		/// <summary>
		/// <para>Delete the database file.</para>
		/// <para>Call 'ReInit()' if you intend to continue using the cache after 'Clear()!</para>
		/// </summary>
		public void Clear()
		{
			if(null == _sqliteWorkaround) { return; }

			_sqliteWorkaround = null;

			//already disposed
			/*if (null == _sqlite) { return; }

			_sqlite.Close();
			_sqlite.Dispose();
			_sqlite = null;

			Debug.LogFormat("deleting {0}", _dbPath);

			// try several times in case SQLite needs a bit more time to dispose
			for (int i = 0; i < 5; i++)
			{
				try
				{
					File.Delete(_dbPath);
					return;
				}
				catch
				{
#if !WINDOWS_UWP
					System.Threading.Thread.Sleep(100);
#else
					System.Threading.Tasks.Task.Delay(100).Wait();
#endif
				}
			}

			// if we got till here, throw on last try
			File.Delete(_dbPath);*/
		}

	}
}
