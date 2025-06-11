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

		private List<TileInfo[,]> gridLayers; 
		private List<TileInfo> allTiles;
		private Dictionary<int, List<TileInfo>> tilesByType;

		public List<TileInfo> AllTiles => allTiles.Where(t => t.controller.gameObject.activeSelf).ToList();

		public LevelConfiguration GetLevelConfig()
		{
			return levelConfig;
		}

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

							Sprite iconSprite = null;
							var prefabController = tilePrefab.GetComponent<TileController>();
							if (prefabController != null)
							{
								var iconTransform = tilePrefab.transform.Find("Icon");
								if (iconTransform != null)
								{
									var iconImage = iconTransform.GetComponent<Image>();
									if (iconImage != null)
									{
										iconSprite = iconImage.sprite;
									}
								}
							}

							Vector3 position = CalculateTilePosition(x, y, layerIndex);
							GameObject tileObj = Instantiate(tilePrefab, tilesContainer);
							tileObj.transform.localPosition = position;

							TileController controller = tileObj.GetComponent<TileController>();
							if (controller != null)
							{
								controller.Initialize(tileType, iconSprite, new Vector2Int(x, y), layerIndex);

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

			GameManager gameManager = FindObjectOfType<GameManager>();
			if (gameManager != null)
			{
				gameManager.OnTileClicked(clickedTile.controller);
			}
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

		public void ReshuffleTiles()
		{
			var activeTiles = AllTiles;
			List<int> tileTypes = new List<int>();

			foreach (var tile in activeTiles)
			{
				tileTypes.Add(tile.tileTypeID);
			}

			ShuffleList(tileTypes);

			for (int i = 0; i < activeTiles.Count; i++)
			{
				var tile = activeTiles[i];
				int newType = tileTypes[i];

				tile.tileTypeID = newType;

				if (levelConfig != null && levelConfig.tilePrefabs.Count > newType)
				{
					var prefab = levelConfig.tilePrefabs[newType];
					var prefabController = prefab.GetComponent<TileController>();
					if (prefabController != null)
					{
						var iconTransform = prefab.transform.Find("Icon");
						if (iconTransform != null)
						{
							var iconImage = iconTransform.GetComponent<Image>();
							if (iconImage != null && tile.controller.GetIconImage() != null)
							{
								tile.controller.GetIconImage().sprite = iconImage.sprite;
							}
						}
					}
				}

				tile.controller.Initialize(newType, tile.controller.GetIconImage()?.sprite, tile.gridPosition, tile.layerIndex);
			}

			tilesByType.Clear();
			foreach (var tile in activeTiles)
			{
				if (!tilesByType.ContainsKey(tile.tileTypeID))
					tilesByType[tile.tileTypeID] = new List<TileInfo>();
				tilesByType[tile.tileTypeID].Add(tile);
			}

		}
	}
}
