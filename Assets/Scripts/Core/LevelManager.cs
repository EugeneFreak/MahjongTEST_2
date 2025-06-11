using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace MahjongGame.Core
{
	public class LevelManager : MonoBehaviour
	{
		[Header("Настройки уровня")]
		[SerializeField] private LevelConfiguration levelConfig;
		[SerializeField] private Transform tilesContainer;
		[SerializeField] private GameManager gameManager;

		[Header("Настройки сетки")]
		[SerializeField] private float tileSpacing = 130f;
		[SerializeField] private float layerOffsetX = 20f;
		[SerializeField] private float layerOffsetY = -20f;

		private Dictionary<Vector3Int, TileInfo> tileGrid = new Dictionary<Vector3Int, TileInfo>();
		private List<TileInfo> allTileInfos = new List<TileInfo>();

		public List<TileInfo> AllTiles => allTileInfos.Where(t => t.controller != null && t.controller.gameObject.activeInHierarchy).ToList();

		private void Start()
		{
			if (gameManager == null)
				gameManager = FindObjectOfType<GameManager>();

			GenerateLevel();
		}

		public void GenerateLevel()
		{
			ClearLevel();

			if (levelConfig == null)
			{
				Debug.LogError("Level configuration is missing!");
				return;
			}

			CreatePyramidLayout();
			CheckAllTilesBlocking();
		}

		private void CreatePyramidLayout()
		{
			int totalLayers = levelConfig.Layers;

			for (int layer = 0; layer < totalLayers; layer++)
			{
				int gridSize = 8 - layer * 2;
				int startOffset = layer;

				List<Vector2Int> positions = new List<Vector2Int>();

				for (int row = 0; row < gridSize; row++)
				{
					for (int col = 0; col < gridSize; col++)
					{
						positions.Add(new Vector2Int(startOffset + col, startOffset + row));
					}
				}

				List<int> tileTypes = GenerateTileTypes(positions.Count);

				for (int i = 0; i < positions.Count && i < tileTypes.Count; i++)
				{
					Vector2Int gridPos = positions[i];
					float xPos = (gridPos.x - 3.5f) * tileSpacing + layer * layerOffsetX;
					float yPos = (gridPos.y - 3.5f) * tileSpacing + layer * layerOffsetY;

					GameObject tile = SpawnTile(tileTypes[i], gridPos, layer, new Vector3(xPos, yPos, 0));

					if (tile != null)
					{
						TileController controller = tile.GetComponent<TileController>();
						if (controller != null)
						{
							Vector3Int gridKey = new Vector3Int(gridPos.x, gridPos.y, layer);
							TileInfo tileInfo = new TileInfo(controller, gridPos, layer, tileTypes[i]);
							tileGrid[gridKey] = tileInfo;
							allTileInfos.Add(tileInfo);
						}
					}
				}
			}
		}

		private List<int> GenerateTileTypes(int count)
		{
			List<int> types = new List<int>();
			int tileTypeCount = 10;
			int pairsNeeded = count / 2;

			while (types.Count < count)
			{
				for (int i = 0; i < tileTypeCount && types.Count < count - 1; i++)
				{
					types.Add(i);
					types.Add(i);
				}
			}

			types = types.Take(count).ToList();

			for (int i = 0; i < types.Count; i++)
			{
				int randomIndex = Random.Range(i, types.Count);
				int temp = types[i];
				types[i] = types[randomIndex];
				types[randomIndex] = temp;
			}

			return types;
		}

		private GameObject SpawnTile(int tileType, Vector2Int gridPos, int layer, Vector3 worldPos)
		{
			GameObject tile = ResourceTileSpawner.SpawnTile(tileType, tilesContainer);

			if (tile == null)
			{
				Debug.LogError($"Failed to spawn tile type {tileType}");
				return null;
			}

			RectTransform rectTransform = tile.GetComponent<RectTransform>();
			if (rectTransform != null)
			{
				rectTransform.anchoredPosition = new Vector2(worldPos.x, worldPos.y);
			}

			TileController controller = tile.GetComponent<TileController>();
			if (controller != null)
			{
				Transform iconTransform = tile.transform.Find("Icon");
				Sprite iconSprite = null;
				if (iconTransform != null)
				{
					Image iconImage = iconTransform.GetComponent<Image>();
					if (iconImage != null)
					{
						iconSprite = iconImage.sprite;
					}
				}

				controller.Initialize(tileType, iconSprite, gridPos, layer);

				Button button = tile.GetComponent<Button>();
				if (button != null)
				{
					button.onClick.RemoveAllListeners();
					button.onClick.AddListener(() => OnTileClicked(controller));
				}
			}

			return tile;
		}

		private void OnTileClicked(TileController tile)
		{
			if (tile.IsBlocked) return;

			if (gameManager != null)
			{
				gameManager.OnTileClicked(tile);
			}
		}

		public TileInfo GetTileInfo(TileController controller)
		{
			return allTileInfos.FirstOrDefault(t => t.controller == controller);
		}

		private void CheckAllTilesBlocking()
		{
			foreach (var tileInfo in allTileInfos)
			{
				if (tileInfo.controller != null && tileInfo.controller.gameObject.activeInHierarchy)
				{
					bool isBlocked = IsTileBlocked(tileInfo);
					tileInfo.controller.SetBlocked(isBlocked);
				}
			}
		}

		private bool IsTileBlocked(TileInfo tileInfo)
		{
			Vector3Int tilePos = new Vector3Int(tileInfo.gridPosition.x, tileInfo.gridPosition.y, tileInfo.layerIndex);

			if (HasTileOnTop(tilePos))
				return true;

			bool hasLeft = HasTileOnSide(tilePos, -1);
			bool hasRight = HasTileOnSide(tilePos, 1);

			return hasLeft && hasRight;
		}

		private bool HasTileOnTop(Vector3Int position)
		{
			Vector3Int topPos = new Vector3Int(position.x, position.y, position.z + 1);
			return tileGrid.ContainsKey(topPos) && tileGrid[topPos].controller != null &&
				   tileGrid[topPos].controller.gameObject.activeInHierarchy;
		}

		private bool HasTileOnSide(Vector3Int position, int direction)
		{
			Vector3Int sidePos = new Vector3Int(position.x + direction, position.y, position.z);
			return tileGrid.ContainsKey(sidePos) && tileGrid[sidePos].controller != null &&
				   tileGrid[sidePos].controller.gameObject.activeInHierarchy;
		}

		public void RemoveTile(TileInfo tileInfo)
		{
			if (tileInfo.controller != null)
			{
				tileInfo.controller.RemoveTile();
			}

			CheckAllTilesBlocking();
		}

		public void ReshuffleTiles()
		{
			var activeTiles = AllTiles;
			List<int> tileTypes = activeTiles.Select(t => t.tileTypeID).ToList();

			for (int i = 0; i < tileTypes.Count; i++)
			{
				int randomIndex = Random.Range(i, tileTypes.Count);
				int temp = tileTypes[i];
				tileTypes[i] = tileTypes[randomIndex];
				tileTypes[randomIndex] = temp;
			}

			for (int i = 0; i < activeTiles.Count; i++)
			{
				activeTiles[i].tileTypeID = tileTypes[i];
				activeTiles[i].controller.Initialize(tileTypes[i], null, activeTiles[i].gridPosition, activeTiles[i].layerIndex);
			}
		}

		private void ClearLevel()
		{
			foreach (var tileInfo in allTileInfos)
			{
				if (tileInfo.controller != null)
					Destroy(tileInfo.controller.gameObject);
			}

			allTileInfos.Clear();
			tileGrid.Clear();

			foreach (Transform child in tilesContainer)
			{
				Destroy(child.gameObject);
			}
		}

		public void RestartLevel()
		{
			GenerateLevel();
		}

		public LevelConfiguration GetLevelConfig()
		{
			return levelConfig;
		}
	}
}