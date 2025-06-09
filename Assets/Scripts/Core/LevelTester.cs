using UnityEngine;

namespace MahjongGame.Core
{

	public class LevelTester : MonoBehaviour
	{
		[SerializeField] private LevelManager levelManager;

		private void Start()
		{
			if (levelManager == null)
			{
				levelManager = FindObjectOfType<LevelManager>();
			}

			if (levelManager != null)
			{
				Debug.Log("Генерируем тестовый уровень...");
				levelManager.GenerateLevel();
			}
			else
			{
				Debug.LogError("LevelManager не найден!");
			}
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				Debug.Log("Перегенерация уровня...");
				levelManager.GenerateLevel();
			}
		}
	}
}
