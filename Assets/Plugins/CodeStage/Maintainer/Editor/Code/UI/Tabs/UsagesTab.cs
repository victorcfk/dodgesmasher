#if UNITY_EDITOR

using System.IO;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI.Filters;
using CodeStage.Maintainer.Usages;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal class UsagesTab : RecordsTab<UsagesRecord>
	{
		private GUIContent caption;
		internal GUIContent Caption
		{
			get
			{
				if (caption == null)
				{
					caption = new GUIContent(UsagesFinder.MODULE_NAME, CSIcons.Find);
				}
				return caption;
			}
		}

		protected override UsagesRecord[] LoadLastRecords()
		{
			UsagesRecord[] loadedRecords = SearchResultsStorage.UsagesSearchResults;
			if (loadedRecords == null) loadedRecords = new UsagesRecord[0];

			return loadedRecords;
		}

		protected override void DrawSettingsBody()
		{
			using (layout.Vertical(UIHelpers.panelWithBackground))
			{
				GUILayout.Space(5);

				if (UIHelpers.ImageButton("Manage Filters...", CSIcons.Gear))
				{
					UsagesFiltersWindow.Create();
				}

				GUILayout.Space(5);
				UIHelpers.Separator();

				if (UIHelpers.ImageButton("Reset", "Resets settings to defaults.", CSIcons.Restore))
				{
					MaintainerSettings.Cleaner.Reset();
				}
			}
		}

		protected override void DrawSearchTop()
		{
			int objectsSelected = CSEditorTools.GetProjectSelections(false).Length;


			string buttonCpation = "Please select object(s)";
			if (objectsSelected > 0)
			{
				buttonCpation = "Find usages of " + objectsSelected + " selected objects";
			}
			else
			{
				GUI.enabled = false;
			}

			if (UIHelpers.ImageButton(buttonCpation, CSIcons.Find))
			{
				EditorApplication.delayCall += StartUsagesSearch;
			}

			GUI.enabled = true;
		}

		/*protected override void DrawPagesRightHeader()
		{
			base.DrawPagesRightHeader();

			GUILayout.Label("Sorting:", GUILayout.ExpandWidth(false));

			EditorGUI.BeginChangeCheck();
			MaintainerSettings.Cleaner.sortingType = (CleanerSortingType)EditorGUILayout.EnumPopup(MaintainerSettings.Cleaner.sortingType, GUILayout.Width(100));
			if (EditorGUI.EndChangeCheck())
			{
				ApplySorting();
			}

			EditorGUI.BeginChangeCheck();
			MaintainerSettings.Cleaner.sortingDirection = (SortingDirection)EditorGUILayout.EnumPopup(MaintainerSettings.Cleaner.sortingDirection, GUILayout.Width(80));
			if (EditorGUI.EndChangeCheck())
			{
				ApplySorting();
			}
		}*/

		protected override void DrawRecord(int recordIndex)
		{
			UsagesRecord record = filteredRecords[recordIndex];

			if (record == null) return;

			using (layout.Vertical())
			{
				if (recordIndex > 0 && recordIndex < filteredRecords.Length) UIHelpers.Separator();

				using (layout.Horizontal())
				{
					DrawRecordCheckbox(record);
					DrawExpandCollapseButton(record);

					if (record.compactMode)
					{
						DrawRecordButtons(record, recordIndex);
						GUILayout.Label(record.GetCompactLine(), UIHelpers.richLabel);
					}
					else
					{
						GUILayout.Space(5);
						GUILayout.Label(record.GetHeader(), UIHelpers.richLabel);
					}
				}

				if (!record.compactMode)
				{
					UIHelpers.Separator();
					using (layout.Horizontal())
					{
						GUILayout.Space(5);
						GUILayout.Label(record.GetBody(), UIHelpers.richLabel);
					}
					using (layout.Horizontal())
					{
						GUILayout.Space(5);
						DrawRecordButtons(record, recordIndex);
					}
					GUILayout.Space(3);
				}
			}

			if (Event.current != null && Event.current.type == EventType.MouseDown)
			{
				Rect guiRect = GUILayoutUtility.GetLastRect();
				guiRect.height += 2; // to compensate the separator's gap

				if (guiRect.Contains(Event.current.mousePosition))
				{
					record.compactMode = !record.compactMode;
					Event.current.Use();
				}
			}
		}

		/*protected override void ApplySorting()
		{
			base.ApplySorting();

			switch (MaintainerSettings.Cleaner.sortingType)
			{
				case CleanerSortingType.Unsorted:
					break;
				case CleanerSortingType.ByPath:
					filteredRecords = MaintainerSettings.Cleaner.sortingDirection == SortingDirection.Ascending ?
						filteredRecords.OrderBy(RecordsSortings.cleanerRecordByPath).ToArray() :
						filteredRecords.OrderByDescending(RecordsSortings.cleanerRecordByPath).ToArray();
					break;
				case CleanerSortingType.ByType:
					filteredRecords = MaintainerSettings.Cleaner.sortingDirection == SortingDirection.Ascending ?
						filteredRecords.OrderBy(RecordsSortings.cleanerRecordByType).ThenBy(RecordsSortings.cleanerRecordByAssetType).ThenBy(RecordsSortings.cleanerRecordByPath).ToArray() :
						filteredRecords.OrderByDescending(RecordsSortings.cleanerRecordByType).ThenBy(RecordsSortings.cleanerRecordByAssetType).ThenBy(RecordsSortings.cleanerRecordByPath).ToArray();
					break;
				case CleanerSortingType.BySize:
					filteredRecords = MaintainerSettings.Cleaner.sortingDirection == SortingDirection.Ascending ?
						filteredRecords.OrderByDescending(RecordsSortings.cleanerRecordBySize).ThenBy(RecordsSortings.cleanerRecordByPath).ToArray() :
						filteredRecords.OrderBy(RecordsSortings.cleanerRecordBySize).ThenBy(RecordsSortings.cleanerRecordByPath).ToArray();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}*/

		protected override void SaveSearchResults()
		{
			SearchResultsStorage.UsagesSearchResults = GetRecords();
		}

		protected override string GetModuleName()
		{
			return UsagesFinder.MODULE_NAME;
		}

		protected override string GetReportFileNamePart()
		{
			return "Usages";
		}

		protected override void AfterClearRecords()
		{
			SearchResultsStorage.UsagesSearchResults = null;
		}

		private void SelectObjects()
		{
			
		}

		private void StartUsagesSearch()
		{
			
		}

		private void DrawRecordButtons(UsagesRecord record, int recordIndex)
		{
			DrawShowButtonIfPossible(record);

			ObjectUsagesRecord assetRecord = record as ObjectUsagesRecord;
			if (assetRecord != null)
			{
				if (record.compactMode)
				{
					DrawMoreButton(assetRecord);
				}
				else
				{
					DrawRevealButton(assetRecord);
					DrawCopyButton(assetRecord);
					DrawMoreButton(assetRecord);
				}
			}
		}

		private void DrawRevealButton(ObjectUsagesRecord record)
		{
			if (UIHelpers.RecordButton(record, "Reveal", "Reveals item in system default File Manager like Explorer on Windows or Finder on Mac.", CSIcons.Reveal))
			{
				EditorUtility.RevealInFinder(record.path);
			}
		}

		private void DrawMoreButton(ObjectUsagesRecord assetRecord)
		{
			if (UIHelpers.RecordButton(assetRecord, "Shows menu with additional actions for this record.", CSIcons.More))
			{
				GenericMenu menu = new GenericMenu();
				if (!string.IsNullOrEmpty(assetRecord.path))
				{
					menu.AddItem(new GUIContent("Ignore/Add path to ignores"), false, () =>
					{
						if (CSArrayTools.AddIfNotExists(ref MaintainerSettings.Cleaner.pathIgnores, assetRecord.assetDatabasePath))
						{
							MaintainerWindow.ShowNotification("Ignore added: " + assetRecord.assetDatabasePath);
							CleanerFiltersWindow.Refresh();
						}
						else
						{
							MaintainerWindow.ShowNotification("Such item already added to the ignores!");
						}
					});

					DirectoryInfo dir = Directory.GetParent(assetRecord.assetDatabasePath);
					if (dir.Name != "Assets")
					{
						menu.AddItem(new GUIContent("Ignore/Add parent directory to ignores"), false, () =>
						{
							if (CSArrayTools.AddIfNotExists(ref MaintainerSettings.Cleaner.pathIgnores, dir.ToString()))
							{
								MaintainerWindow.ShowNotification("Ignore added: " + dir);
								CleanerFiltersWindow.Refresh();
							}
							else
							{
								MaintainerWindow.ShowNotification("Such item already added to the ignores!");
							}
						});
					}
				}
				menu.ShowAsContext();
			}
		}
	}
}

#endif