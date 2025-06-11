using UnityEngine;
using System.Collections.Generic;

namespace MahjongGame.Core
{
	public class SpritePreloader : MonoBehaviour
	{
		[Header("Спрайты для принудительного включения в билд")]
		[SerializeField] private List<Sprite> tileSprites = new List<Sprite>();

		private static SpritePreloader instance;
		public static SpritePreloader Instance => instance;

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
			}
		}
	}
}