using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace MahjongGame.Core
{

	public class LevelManager : MonoBehaviour
	{
		[Header("Конфигурация")]
		[SerializeField] private LevelConfiguration levelConfig;
		[SerializeField] private Transform tilesContainer; 
		[SerializeField] private float tileSize = 120f; 
		[SerializeField] private float tileSpacing = 5f;

		[Header("Отладка")]
		[SerializeField] private bool showDebugInfo = true; 

		private List<TileInfo[,]> gridLayers; 
		private List<TileInfo> allTiles; 
		private Dictionary<int, List<TileInfo>> tilesByType;

		public List<TileInfo> AllTiles => allTiles.Where(t => t.controller.gameObject.activeSelf).ToList();

		private void Awake()
		{
			gridLayers = new List<TileInfo[,]>();
			allTiles = new List<TileInfo>();
			tilesByType = new Dictionary<int, List<TileInfo>>();

			if (tilesContainer == null)
			{
				GameObject container = new GameObject("TilesContainer");
				container.transform.SetParent(transform);
				container.transform.localPosition = Vector3.zero;
				tilesContainer = container.transform;
			}
		}

		public void GenerateLevel()
		{
			ClearLevel();

			if (levelConfig == null || levelConfig.tilePrefabs.Count == 0)
			{
				Debug.LogError("Не настроена конфигурация уровня или нет префабов плиток!");
				return;
			}

			int totalPositions = 0;
			foreach (var layer in levelConfig.layers)
			{
				totalPositions += layer.width * layer.height;
			}

			List<int> tileTypes = GenerateTileTypesList(totalPositions);

			ShuffleList(tileTypes);

			int tileIndex = 0;
			for (int layerIndex = 0; layerIndex < levelConfig.layers.Count; layerIndex++)
			{
				var layerConfig = levelConfig.layers[layerIndex];
				TileInfo[,] grid = new TileInfo[layerConfig.width, layerConfig.height];

				for (int y = 0; y < layerConfig.height; y++)
				{
					for (int x = 0; x < layerConfig.width; x++)
					{
						if (tileIndex < tileTypes.Count)
						{
							int tileType = tileTypes[tileIndex];
							GameObject tilePrefab = levelConfig.tilePrefabs[tileType % levelConfig.tilePrefabs.Count];

							Vector3 position = CalculateTilePosition(x, y, layerIndex);
							GameObject tileObj = Instantiate(tilePrefab, tilesContainer);
							tileObj.transform.localPosition = position;
							tileObj.name = $"Tile_L{layerIndex}_X{x}_Y{y}";

							TileController controller = tileObj.GetComponent<TileController>();
							if (controller != null)
							{
								controller.Initialize(tileType, null, new Vector2Int(x, y), layerIndex);

								TileInfo tileInfo = new TileInfo(controller, new Vector2Int(x, y), layerIndex, tileType);
								grid[x, y] = tileInfo;
								allTiles.Add(tileInfo);

								if (!tilesByType.ContainsKey(tileType))
									tilesByType[tileType] = new List<TileInfo>();
								tilesByType[tileType].Add(tileInfo);

								Button btn = controller.GetComponent<Button>();
								if (btn != null)
								{
									int capturedIndex = allTiles.Count - 1;
									btn.onClick.AddListener(() => OnTileClicked(capturedIndex));
								}
							}

							tileIndex++;
						}
					}
				}

				gridLayers.Add(grid);
			}

			UpdateAllTilesBlockState();

			Debug.Log($"Уровень сгенерирован: {totalPositions} плиток, {levelConfig.layers.Count} слоев");
		}

		public void ClearLevel()
		{
			foreach (Transform child in tilesContainer)
			{
				Destroy(child.gameObject);
			}

			gridLayers.Clear();
			allTiles.Clear();
			tilesByType.Clear();
		}

		private List<int> GenerateTileTypesList(int totalPositions)
		{
			List<int> types = new List<int>();
			int numTypes = levelConfig.tilePrefabs.Count;

			if (totalPositions % 2 != 0)
			{
				Debug.LogWarning("Нечетное количество позиций! Уменьшаем на 1");
				totalPositions--;
			}

			int pairsNeeded = totalPositions / 2;
			int pairsPerType = Mathf.Max(levelConfig.minPairsPerType, pairsNeeded / numTypes);

			for (int i = 0; i < numTypes && types.Count < totalPositions; i++)
			{
				int pairsToAdd = Mathf.Min(pairsPerType, (totalPositions - types.Count) / 2);
				for (int j = 0; j < pairsToAdd * 2; j++)
				{
					types.Add(i);
				}
			}

			while (types.Count < totalPositions)
			{
				int randomType = Random.Range(0, numTypes);
				types.Add(randomType);
				types.Add(randomType); 
			}

			return types;
		}

		private void ShuffleList<T>(List<T> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				int randomIndex = Random.Range(i, list.Count);
				T temp = list[i];
				list[i] = list[randomIndex];
				list[randomIndex] = temp;
			}
		}
		private Vector3 CalculateTilePosition(int x, int y, int layerIndex)
		{
			var layer = levelConfig.layers[layerIndex];

			float totalTileSize = tileSize + tileSpacing;

			float gridWidth = layer.width * totalTileSize - tileSpacing;
			float gridHeight = layer.height * totalTileSize - tileSpacing;

			float posX = x * totalTileSize - gridWidth / 2f + tileSize / 2f;
			float posY = -y * totalTileSize + gridHeight / 2f - tileSize / 2f;

			posX += layer.offset.x;
			posY += layer.offset.y;

			return new Vector3(posX, posY, layer.zOffset);
		}

		private void OnTileClicked(int tileIndex)
		{
			if (tileIndex < 0 || tileIndex >= allTiles.Count) return;

			TileInfo clickedTile = allTiles[tileIndex];
			if (clickedTile.controller.IsBlocked) return;

			Debug.Log($"Клик по плитке: {clickedTile.controller.name}, Тип: {clickedTile.tileTypeID}");
		}

		public void UpdateAllTilesBlockState()
		{
			foreach (var tile in allTiles)
			{
				if (tile.controller.gameObject.activeSelf)
				{
					bool isBlocked = IsTileBlocked(tile);
					tile.controller.SetBlocked(isBlocked);
				}
			}
		}

		private bool IsTileBlocked(TileInfo tile)
		{

			if (HasTileAbove(tile))
				return true;

			bool hasLeft = HasTileOnSide(tile, -1);
			bool hasRight = HasTileOnSide(tile, 1);

			return hasLeft && hasRight;
		}

		private bool HasTileAbove(TileInfo tile)
		{

			for (int layer = tile.layerIndex + 1; layer < gridLayers.Count; layer++)
			{
				var grid = gridLayers[layer];
				var layerConfig = levelConfig.layers[layer];

				for (int x = 0; x < layerConfig.width; x++)
				{
					for (int y = 0; y < layerConfig.height; y++)
					{
						if (grid[x, y] != null && grid[x, y].controller.gameObject.activeSelf)
						{
							if (AreTilesOverlapping(tile, grid[x, y]))
								return true;
						}
					}
				}
			}

			return false;
		}

		private bool HasTileOnSide(TileInfo tile, int direction)
		{
			var grid = gridLayers[tile.layerIndex];
			int checkX = tile.gridPosition.x + direction;
			int checkY = tile.gridPosition.y;

			if (checkX < 0 || checkX >= levelConfig.layers[tile.layerIndex].width)
				return false;

			var sideTile = grid[checkX, checkY];
			return sideTile != null && sideTile.controller.gameObject.activeSelf;
		}

		private bool AreTilesOverlapping(TileInfo tile1, TileInfo tile2)
		{
			Vector3 pos1 = tile1.controller.transform.localPosition;
			Vector3 pos2 = tile2.controller.transform.localPosition;

			float halfSize = tileSize / 2f;

			bool overlapX = Mathf.Abs(pos1.x - pos2.x) < tileSize;
			bool overlapY = Mathf.Abs(pos1.y - pos2.y) < tileSize;

			return overlapX && overlapY;
		}

		public void RemoveTile(TileInfo tile)
		{
			tile.controller.RemoveTile();
			UpdateAllTilesBlockState();
		}

		public TileInfo GetTileInfo(TileController controller)
		{
			return allTiles.FirstOrDefault(t => t.controller == controller);
		}
	}
}
