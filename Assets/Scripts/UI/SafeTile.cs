using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeTile : MonoBehaviour
{
	[RequireComponent(typeof(RectTransform))]
	public class SafeAreaHelper : MonoBehaviour
	{
		private RectTransform rectTransform;

		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
			SetupFullScreen();
		}

		private void SetupFullScreen()
		{
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;
		}
	}
}
