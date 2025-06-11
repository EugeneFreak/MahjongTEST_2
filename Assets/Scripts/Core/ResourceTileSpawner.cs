using UnityEngine;

namespace MahjongGame.Core
{
	public static class ResourceTileSpawner
	{
		private static readonly string[] tileNames = {
			"acorn", "amanita", "apple", "asparagus", "avocado",
			"bananas", "becone", "beer", "beet", "blueberry"
		};

		public static GameObject GetTilePrefab(int tileType)
		{
			if (tileType < 0 || tileType >= tileNames.Length)
			{
				Debug.LogError($"Invalid tile type: {tileType}");
				return null;
			}

			string prefabPath = $"Prefabs/Tiles/Tile_{tileNames[tileType]}";
			GameObject prefab = Resources.Load<GameObject>(prefabPath);

			if (prefab == null)
			{
				Debug.LogError($"Failed to load tile prefab from Resources: {prefabPath}");
			}

			return prefab;
		}

		public static GameObject SpawnTile(int tileType, Transform parent)
		{
			GameObject prefab = GetTilePrefab(tileType);
			if (prefab != null)
			{
				return Object.Instantiate(prefab, parent);
			}
			return null;
		}
	}
}
