using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MahjongGame.UI
{

	public class UIController : MonoBehaviour
	{
		[Header("Кнопки управления")]
		[SerializeField] private Button restartButton; 
		[SerializeField] private Button autoPlayButton; 

		[Header("Панели")]
		[SerializeField] private GameObject buttonPanel; 
		[SerializeField] private GameObject victoryPanel;
		[SerializeField] private GameObject selectedTilePanel;

		[Header("Отображение выбранной плитки")]
		[SerializeField] private Image selectedTileIcon;

		[Header("Текст автоигры")]
		[SerializeField] private TextMeshProUGUI autoPlayButtonText;

		public event Action OnRestartRequested; 
		public event Action OnAutoPlayToggled;

		private bool isAutoPlayActive = false;

		private void Awake()
		{
			if (restartButton == null)
			{
				Debug.LogError("RestartButton не назначен в UIController!");
			}

			if (autoPlayButton == null)
			{
				Debug.LogError("AutoPlayButton не назначен в UIController!");
			}

			if (autoPlayButtonText == null && autoPlayButton != null)
			{
				autoPlayButtonText = autoPlayButton.GetComponentInChildren<TextMeshProUGUI>();
			}

			if (selectedTilePanel == null)
			{
				Debug.LogWarning("SelectedTilePanel не назначен в UIController!");
			}
		}

		private void Start()
		{
			if (restartButton != null)
			{
				restartButton.onClick.AddListener(HandleRestartClick);
			}

			if (autoPlayButton != null)
			{
				autoPlayButton.onClick.AddListener(HandleAutoPlayClick);
			}

			UpdateAutoPlayButton();

			HideSelectedTile();
		}

		private void OnDestroy()
		{
			if (restartButton != null)
			{
				restartButton.onClick.RemoveListener(HandleRestartClick);
			}

			if (autoPlayButton != null)
			{
				autoPlayButton.onClick.RemoveListener(HandleAutoPlayClick);
			}
		}

		private void HandleRestartClick()
		{
			Debug.Log("Кнопка 'Пересобрать' нажата");

			if (isAutoPlayActive)
			{
				isAutoPlayActive = false;
				UpdateAutoPlayButton();
			}

			OnRestartRequested?.Invoke();
		}

		private void HandleAutoPlayClick()
		{
			Debug.Log("Кнопка 'Автоигра' нажата");

			isAutoPlayActive = !isAutoPlayActive;
			UpdateAutoPlayButton();

			OnAutoPlayToggled?.Invoke();
		}

		private void UpdateAutoPlayButton()
		{
			if (autoPlayButtonText != null)
			{
				autoPlayButtonText.text = isAutoPlayActive ? "Стоп" : "Автоигра";
			}

			if (autoPlayButton != null)
			{
				ColorBlock colors = autoPlayButton.colors;
				if (isAutoPlayActive)
				{
					colors.normalColor = new Color(200f / 255f, 50f / 255f, 50f / 255f);
					colors.highlightedColor = new Color(220f / 255f, 70f / 255f, 70f / 255f);
					colors.pressedColor = new Color(180f / 255f, 30f / 255f, 30f / 255f);
				}
				
				autoPlayButton.colors = colors;
			}
		}

		public bool IsAutoPlayActive => isAutoPlayActive;

		public void SetAutoPlayState(bool active)
		{
			isAutoPlayActive = active;
			UpdateAutoPlayButton();
		}

		public void SetButtonPanelActive(bool active)
		{
			if (buttonPanel != null)
			{
				buttonPanel.SetActive(active);
			}
		}

		public void SetButtonsInteractable(bool interactable)
		{
			if (restartButton != null)
				restartButton.interactable = interactable;

			if (autoPlayButton != null)
				autoPlayButton.interactable = interactable;
		}

		public void ShowSelectedTile(Sprite tileSprite)
		{
			if (selectedTilePanel != null)
			{
				selectedTilePanel.SetActive(true);
			}

			if (selectedTileIcon != null)
			{
				selectedTileIcon.sprite = tileSprite;
				selectedTileIcon.enabled = true;
			}
		}

		public void HideSelectedTile()
		{
			if (selectedTilePanel != null)
			{
				selectedTilePanel.SetActive(false);
			}

			if (selectedTileIcon != null)
			{
				selectedTileIcon.sprite = null;
				selectedTileIcon.enabled = false;
			}
		}
	}
}