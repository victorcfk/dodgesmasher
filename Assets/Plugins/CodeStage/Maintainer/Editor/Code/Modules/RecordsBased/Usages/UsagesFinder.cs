#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.Usages
{
	/// <summary>
	/// Allows to find usages of specific objects in your project (i.e. searches for references).
	/// </summary>
	public class UsagesFinder
	{
		internal const string MODULE_NAME = "Usages Finder";

		private const string PROGRESS_CAPTION = MODULE_NAME + ": checking asset {0} of {1}";

		private static int itemsCount;
		private static int currentItem;

		private static string returnToScene;



		/// <summary>
		/// Starts usages search with current settings.
		/// </summary>
		/// <param name="showResults">Shows results in %Maintainer window if true.</param>
		/// <returns>Array of CleanerRecords in case you wish to manually iterate over them and make custom report.</returns>
		public static UsagesRecord[] StartSearch(ProjectAsset[] selectedAssets, bool showResults)
		{
			List<UsagesRecord> results = new List<UsagesRecord>();

			returnToScene = string.Empty;
			itemsCount = 0;
			currentItem = 0;

			/*if (MaintainerSettings.Cleaner.findEmptyFolders) phasesCount++;
			if (MaintainerSettings.Cleaner.findUnusedAssets) phasesCount++;*/

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			try
			{
				Stopwatch sw = Stopwatch.StartNew();

				bool searchCanceled = LookForUsages(selectedAssets, results);

				sw.Stop();

				EditorUtility.ClearProgressBar();

				if (!searchCanceled)
				{
					Debug.Log(Maintainer.LOG_PREFIX + MODULE_NAME + " results: " + results.Count +
							  " items found in " + sw.Elapsed.TotalSeconds.ToString("0.000") +
							  " seconds.");
				}
				else
				{
					Debug.Log(Maintainer.LOG_PREFIX + "Search canceled by user!");
				}

			}
			catch (Exception e)
			{
				Debug.Log(e);
				EditorUtility.ClearProgressBar();
			}

			SearchResultsStorage.UsagesSearchResults = results.ToArray();
			//if (showResults) MaintainerWindow.ShowUsages();

			return results.ToArray();
		}

		private static bool LookForUsages(ProjectAsset[] selectedAssets, List<UsagesRecord> results)
		{
			bool canceled = false;

			string[] allAssets = AssetDatabase.GetAllAssetPaths();
			//string[] allAssets = { @"Assets\Tests\Temp\111.unity" };
			//string[] allAssets = { @"Assets\Tests\Temp\IssuesSandbox 34.unity" };

			itemsCount = allAssets.Length;

			for (int i = 0; i < itemsCount; i++)
			{
				string assetFile = allAssets[i];
				if (Directory.Exists(assetFile)) continue;
				if (!File.Exists(assetFile)) continue;

				currentItem = i;

				if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentItem + 1, itemsCount),
					"Looking for usages at " + Path.GetFileName(assetFile), (float) (currentItem + 1) / itemsCount))
				{
					canceled = true;
					break;
				}

				string assetFileExtension = Path.GetExtension(assetFile);

				if (string.IsNullOrEmpty(assetFileExtension) || 
					assetFileExtension == ".dll") continue;

				if (assetFileExtension == ".unity")
				{
					canceled = ProcessScene(assetFile, selectedAssets, results);
				}
				else if (assetFileExtension == ".prefab")
				{
					canceled = ProcessPrefab(assetFile, selectedAssets, results);
				}

				/*Object[] unityObjects = AssetDatabase.LoadAllAssetsAtPath(assetFile);
				if (unityObjects != null && unityObjects.Length > 0)
				{
					//Debug.Log("Objects in asset: " + unityObjects.Length + "\n" + assetFile);
					for (int j = 0; j < unityObjects.Length; j++)
					{
						Object unityObject = unityObjects[j];
						for (int z = 0; z < selectedAssets.Length; i++)
						{
							if (unityObject == selectedAssets[z].unityObject)
							{
								Debug.Log("Match!" + selectedAssets[z]);
							}
						}
					}
				}
				Object[] rep = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetFile);

				Debug.Log(assetFile + "\nObjects: " + unityObjects.Length + " Representations: " + rep.Length);*/
			}

			/*for (int z = 0; z < selectedAssets.Length; i++)
			{
				ProjectAsset item = selectedAssets[z];
				Debug.Log("Looking for usages of " + item);
			}*/

			return canceled;
		}

		private static bool ProcessScene(string scenePath, ProjectAsset[] selectedAssets, List<UsagesRecord> results)
		{
			bool canceled = false;

			if (string.IsNullOrEmpty(returnToScene))
			{
				returnToScene = CSSceneTools.GetCurrentScenePath();

				if (!CSSceneTools.SaveCurrentSceneIfUserWantsTo())
				{
					Debug.Log(Maintainer.LOG_PREFIX + "Usages search canceled by user!");
					return true;
				}

				if (CSSceneTools.CurrentSceneIsDirty()) CSSceneTools.NewScene(true);
			}

			string sceneName = Path.GetFileNameWithoutExtension(scenePath);

			if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentItem + 1, itemsCount),
					"Opening scene " + sceneName, (float)(currentItem + 1) / itemsCount))
			{
				return true;
			}

			//Debug.Log(CSSceneTools.GetCurrentScenePath());
			//Debug.Log(scenePath);

			if (CSSceneTools.GetCurrentScenePath() != scenePath)
			{
				CSSceneTools.OpenScene(scenePath);
			}
#if UNITY_5_3_PLUS
			// if we're scanning currently opened scene and going to scan more scenes,
			// we need to close all additional scenes to avoid duplicates
			else if (EditorSceneManager.loadedSceneCount > 1)
			{
				CSSceneTools.CloseAllScenesButActive();
			}
#endif

			GameObject[] gameObjects = CSEditorTools.GetAllSuitableGameObjectsInCurrentScene();
			int objectsCount = gameObjects.Length;

			for (int i = 0; i < objectsCount; i++)
			{
				if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentItem + 1, itemsCount),
					"Checking objects in scene " + sceneName, (float)(i + 1) / objectsCount))
				{
					canceled = true;
					break;
				}

				ProcessObject(selectedAssets, results, scenePath, gameObjects[i], false);
			}

			return canceled;
		}

		private static bool ProcessPrefab(string prefabPath, ProjectAsset[] selectedAssets, List<UsagesRecord> results)
		{
			bool canceled = false;

			Object[] allAssetsInPrefab = AssetDatabase.LoadAllAssetsAtPath(prefabPath);
			int objectsCount = allAssetsInPrefab.Length;

			string prefabName = Path.GetFileNameWithoutExtension(prefabPath);

			for (int i = 0; i < objectsCount; i++)
			{
				if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentItem + 1, itemsCount),
					"Checking objects in prefab " + prefabName, (float)(currentItem + 1) / itemsCount))
				{
					canceled = true;
					break;
				}

				ProcessObject(selectedAssets, results, prefabPath, allAssetsInPrefab[i], true);
			}

			return canceled;
		}

		/// <summary>
		/// Looks for usages at the specified object.
		/// </summary>
		/// <param name="selectedAssets">Assets selected by user he wish to find usages of.</param>
		/// <param name="results">Found usages.</param>
		/// <param name="assetPath">Path to the scene or prefab.</param>
		/// <param name="unityObject">Objet to look in.</param>
		/// <param name="objectFromPrefab">Indicates precessed object is from prefab or scene.</param>
		private static void ProcessObject(ProjectAsset[] selectedAssets, List<UsagesRecord> results, string assetPath, Object unityObject, bool objectFromPrefab)
		{
			if (unityObject is GameObject)
			{
				var go = unityObject as GameObject;

				Component[] components = go.GetComponents<Component>();

				for (int i = 0; i < components.Length; i++)
				{
					Component component = components[i];
					if (component == null) continue;

					for (int j = 0; j < selectedAssets.Length; j++)
					{
						if (component == selectedAssets[j].unityObject)
						{
							Debug.Log("Found reference!!!\n" + assetPath + "\n" + unityObject.name);
						}
					}

					SerializedObject so = new SerializedObject(component);
					SerializedProperty sp = so.GetIterator();

					while (sp.NextVisible(true))
					{
						/*Debug.Log(sp.name + '\n' +
									sp.displayName);*/

						if (sp.propertyType == SerializedPropertyType.ObjectReference)
						{
							for (int j = 0; j < selectedAssets.Length; j++)
							{
								if (sp.objectReferenceValue == selectedAssets[j].unityObject)
								{
									Debug.Log("Found reference!!!\n" + assetPath + "\n" + unityObject.name);
								}
							}

						}
					}
				}

			}
				/*остановился тут
				throw new NotImplementedException();*/
		}

		private struct LookupItem
		{
			public string path;
			public Object[] unityObject;
		}
	}
}

#endif