#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.Cleaner
{
	/// <summary>
	/// Allows to find and clean garbage in your Unity project. See readme for details.
	/// </summary>
	public class ProjectCleaner
	{
		internal const string MODULE_NAME = "Project Cleaner";
		
		private const string PROGRESS_CAPTION = MODULE_NAME + ": phase {0} of {1}, item {2} of {3}";

		private static int phasesCount;
		private static int currentPhase;

		private static int folderIndex;
		private static int foldersCount;

		private static int itemsToClean;

		private static long cleanedBytes;

		/// <summary>
		/// Starts garbage search and generates report.
		/// </summary>
		/// <returns>Project Cleaner report, similar to the exported report from the %Maintainer window.</returns>
		/// %Maintainer window is not shown.
		/// <br/>Useful when you wish to integrate %Maintainer in your build pipeline.
		public static string SearchAndReport()
		{
			CleanerRecord[] foundGarbage = StartSearch(false);

			// ReSharper disable once CoVariantArrayConversion
			return ReportsBuilder.GenerateReport(MODULE_NAME, foundGarbage);
		}

		/// <summary>
		/// Starts garbage search, cleans what was found with optional confirmation and 
		/// generates report to let you know what were cleaned up.
		/// </summary>
		/// <param name="showConfirmation">Enables or disables confirmation dialog about cleaning up found stuff.</param>
		/// <returns>Project Cleaner report about removed items.</returns>
		/// %Maintainer window is not shown.
		/// <br/>Useful when you wish to integrate %Maintainer in your build pipeline.
		public static string SearchAndCleanAndReport(bool showConfirmation = true)
		{
			CleanerRecord[] foundGarbage = StartSearch(false);
			CleanerRecord[] cleanedGarbage = StartClean(foundGarbage, false, showConfirmation);

			string header = "Total cleaned bytes: " + CSEditorTools.FormatBytes(cleanedBytes);
			header += "\nFollowing items were cleaned up:";

			// ReSharper disable once CoVariantArrayConversion
			return ReportsBuilder.GenerateReport(MODULE_NAME, cleanedGarbage, header);
		}

		/// <summary>
		/// Starts garbage search with current settings.
		/// </summary>
		/// <param name="showResults">Shows results in %Maintainer window if true.</param>
		/// <returns>Array of CleanerRecords in case you wish to manually iterate over them and make custom report.</returns>
		public static CleanerRecord[] StartSearch(bool showResults)
		{
			var results = new List<CleanerRecord>();

			phasesCount = 0;
			currentPhase = 0;

			if (MaintainerSettings.Cleaner.findEmptyFolders) phasesCount++;
			if (MaintainerSettings.Cleaner.findUnusedAssets) phasesCount++;

			bool searchCanceled = false;

			AssetDatabase.SaveAssets();

			try
			{
				Stopwatch sw = Stopwatch.StartNew();

				if (MaintainerSettings.Cleaner.findEmptyFolders)
				{
					searchCanceled = ScanFolders(results);
				}

				if (!searchCanceled && MaintainerSettings.Cleaner.findUnusedAssets)
				{
					searchCanceled = ScanProjectFiles(results);
				}

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

			SearchResultsStorage.CleanerSearchResults = results.ToArray();
			if (showResults) MaintainerWindow.ShowCleaner();

			return results.ToArray();
		}

		/// <summary>
		/// Starts clean of the garbage found with StartSearch() method.
		/// </summary>
		/// <param name="recordsToClean">Pass records you wish to clean here or leave null to let it load last search results.</param>
		/// <param name="showResults">Shows results in the %Maintainer window if true.</param>
		/// <param name="showConfirmation">Shows confirmation dialog before performing cleanup if true.</param>
		/// <returns>Array of CleanRecords which were cleaned up.</returns>
		public static CleanerRecord[] StartClean(CleanerRecord[] recordsToClean = null, bool showResults = true, bool showConfirmation = true)
		{
			CleanerRecord[] records = recordsToClean;
			if (records == null)
			{
				records = SearchResultsStorage.CleanerSearchResults;
			}
			
			if (records.Length == 0)
			{
				return null;
			}

			cleanedBytes = 0;
			itemsToClean = 0;

			foreach (var record in records)
			{
				if (record.selected) itemsToClean++;
			}

			if (itemsToClean == 0)
			{
				EditorUtility.DisplayDialog(MODULE_NAME, "Please select items to clean up!", "Ok");
				return null;
			}

			if (!showConfirmation || EditorUtility.DisplayDialog("Confirmation", "Do you really wish to delete " + itemsToClean + " items?\n" + Maintainer.DATA_LOSS_WARNING, "Go for it!", "Cancel"))
			{
				Stopwatch sw = Stopwatch.StartNew();

				bool cleanCanceled = CleanRecords(records);

				List<CleanerRecord> cleanedRecords = new List<CleanerRecord>(records.Length);
				List<CleanerRecord> notCleanedRecords = new List<CleanerRecord>(records.Length);

				foreach (var record in records)
				{
					if (record.cleaned)
					{
						cleanedRecords.Add(record);
					}
					else
					{
						notCleanedRecords.Add(record);
					}
				}

				records = notCleanedRecords.ToArray();

				sw.Stop();

				EditorUtility.ClearProgressBar();

				if (!cleanCanceled)
				{
					Debug.Log(Maintainer.LOG_PREFIX + MODULE_NAME + " results: " + cleanedRecords.Count +
						" items (" + CSEditorTools.FormatBytes(cleanedBytes) + " in size) cleaned in " + sw.Elapsed.TotalSeconds.ToString("0.000") +
						" seconds.");
				}
				else
				{
					Debug.Log(Maintainer.LOG_PREFIX + "Clean canceled by user!");
				}

				SearchResultsStorage.CleanerSearchResults = records;
				if (showResults) MaintainerWindow.ShowCleaner();

				return cleanedRecords.ToArray();
			}

			return null;
		}

		[DidReloadScripts]
		private static void AutoCleanFolders()
		{
			if (!MaintainerSettings.Cleaner.findEmptyFolders || !MaintainerSettings.Cleaner.findEmptyFoldersAutomatically) return;

			List<CleanerRecord> results = new List<CleanerRecord>();
			ScanFolders(results, false);

			if (results.Count > 0)
			{
				int result = EditorUtility.DisplayDialogComplex("Maintainer", MODULE_NAME + " found " + results.Count + " empty folders. Do you wish to remove them?\n" + Maintainer.DATA_LOSS_WARNING, "Yes", "No", "Show in Maintainer");
				if (result == 0)
				{
					CleanerRecord[] records = results.ToArray();
					CleanRecords(records, false);
					Debug.Log(Maintainer.LOG_PREFIX + results.Count + " empty folders cleaned.");
				}
				else if (result == 2)
				{
					SearchResultsStorage.CleanerSearchResults = results.ToArray();
					MaintainerWindow.ShowCleaner(); 
				}
			}
		}

		private static bool ScanFolders(ICollection<CleanerRecord> results, bool showProgress = true)
		{
			bool canceled;
			currentPhase++;

			folderIndex = 0;

			List<string> emptyFolders = new List<string>();
			string root = Application.dataPath;

			foldersCount = Directory.GetDirectories(root, "*", SearchOption.AllDirectories).Length;
			FindEmptyFoldersRecursive(emptyFolders, root, showProgress, out canceled);

			ExcludeSubFoldersOfEmptyFolders(ref emptyFolders);

			foreach (string folder in emptyFolders)
			{
				AssetRecord newRecord = AssetRecord.Create(RecordType.EmptyFolder, folder);
				if (newRecord != null) results.Add(newRecord);
			}

			return canceled;
		}

		private static bool ScanProjectFiles(ICollection<CleanerRecord> results, bool showProgress = true)
		{
			currentPhase++;

			List<string> ignoredScenes = new List<string>();

			if (MaintainerSettings.Cleaner.ignoreScenesInBuild)
			{
				ignoredScenes.AddRange(CSSceneTools.GetScenesInBuild(!MaintainerSettings.Cleaner.ignoreOnlyEnabledScenesInBuild));
			}

			CheckScenesForExistence(results, ignoredScenes, "Scenes in build");

			foreach (string scene in MaintainerSettings.Cleaner.sceneIgnores)
			{
				if (!ignoredScenes.Contains(scene))
				{
					ignoredScenes.Add(scene);
				}
			}

			CheckScenesForExistence(results, ignoredScenes, "Scene Ignores");

			if (ignoredScenes.Count == 0)
			{
				results.Add(CleanerErrorRecord.Create("No ignored scenes, terminating unused assets search!"));
				return false;
			}

			List<string> ignores = MaintainerSettings.Cleaner.pathIgnores.ToList();
			ignores.Add("Assets/Editor Default Resources");
			ignores.Add("/Resources/");
			ignores.Add("/StreamingAssets/");
			ignores.Add("/Gizmos/");
			ignores.Add("/Editor/");
			ignores.Add("/Plugins/");
			ignores.Add(".dll");
			ignores.Add(".rsp");
			ignores.AddRange(ignoredScenes);
			ignores.AddRange(CSEditorTools.FindAssetsFiltered("t:Script"));
			ignores.AddRange(GetAssetsUsedInPlayerSettings());

			List<string> assets = new List<string>();
			List<string> usedAssets = new List<string>();
			List<string> assetsWithDependencies = new List<string>();

			string[] allAssets = CSEditorTools.FindAssetsFiltered("t:Object t:GUISkin", null, ignores.ToArray());
			int count = allAssets.Length;

			for (int i = 0; i < count; i++)
			{
				if (showProgress && EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, i + 1, count), "Gathering all assets...", (float)i / count))
				{
					return true;
				}

				string asset = allAssets[i];

				if (File.Exists(asset))
				{
					if (assets.IndexOf(asset) == -1)
					{
						assets.Add(asset);
					}
				}
			}
			
			assetsWithDependencies.AddRange(ignoredScenes);

			List<string> foldersAddedToBuild = new List<string>();

			foldersAddedToBuild.AddRange(CSEditorTools.FindFoldersFiltered("Resources"));
			foldersAddedToBuild.AddRange(CSEditorTools.FindFoldersFiltered("StreamingAssets"));

			assetsWithDependencies.AddRange(CSEditorTools.FindAssetsInFolders("t:Object t:GUISkin", foldersAddedToBuild.ToArray()));

			count = assetsWithDependencies.Count;

			for (int i = 0; i < count; i++)
			{
				if (showProgress && EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, i + 1, count), "Looking for dependencies...", (float)i / count))
				{
					return true;
				}

				string[] dependencies = AssetDatabase.GetDependencies(new [] { assetsWithDependencies[i]});

				foreach (string dependency in dependencies)
				{
					if (!File.Exists(dependency)) continue;

					if (usedAssets.IndexOf(dependency) == -1) usedAssets.Add(dependency);
				}
			}

			if (ScanShadersForFallbacks(ref usedAssets, showProgress))
			{
				return true;
			}

			foreach (string usedAsset in usedAssets)
			{
				if (assets.IndexOf(usedAsset) != -1)
				{
					assets.Remove(usedAsset);
				}
			}

			foreach (string asset in assets)
			{
				results.Add(AssetRecord.Create(RecordType.UnusedAsset, asset));
			}

			return false;
		}

		private static IEnumerable<string> GetAssetsUsedInPlayerSettings()
		{
			var usedAssets = new List<string>();

			foreach (BuildTargetGroup buildTargetGroup in Enum.GetValues(typeof(BuildTargetGroup)))
			{
				Texture2D[] textures = PlayerSettings.GetIconsForTargetGroup(buildTargetGroup);
				if (textures != null && textures.Length > 0)
				{
					foreach (Texture2D texture2D in textures)
					{
						if (texture2D != null)
						{
							string path = AssetDatabase.GetAssetPath(texture2D);
							if (!string.IsNullOrEmpty(path) && path.StartsWith("Assets"))
							{
								if (usedAssets.IndexOf(path) == -1)
									usedAssets.Add(path);
							}
						}
					}
				}
			}

			Object[] projectSettingsAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
			if (projectSettingsAssets != null && projectSettingsAssets.Length > 0)
			{
				var projectSettingsSerialized = new SerializedObject(projectSettingsAssets[0]);

				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "defaultCursor");
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "resolutionDialogBanner");
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "iPhoneSplashScreen");
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "iPhoneHighResSplashScreen");
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "iPhoneTallHighResSplashScreen");
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "iPhone47inSplashScreen");
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "iPhone55inPortraitSplashScreen");
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "iPhone55inLandscapeSplashScreen");
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "iPadPortraitSplashScreen");
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "iPadHighResPortraitSplashScreen");
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "iPadLandscapeSplashScreen");
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "iPadHighResLandscapeSplashScreen");

#if UNITY_5_3_OR_NEWER
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "m_VirtualRealitySplashScreen");
#endif

#if UNITY_5_5_OR_NEWER
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "m_SplashScreenLogos", "logo");
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "splashScreenBackgroundSourceLandscape");
				GetPathsFromSerializedObject(projectSettingsSerialized, ref usedAssets, "splashScreenBackgroundSourcePortrait");
#endif
			}
			else
			{
				Debug.LogWarning(Maintainer.LOG_PREFIX + "Can't read the ProjectSettings! Please report to " + Maintainer.SUPPORT_EMAIL);
			}


			Object[] graphicsSettingsAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
			if (graphicsSettingsAssets != null && graphicsSettingsAssets.Length > 0)
			{
				var graphicsSettingsSerialized = new SerializedObject(graphicsSettingsAssets[0]);
				SerializedProperty alwaysIncludedShadersArray = graphicsSettingsSerialized.FindProperty("m_AlwaysIncludedShaders");
				if (alwaysIncludedShadersArray != null && alwaysIncludedShadersArray.isArray)
				{
					for (int i = 0; i < alwaysIncludedShadersArray.arraySize; i++)
					{
						SerializedProperty item = alwaysIncludedShadersArray.GetArrayElementAtIndex(i);
						if (item.propertyType == SerializedPropertyType.ObjectReference && item.objectReferenceInstanceIDValue != 0)
						{
							string path = AssetDatabase.GetAssetPath(item.objectReferenceInstanceIDValue);
							if (!string.IsNullOrEmpty(path) && path.StartsWith("Assets"))
							{
								if (usedAssets.IndexOf(path) == -1)
									usedAssets.Add(path);
							}
						}
					}
				}
				else
				{
					Debug.LogWarning(Maintainer.LOG_PREFIX + "Can't find the m_AlwaysIncludedShaders property! Please report to " + Maintainer.SUPPORT_EMAIL);
				}
			}
			else
			{
				Debug.LogWarning(Maintainer.LOG_PREFIX + "Can't read the GraphicsSettings! Please report to " + Maintainer.SUPPORT_EMAIL);
			}

			return usedAssets;
		}

		private static bool ScanShadersForFallbacks(ref List<string> usedAssets, bool showProgress = true)
		{
			int count = usedAssets.Count;

			for (int i = 0; i < count; i++)
			{
				string usedAsset = usedAssets[i];
				if (showProgress && EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, i + 1, count), "Looking for shader fallbacks...", (float)i / count))
				{
					return true;
				}

				if (usedAsset.EndsWith(".shader"))
				{
					//Debug.Log(usedAsset);

					ScanShaderForNestedFallbacks(ref usedAssets, usedAsset);
				}
			}

			return false;
		}

		private static void ScanShaderForNestedFallbacks(ref List<string> usedAssets, string usedAsset)
		{
			var shaderCode = new StringBuilder();

			using (StreamReader sr = File.OpenText(usedAsset))
			{
				string s;
				while ((s = sr.ReadLine()) != null)
				{
					shaderCode.AppendLine(s);
				}
			}

			string shaderCodeString = shaderCode.ToString();

			int lastIndex = 0;

			while (lastIndex != -1)
			{
				lastIndex = shaderCodeString.IndexOf("fallback", lastIndex + 1, StringComparison.CurrentCultureIgnoreCase);
				if (lastIndex != -1)
				{
					int firstQuoteIndex = shaderCodeString.IndexOf("\"", lastIndex, StringComparison.CurrentCultureIgnoreCase);
					if (firstQuoteIndex == -1) continue;

					string whiteSpace = shaderCodeString.Substring(lastIndex + 8, firstQuoteIndex - (lastIndex + 8));
					whiteSpace = whiteSpace.Trim();
					if (whiteSpace.Length != 0) continue;


					int lastQuoteIndex = shaderCodeString.IndexOf("\"", firstQuoteIndex + 1,
						StringComparison.CurrentCultureIgnoreCase);
					string fallbackName = shaderCodeString.Substring(firstQuoteIndex + 1, lastQuoteIndex - firstQuoteIndex - 1);
					Shader fallbackShader = Shader.Find(fallbackName);
					string fallbackShaderPath = AssetDatabase.GetAssetPath(fallbackShader);

					if (!fallbackShaderPath.StartsWith("Assets")) continue;

					if (usedAssets.IndexOf(fallbackShaderPath) == -1)
					{
						usedAssets.Add(fallbackShaderPath);
					}

					ScanShaderForNestedFallbacks(ref usedAssets, fallbackShaderPath);
				}
			}
		}

		private static void CheckScenesForExistence(ICollection<CleanerRecord> results, List<string> ignoredScenes, string where)
		{
			for (int i = ignoredScenes.Count - 1; i >= 0; i--)
			{
				string scenePath = ignoredScenes[i];
				if (!File.Exists(scenePath))
				{
					results.Add(CleanerErrorRecord.Create("Scene " + Path.GetFileName(scenePath) + " from " + where + " not found!"));
					ignoredScenes.RemoveAt(i);
				}
			}
		}

		private static void ExcludeSubFoldersOfEmptyFolders(ref List<string> emptyFolders)
		{
			List<string> emptyFoldersFiltered = new List<string>(emptyFolders.Count);
			for (int i = emptyFolders.Count-1; i >= 0; i--)
			{
				string folder = emptyFolders[i];
				if (!CSArrayTools.IsItemContainsAnyStringFromArray(folder, emptyFoldersFiltered))
				{
					emptyFoldersFiltered.Add(folder);
				}
			}
			emptyFolders = emptyFoldersFiltered;
		}

		private static bool FindEmptyFoldersRecursive(List<string> foundEmptyFolders, string root, bool showProgress, out bool canceledByUser)
		{
			string[] rootSubFolders = Directory.GetDirectories(root);

			bool canceled = false;
			bool emptySubFolders = true;
			foreach (string folder in rootSubFolders)
			{
				folderIndex++;

				if (showProgress && EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, folderIndex, foldersCount), "Scanning folders...", (float)folderIndex / foldersCount))
				{
					canceled = true;
					break;
				}

				if (CSArrayTools.IsItemContainsAnyStringFromArray(folder.Replace('\\', '/'), MaintainerSettings.Cleaner.pathIgnores))
				{
					emptySubFolders = false;
					continue;
				}

				/*if (Path.GetFileName(folder).StartsWith(".") )
				{
					continue;
				}*/

				emptySubFolders &= FindEmptyFoldersRecursive(foundEmptyFolders, folder, showProgress, out canceled);
				if (canceled) break;
			}

			if (canceled)
			{
				canceledByUser = true;
				return false;
			}

			bool rootFolderHasFiles = true;
			string[] filesInRootFolder = Directory.GetFiles(root);

			foreach (string file in filesInRootFolder)
			{
				if (file.EndsWith(".meta")) continue;

				rootFolderHasFiles = false;
				break;
			}

			bool rootFolderEmpty = emptySubFolders && rootFolderHasFiles;
			if (rootFolderEmpty)
			{
				foundEmptyFolders.Add(root);
			}

			canceledByUser = false;
			return rootFolderEmpty;
		}

		private static bool CleanRecords(IEnumerable<CleanerRecord> results, bool showProgress = true)
		{
			bool canceled = false;
			int i = 0;

			AssetDatabase.StartAssetEditing();

			foreach (CleanerRecord item in results)
			{
				if (showProgress && EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, 1, 1, i + 1, itemsToClean), "Cleaning selected items...", (float)i/itemsToClean))
				{
					canceled = true;
					break;
				}

				if (item.selected)
				{
					i++;
					if (item.Clean() && item is AssetRecord)
					{
						cleanedBytes += (item as AssetRecord).size;
					} 
				}
			}

			AssetDatabase.StopAssetEditing();

			AssetDatabase.Refresh();

			return canceled;
		}

		private static int GetPathsFromSerializedObject(SerializedObject so, ref List<string> paths, string propertyName, string arrayItemName = null)
		{
			int foundPaths = 0;

			SerializedProperty property = so.FindProperty(propertyName);

			if (string.IsNullOrEmpty(arrayItemName))
			{
				if (property != null && property.propertyType == SerializedPropertyType.ObjectReference)
				{
					int instanceId = property.objectReferenceInstanceIDValue;
					if (instanceId != 0)
					{
						string path = AssetDatabase.GetAssetPath(instanceId);
						if (!string.IsNullOrEmpty(path) && path.StartsWith("Assets"))
						{
							foundPaths++;
							if (paths.IndexOf(path) == -1)
								paths.Add(path);
						}
					}
				}
				else
				{
					Debug.LogWarning(Maintainer.LOG_PREFIX + "Can't get the " + propertyName + " property! Please report to " + Maintainer.SUPPORT_EMAIL);
				}
			}
			else
			{
				if (property != null && property.isArray)
				{
					for (int i = 0; i < property.arraySize; i++)
					{
						SerializedProperty item = property.GetArrayElementAtIndex(i);
						SerializedProperty logoProperty = item.FindPropertyRelative(arrayItemName);
						if (logoProperty != null && logoProperty.propertyType == SerializedPropertyType.ObjectReference)
						{
							int instanceId = logoProperty.objectReferenceInstanceIDValue;
							if (instanceId != 0)
							{
								string path = AssetDatabase.GetAssetPath(instanceId);
								if (!string.IsNullOrEmpty(path) && path.StartsWith("Assets"))
								{
									paths.Add(path);
								}
							}
						}
						else
						{
							Debug.LogWarning(Maintainer.LOG_PREFIX + "Can't get the " + arrayItemName + " property! Please report to " + Maintainer.SUPPORT_EMAIL);
						}
					}
				}
				else
				{
					Debug.LogWarning(Maintainer.LOG_PREFIX + "Can't get the " + propertyName + " property! Please report to " + Maintainer.SUPPORT_EMAIL);
				}
			}

			return foundPaths;
		}
	}

	internal class CSBuildReportInfo
	{
		private string[] usedAssets;

		public static CSBuildReportInfo GetLatestBuildReportFromFile(string logFilePath)
		{
			CSBuildReportInfo[] reports = GetBuildReportsFromFile(logFilePath);
			if (reports != null && reports.Length > 0)
			{
				return reports.LastOrDefault();
			}

			return null;
		}

		public static CSBuildReportInfo[] GetBuildReportsFromFile(string logFilePath)
		{
			if (!File.Exists(logFilePath)) return null;

			List<CSBuildReportInfo> reports = new List<CSBuildReportInfo>();

			Stopwatch sw = Stopwatch.StartNew();

			FileStream fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

			using (StreamReader sr = new StreamReader(fs))
			{
				string line;

				CSBuildReportInfo currentReport = null;
				List<string> usedAssets = null;

				while ((line = sr.ReadLine()) != null)
				{
					if (line.Contains("building target "))
					{
						currentReport = new CSBuildReportInfo();
					}

					if (currentReport != null)
					{
						if (line.Contains("% Assets"))
						{
							if (usedAssets == null) usedAssets = new List<string>();

							string usedAsset = line.Substring(line.IndexOf("% Assets") + 2);
							usedAssets.Add(usedAsset);
						}

						if (line.StartsWith("*** Completed 'Build."))
						{
							if (usedAssets != null && usedAssets.Count > 0)
								currentReport.usedAssets = usedAssets.ToArray();

							reports.Add(currentReport);
							currentReport = null;
						}
					}
				}
			}

			fs.Close();

			sw.Stop();

			return reports.ToArray();
		}

		public string[] GetUsedAssets()
		{
			return usedAssets;
		}
	}
}

#endif