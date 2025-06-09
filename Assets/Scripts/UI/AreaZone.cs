using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaZone : MonoBehaviour
{
	[Header("Настройки")]
	[SerializeField] private float targetAspectRatio = 1.0f;
	[SerializeField] private bool fitInParent = true;

	private RectTransform rectTransform;
	private RectTransform parentRectTransform;

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		if (transform.parent != null)
		{
			parentRectTransform = transform.parent.GetComponent<RectTransform>();
		}
	}

	private void Start()
	{
		UpdateAspectRatio();
	}

	private void Update()
	{
#if UNITY_EDITOR
		UpdateAspectRatio();
#endif
	}

	public void UpdateAspectRatio()
	{
		if (parentRectTransform == null) return;

		float parentWidth = parentRectTransform.rect.width;
		float parentHeight = parentRectTransform.rect.height;

		if (parentWidth <= 0 || parentHeight <= 0) return;

		float width, height;

		if (fitInParent)
		{
			float parentAspect = parentWidth / parentHeight;

			if (parentAspect > targetAspectRatio)
			{
				height = parentHeight;
				width = height * targetAspectRatio;
			}
			else
			{
				width = parentWidth;
				height = width / targetAspectRatio;
			}
		}
		else
		{
			width = parentWidth;
			height = width / targetAspectRatio;
		}

		rectTransform.sizeDelta = new Vector2(width, height);

		rectTransform.anchoredPosition = Vector2.zero;
	}

	public void SetAspectRatio(float ratio)
	{
		targetAspectRatio = ratio;
		UpdateAspectRatio();
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		if (Application.isPlaying && rectTransform != null)
		{
			UpdateAspectRatio();
		}
	}
#endif
}

