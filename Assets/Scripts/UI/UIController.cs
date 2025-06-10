using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace MahjongGame.UI
{
	public class UIController : MonoBehaviour
	{
		[Header("Кнопки")]
		[SerializeField] private Button restartButton; 
		[SerializeField] private Button autoPlayButton; 

		[Header("Экраны")]
		[SerializeField] private Victory victoryScreen; 

		[Header("Панели")]
		[SerializeField] private GameObject buttonPanel; 
		[SerializeField] private GameObject victoryPanel; 
		[SerializeField] private GameObject selectedTilePanel;

		private Image selectedTileIcon; 

		[Header("Текста")]
		[SerializeField] private TextMeshProUGUI autoPlayButtonText;

		public event Action OnRestartRequested; 
		public event Action OnAutoPlayToggled; 

		private bool isAutoPlayActive = false; 

		private void Awake()
		{
			if (autoPlayButtonText == null && autoPlayButton != null)
			{
				autoPlayButtonText = autoPlayButton.GetComponentInChildren<TextMeshProUGUI>();
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
			if (isAutoPlayActive)
			{
				isAutoPlayActive = false;
				UpdateAutoPlayButton();
			}
			OnRestartRequested?.Invoke();
		}

		private void HandleAutoPlayClick()
		{
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

		public void SetAutoPlayState(bool active)
		{
			isAutoPlayActive = active;
			UpdateAutoPlayButton();
		}

		public void ShowSelectedTile(Sprite tileSprite)
		{
			if (selectedTileIcon != null)
			{
				selectedTileIcon.sprite = tileSprite;
				selectedTileIcon.enabled = true;
			}
		}

		public void HideSelectedTile()
		{
			if (selectedTileIcon != null)
			{
				selectedTileIcon.sprite = null;
				selectedTileIcon.enabled = false;
			}
		}
	}
}