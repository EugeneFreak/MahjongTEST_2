using UnityEngine;

namespace MahjongGame.Core
{
	public class GameArea : MonoBehaviour
	{
		
		private float maxScale = 1f; 
		private float minScale = 0.5f; 
		private float padding = 50f; 

		private float fieldWidth = 1200f; 
		private float fieldHeight = 800f; 

		private RectTransform rectTransform;
		private float lastAspectRatio;

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		private void Start()
		{
			AdaptToScreen();
		}

		private void Update()
		{
			float currentAspectRatio = (float)Screen.width / Screen.height;
			if (Mathf.Abs(currentAspectRatio - lastAspectRatio) > 0.01f)
			{
				AdaptToScreen();
				lastAspectRatio = currentAspectRatio;
			}
		}

		private void AdaptToScreen()
		{
			if (rectTransform == null) return;

			RectTransform parentRect = rectTransform.parent as RectTransform;
			if (parentRect == null) return;

			float availableWidth = parentRect.rect.width - padding * 2;
			float availableHeight = parentRect.rect.height - padding * 2;

			float scaleX = availableWidth / fieldWidth;
			float scaleY = availableHeight / fieldHeight;
			float scale = Mathf.Min(scaleX, scaleY);

			scale = Mathf.Clamp(scale, minScale, maxScale);

			rectTransform.localScale = Vector3.one * scale;
		}
	}
}
