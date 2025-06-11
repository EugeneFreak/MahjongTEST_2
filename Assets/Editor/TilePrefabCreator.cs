using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace MahjongGame.Editor
{
	public class TilePrefabCreator : EditorWindow
	{
		private GameObject tilePrefabTemplate;
		private Sprite[] tileSprites = new Sprite[10];
		private string[] spriteNames = { "acorn", "amanita", "apple", "asparagus", "avocado",
										"bananas", "becone", "beer", "beet", "blueberry" };

		[MenuItem("Tools/Mahjong/Create Tile Prefabs")]
		public static void ShowWindow()
		{
			GetWindow<TilePrefabCreator>("Tile Prefab Creator");
		}

		private void OnGUI()
		{
			GUILayout.Label("Tile Prefab Creator", EditorStyles.boldLabel);

			tilePrefabTemplate = EditorGUILayout.ObjectField("Base Tile Template", tilePrefabTemplate, typeof(GameObject), false) as GameObject;

			GUILayout.Space(10);
			GUILayout.Label("Tile Sprites:", EditorStyles.boldLabel);

			for (int i = 0; i < tileSprites.Length; i++)
			{
				tileSprites[i] = EditorGUILayout.ObjectField($"{spriteNames[i]}:", tileSprites[i], typeof(Sprite), false) as Sprite;
			}

			GUILayout.Space(20);

			if (GUILayout.Button("Create All Tile Prefabs", GUILayout.Height(30)))
			{
				CreateTilePrefabs();
			}

			GUILayout.Space(10);

			if (GUILayout.Button("Create Base Tile Template", GUILayout.Height(25)))
			{
				CreateBaseTileTemplate();
			}
		}

		private void CreateBaseTileTemplate()
		{
			// Создаем базовый шаблон тайла
			GameObject tileBase = new GameObject("Tile_Base");

			// Добавляем RectTransform
			RectTransform rectTransform = tileBase.AddComponent<RectTransform>();
			rectTransform.sizeDelta = new Vector2(120, 120);

			// Добавляем основные компоненты
			Image backgroundImage = tileBase.AddComponent<Image>();
			backgroundImage.color = Color.white;

			Button button = tileBase.AddComponent<Button>();
			CanvasGroup canvasGroup = tileBase.AddComponent<CanvasGroup>();

			// Создаем дочерний объект для иконки
			GameObject iconObject = new GameObject("Icon");
			iconObject.transform.SetParent(tileBase.transform);

			RectTransform iconRect = iconObject.AddComponent<RectTransform>();
			iconRect.anchorMin = Vector2.zero;
			iconRect.anchorMax = Vector2.one;
			iconRect.sizeDelta = Vector2.zero;
			iconRect.anchoredPosition = Vector2.zero;

			Image iconImage = iconObject.AddComponent<Image>();
			iconImage.preserveAspect = true;

			// Добавляем TileController
			var tileController = tileBase.AddComponent<MahjongGame.Core.TileController>();

			// Сохраняем как префаб
			string path = "Assets/Prefabs/Tiles/Tile_Base.prefab";
			EnsureDirectoryExists("Assets/Prefabs/Tiles");

			PrefabUtility.SaveAsPrefabAsset(tileBase, path);
			DestroyImmediate(tileBase);

			tilePrefabTemplate = AssetDatabase.LoadAssetAtPath<GameObject>(path);

			Debug.Log($"Created base tile template at {path}");
		}

		private void CreateTilePrefabs()
		{
			if (tilePrefabTemplate == null)
			{
				EditorUtility.DisplayDialog("Error", "Please assign a base tile template first!", "OK");
				return;
			}

			EnsureDirectoryExists("Assets/Resources/Prefabs/Tiles");

			for (int i = 0; i < tileSprites.Length; i++)
			{
				if (tileSprites[i] == null)
				{
					Debug.LogWarning($"Sprite for {spriteNames[i]} is missing, skipping...");
					continue;
				}

				// Создаем экземпляр префаба
				GameObject tileInstance = PrefabUtility.InstantiatePrefab(tilePrefabTemplate) as GameObject;

				// Находим компонент иконки
				Transform iconTransform = tileInstance.transform.Find("Icon");
				if (iconTransform != null)
				{
					Image iconImage = iconTransform.GetComponent<Image>();
					if (iconImage != null)
					{
						iconImage.sprite = tileSprites[i];
					}
				}

				// Сохраняем как новый префаб
				string prefabPath = $"Assets/Resources/Prefabs/Tiles/Tile_{spriteNames[i]}.prefab";
				PrefabUtility.SaveAsPrefabAsset(tileInstance, prefabPath);

				DestroyImmediate(tileInstance);

				Debug.Log($"Created prefab: {prefabPath}");
			}

			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Success", "Tile prefabs created successfully!", "OK");
		}

		private void EnsureDirectoryExists(string path)
		{
			string[] folders = path.Split('/');
			string currentPath = folders[0];

			for (int i = 1; i < folders.Length; i++)
			{
				string nextPath = currentPath + "/" + folders[i];
				if (!AssetDatabase.IsValidFolder(nextPath))
				{
					AssetDatabase.CreateFolder(currentPath, folders[i]);
				}
				currentPath = nextPath;
			}
		}
	}
}