using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MahjongGame.UI;
using UnityEngine.UI;

namespace MahjongGame.Core
{
	public class GameManager : MonoBehaviour
	{
		[Header("Настройки игры")]
		[SerializeField] private float autoPlayDelay = 1f;
		
		private LevelManager levelManager;
		private UIController uiController;
		private TileInfo selectedTile; 
		private bool isProcessing = false; 
		private bool isAutoPlaying = false; 
		private Coroutine autoPlayCoroutine; 

		public System.Action OnGameWon; 

		private void Awake()
		{
			if (levelManager == null)
				levelManager = FindObjectOfType<LevelManager>();

			if (uiController == null)
				uiController = FindObjectOfType<UIController>();
		}

		private void Start()
		{
			if (uiController != null)
			{
				uiController.OnRestartRequested += RestartLevel;
				uiController.OnAutoPlayToggled += ToggleAutoPlay;
			}

			RestartLevel();
		}

		private void OnDestroy()
		{
			if (uiController != null)
			{
				uiController.OnRestartRequested -= RestartLevel;
				uiController.OnAutoPlayToggled -= ToggleAutoPlay;
			}
		}

		public void RestartLevel()
		{
			StopAutoPlay();

			selectedTile = null;
			isProcessing = false;

			if (uiController != null)
			{
				uiController.HideSelectedTile();
			}

			if (levelManager != null)
				levelManager.GenerateLevel();
		}

		public void OnTileClicked(TileController tileController)
		{
			if (isProcessing || isAutoPlaying) return;

			TileInfo clickedTile = levelManager.GetTileInfo(tileController);
			if (clickedTile == null) return;

			if (clickedTile.controller.IsBlocked) return;

			ProcessTileSelection(clickedTile);
		}

		private void ProcessTileSelection(TileInfo tile)
		{
			if (selectedTile == null)
			{
				SelectTile(tile);
			}
			else if (selectedTile == tile)
			{
				DeselectTile();
			}
			else
			{
				if (selectedTile.tileTypeID == tile.tileTypeID)
				{
					ProcessMatch(selectedTile, tile);
				}
				else
				{
					DeselectTile();
					SelectTile(tile);
				}
			}
		}

		private void SelectTile(TileInfo tile)
		{
			selectedTile = tile;
			tile.controller.SetSelected(true);

			if (uiController != null && levelManager != null)
			{

				var prefabs = levelManager.GetLevelConfig()?.tilePrefabs;
				if (prefabs != null && tile.tileTypeID < prefabs.Count)
				{
					var prefab = prefabs[tile.tileTypeID];
					var prefabController = prefab.GetComponent<TileController>();
					if (prefabController != null)
					{
						var iconTransform = prefab.transform.Find("Icon");
						if (iconTransform != null)
						{
							var iconImage = iconTransform.GetComponent<Image>();
							if (iconImage != null && iconImage.sprite != null)
							{
								uiController.ShowSelectedTile(iconImage.sprite);
							}
						}
					}
				}
			}
		}

		private void DeselectTile()
		{
			if (selectedTile != null)
			{
				selectedTile.controller.SetSelected(false);
				selectedTile = null;

				if (uiController != null)
					uiController.HideSelectedTile();
			}
		}

		private void ProcessMatch(TileInfo tile1, TileInfo tile2)
		{
			isProcessing = true;

			levelManager.RemoveTile(tile1);
			levelManager.RemoveTile(tile2);

			selectedTile = null;
			if (uiController != null)
				uiController.HideSelectedTile();

			CheckGameState();

			isProcessing = false;
		}

		private void CheckGameState()
		{
			var activeTiles = levelManager.AllTiles;

			if (activeTiles.Count == 0)
			{
				OnGameWon?.Invoke();
				StopAutoPlay();

				if (uiController != null)
				{
					var victory = GameObject.FindObjectOfType<Victory>();
					if (victory != null)
						victory.Show();
				}

				return;
			}

			if (!HasAvailableMoves())
			{
				StartCoroutine(ReshuffleTiles());
			}
		}

		private IEnumerator ReshuffleTiles()
		{
			isProcessing = true;

			DeselectTile();

			yield return new WaitForSeconds(0.5f);

			if (levelManager != null)
			{
				levelManager.ReshuffleTiles();
			}

			isProcessing = false;

			if (!HasAvailableMoves())
			{
				RestartLevel();
			}
		}

		private bool HasAvailableMoves()
		{
			var activeTiles = levelManager.AllTiles;
			var availableTiles = new List<TileInfo>();

			foreach (var tile in activeTiles)
			{
				if (!tile.controller.IsBlocked)
				{
					availableTiles.Add(tile);
				}
			}

			for (int i = 0; i < availableTiles.Count; i++)
			{
				for (int j = i + 1; j < availableTiles.Count; j++)
				{
					if (availableTiles[i].tileTypeID == availableTiles[j].tileTypeID)
					{
						return true; 
					}
				}
			}
			return false; 
		}

		public List<(TileInfo, TileInfo)> FindAvailablePairs()
		{
			var pairs = new List<(TileInfo, TileInfo)>();
			var activeTiles = levelManager.AllTiles;
			var availableTiles = new List<TileInfo>();

			foreach (var tile in activeTiles)
			{
				if (!tile.controller.IsBlocked)
				{
					availableTiles.Add(tile);
				}
			}

			for (int i = 0; i < availableTiles.Count; i++)
			{
				for (int j = i + 1; j < availableTiles.Count; j++)
				{
					if (availableTiles[i].tileTypeID == availableTiles[j].tileTypeID)
					{
						pairs.Add((availableTiles[i], availableTiles[j]));
					}
				}
			}

			return pairs;
		}

		private void ToggleAutoPlay()
		{
			if (isAutoPlaying)
			{
				StopAutoPlay();
			}
			else
			{
				StartAutoPlay();
			}
		}

		private void StartAutoPlay()
		{
			if (autoPlayCoroutine != null)
				StopCoroutine(autoPlayCoroutine);

			isAutoPlaying = true;
			autoPlayCoroutine = StartCoroutine(AutoPlayRoutine());
		}

		private void StopAutoPlay()
		{
			if (autoPlayCoroutine != null)
			{
				StopCoroutine(autoPlayCoroutine);
				autoPlayCoroutine = null;
			}

			isAutoPlaying = false;

			if (uiController != null)
				uiController.SetAutoPlayState(false);

		}

		private IEnumerator AutoPlayRoutine()
		{
			while (isAutoPlaying)
			{
				while (isProcessing)
				{
					yield return null;
				}

				var pairs = FindAvailablePairs();

				if (pairs.Count > 0)
				{
					var randomPair = pairs[Random.Range(0, pairs.Count)];

					SelectTile(randomPair.Item1);
					yield return new WaitForSeconds(autoPlayDelay * 0.5f);

					ProcessTileSelection(randomPair.Item2);

					yield return new WaitForSeconds(autoPlayDelay);
				}
				else
				{
					yield return new WaitForSeconds(1f);
				}
			}
		}
	}
}
