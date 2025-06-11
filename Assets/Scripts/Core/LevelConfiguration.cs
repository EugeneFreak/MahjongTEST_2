using UnityEngine;
using System.Collections.Generic;

namespace MahjongGame.Core
{
	[CreateAssetMenu(fileName = "LevelConfig", menuName = "Mahjong/Level Config")]
	public class LevelConfiguration : ScriptableObject
	{
		[Header("Слои уровня")]
		[SerializeField] private int layers = 3;

		[Header("Префабы плиток")]
		[SerializeField] private List<GameObject> tilePrefabs = new List<GameObject>();

		[Header("Настройки генерации")]
		[SerializeField] private int minPairsPerType = 2;
		[SerializeField] private bool ensureSolvable = true;

		public int Layers => layers;
		public List<GameObject> TilePrefabs => tilePrefabs;
		public int MinPairsPerType => minPairsPerType;
		public bool EnsureSolvable => ensureSolvable;
	}
}
