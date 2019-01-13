// Adapted from https://gist.github.com/liortal53/780075ddb17f9306ae32
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RemoveEmptyFolders
{
	/// <summary>
	/// Use this flag to simulate a run, before really deleting any folders.
	/// </summary>
	private static bool dryRun = true;

	[MenuItem("Tools/Delete empty folders (Dry Run)", false, 54)]
	private static void RemoveEmptyFoldersMenuItemDry()
	{
		dryRun = true;
		RemoveEmptyFoldersMenuItem();
	}
	[MenuItem("Tools/Delete empty folders", false, 55)]
	private static void RemoveEmptyFoldersMenuItemReal()
	{
		dryRun = false;
		RemoveEmptyFoldersMenuItem();
	}

	private static void RemoveEmptyFoldersMenuItem()
	{
		var index = Application.dataPath.IndexOf("/Assets");
		var projectSubfolders = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);

		// Create a list of all the empty subfolders under Assets.
		var emptyFolders = projectSubfolders.Where(path => IsEmptyRecursive(path)).ToArray();

		foreach (var folder in emptyFolders)
		{
			// Verify that the folder exists (may have been already removed).
			if (Directory.Exists (folder))
			{
				if (dryRun)
				{
					Debug.Log ("Will delete : " + folder);
				}
				else
				{
					Debug.Log ("Deleting : " + folder);

					// Remove dir (recursively)
					Directory.Delete(folder, true);

					// Sync AssetDatabase with the delete operation.
					AssetDatabase.DeleteAsset(folder.Substring(index + 1));
				}
			}
		}

		// Refresh the asset database once we're done.
		AssetDatabase.Refresh();
	}

	/// <summary>
	/// A helper method for determining if a folder is empty or not.
	/// </summary>
	private static bool IsEmptyRecursive(string path)
	{
		// A folder is empty if it (and all its subdirs) have no files (ignore .meta files)
		return Directory.GetFiles(path).Select(file => !file.EndsWith(".meta")).Count() == 0
			&& Directory.GetDirectories(path, string.Empty, SearchOption.AllDirectories).All(IsEmptyRecursive);
	}
}
