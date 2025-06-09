using UnityEngine;
using UnityEngine.UI;

namespace MahjongGame.Core
{

	public class TileController : MonoBehaviour
	{
		[Header("Компоненты")]
		[SerializeField] private Button tileButton; 
		[SerializeField] private Image backgroundImage; 
		[SerializeField] private Image iconImage; 
		[SerializeField] private CanvasGroup canvasGroup; 

		[Header("Настройки визуала")]
		[SerializeField] private Color normalColor = Color.white; 
		[SerializeField] private Color blockedColor = new Color(0.6f, 0.6f, 0.6f, 1f); // Цвет заблокированной плитки
		[SerializeField] private Color selectedColor = new Color(1f, 1f, 0.5f, 1f); // Цвет выбранной плитки

		private int tileID; 
		private bool isBlocked = false; 
		private bool isSelected = false; 
		private Vector2Int gridPosition; 
		private int layerIndex; 

		public int TileID => tileID;

		public Vector2Int GridPosition => gridPosition;

		public int LayerIndex => layerIndex;

		public bool IsBlocked => isBlocked;

	
		public bool IsSelected => isSelected;

		private void Awake()
		{
			if (tileButton == null) tileButton = GetComponent<Button>();
			if (backgroundImage == null) backgroundImage = GetComponent<Image>();
			if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

			if (iconImage == null)
			{
				Transform iconTransform = transform.Find("Icon");
				if (iconTransform != null)
					iconImage = iconTransform.GetComponent<Image>();
			}
		}

		public void Initialize(int id, Sprite iconSprite, Vector2Int position, int layer)
		{
			tileID = id;
			gridPosition = position;
			layerIndex = layer;

			if (iconImage != null && iconSprite != null)
			{
				iconImage.sprite = iconSprite;
			}

			SetSelected(false);
			UpdateVisualState();
		}

		public void SetBlocked(bool blocked)
		{
			isBlocked = blocked;

			tileButton.interactable = !blocked;

			UpdateVisualState();
		}

		public void SetSelected(bool selected)
		{
			isSelected = selected;
			UpdateVisualState();
		}
		private void UpdateVisualState()
		{
			if (isBlocked)
			{
				backgroundImage.color = blockedColor;
				canvasGroup.alpha = 0.7f;
			}
			else if (isSelected)
			{
				backgroundImage.color = selectedColor;
				canvasGroup.alpha = 1f;
			}
			else
			{
				backgroundImage.color = normalColor;
				canvasGroup.alpha = 1f;
			}
		}

		public void RemoveTile()
		{
			gameObject.SetActive(false);
		}

		public bool IsMatchWith(TileController other)
		{
			return other != null && other.tileID == this.tileID && other != this;
		}
	}
}
