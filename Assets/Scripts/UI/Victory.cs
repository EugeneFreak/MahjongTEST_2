using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MahjongGame.UI
{
	public class Victory : MonoBehaviour
	{
		[Header("UI элементы")]
		[SerializeField] private GameObject victoryPanel;
		[SerializeField] private TextMeshProUGUI titleText;
		[SerializeField] private Button playAgainButton;

		[Header("Настройки")]
		[SerializeField] private float showDelay = 0.5f;

		public System.Action OnPlayAgainClicked;

		private void Awake()
		{
			if (victoryPanel == null)
				victoryPanel = gameObject;

			if (playAgainButton != null)
			{
				playAgainButton.onClick.AddListener(() =>
				{
					OnPlayAgainClicked?.Invoke();
				});
			}

			Hide();
		}

		public void Show()
		{
			if (titleText != null)
				titleText.text = "Победа!";

			if (victoryPanel != null)
			{
				victoryPanel.SetActive(true);

				CanvasGroup canvasGroup = victoryPanel.GetComponent<CanvasGroup>();
				if (canvasGroup == null)
				canvasGroup = victoryPanel.AddComponent<CanvasGroup>();

				canvasGroup.alpha = 0;

				StartCoroutine(ShowWithDelay(canvasGroup));
				
			}
		}

		private System.Collections.IEnumerator ShowWithDelay(CanvasGroup canvasGroup)
		{
			yield return new WaitForSeconds(showDelay);

			float elapsed = 0;
			float fadeTime = 0.3f;

			while (elapsed < fadeTime)
			{
				elapsed += Time.deltaTime;
				canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeTime);
				yield return null;
			}

			canvasGroup.alpha = 1;
		}

		public void Hide()
		{
			if (victoryPanel != null)
				victoryPanel.SetActive(false);
		}
	}
}
