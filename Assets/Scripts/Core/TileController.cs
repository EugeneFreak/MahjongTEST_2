using UnityEngine;
using UnityEngine.UI;

namespace MahjongGame.Core
{
	public class TileController : MonoBehaviour
	{
		[Header("Компоненты")]
		[SerializeField] private Image backgroundImage;
		[SerializeField] private Image iconImage;
		[SerializeField] private Button button;
		[SerializeField] private CanvasGroup canvasGroup;

		[Header("Визуальные настройки")]
		[SerializeField] private Color normalColor = Color.white;
		[SerializeField] private Color blockedColor = new Color(0.6f, 0.6f, 0.6f);
		[SerializeField] private Color selectedColor = new Color(1f, 1f, 0.5f);

		private int tileTypeID;
		private bool isBlocked = false;
		private bool isSelected = false;
		private Vector2Int gridPosition;
		private int layerIndex;

		public bool IsBlocked => isBlocked;
		public int TileTypeID => tileTypeID;

		private void Awake()
		{
			if (backgroundImage == null)
				backgroundImage = GetComponent<Image>();

			if (button == null)
				button = GetComponent<Button>();

			if (canvasGroup == null)
				canvasGroup = GetComponent<CanvasGroup>();

			if (iconImage == null)
			{
				Transform iconTransform = transform.Find("Icon");
				if (iconTransform != null)
					iconImage = iconTransform.GetComponent<Image>();
			}
		}

		public void Initialize(int typeID, Sprite iconSprite, Vector2Int gridPos, int layer)
		{
			tileTypeID = typeID;
			gridPosition = gridPos;
			layerIndex = layer;

			if (iconImage != null && iconSprite != null)
			{
				iconImage.sprite = iconSprite;
			}

			UpdateVisualState();
		}

		public void SetBlocked(bool blocked)
		{
			isBlocked = blocked;
			UpdateVisualState();
		}

		public void SetSelected(bool selected)
		{
			isSelected = selected;
			UpdateVisualState();
		}

		private void UpdateVisualState()
		{
			if (backgroundImage != null)
			{
				if (isSelected)
				{
					backgroundImage.color = selectedColor;
				}
				else if (isBlocked)
				{
					backgroundImage.color = blockedColor;
				}
				else
				{
					backgroundImage.color = normalColor;
				}
			}

			if (button != null)
			{
				button.interactable = !isBlocked;
			}

			if (canvasGroup != null)
			{
				canvasGroup.alpha = isBlocked ? 0.6f : 1f;
			}

			if (iconImage != null)
			{
				Color iconColor = iconImage.color;
				iconColor.a = (isBlocked && !isSelected) ? 0.5f : 1f;
				iconImage.color = iconColor;
			}
		}

		public Image GetIconImage()
		{
			return iconImage;
		}

		public void RemoveTile()
		{
			gameObject.SetActive(false);
		}
	}
}