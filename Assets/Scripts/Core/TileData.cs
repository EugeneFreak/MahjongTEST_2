using UnityEngine;
using System;
using System.Collections.Generic;

namespace MahjongGame.Core
{
	[Serializable]
	public class LayerConfig
	{
		public string layerName = "Layer";
		public int width = 6;
		public int height = 6;
		public Vector2 offset = Vector2.zero;
		public float zOffset = 0f;

		public LayerConfig(string name, int w, int h, Vector2 off, float z)
		{
			layerName = name;
			width = w;
			height = h;
			offset = off;
			zOffset = z;
		}
	}

	public class TileInfo
	{
		public TileController controller; 
		public Vector2Int gridPosition; 
		public int layerIndex; 
		public int tileTypeID;

		public TileInfo(TileController ctrl, Vector2Int pos, int layer, int typeID)
		{
			controller = ctrl;
			gridPosition = pos;
			layerIndex = layer;
			tileTypeID = typeID;
		}
	}
}
