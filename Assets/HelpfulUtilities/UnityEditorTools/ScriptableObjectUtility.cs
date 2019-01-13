#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class ScriptableObjectUtility
{
    /// <summary>
    // This makes it easy to create, name and place unique new ScriptableObject asset files.
    // Code taken from unifycommunity.com
    /// </summary>
    public static void CreateAsset<T>() where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);

        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }

    [MenuItem("Assets/Copy Asset Path")]
    public static void CopyAssetPath()
    {
        Object o = Selection.activeObject;
        if (o != null)
            EditorGUIUtility.systemCopyBuffer = AssetDatabase.GetAssetPath(o);
    }
    [MenuItem("Assets/Copy Asset Path", true)]
    public static bool CopyAssetPathValidate()
    {
        Object o = Selection.activeObject;
        if (o == null) return false;

        switch (PrefabUtility.GetPrefabType(o))
        {
            case PrefabType.None:
            case PrefabType.Prefab:
            case PrefabType.ModelPrefab:
                return true;
            default:
                return false;
        }
    }

    [MenuItem("Assets/Copy Resource Path")]
    public static void CopyResourcePath()
    {
        Object o = Selection.activeObject;
        string path = AssetDatabase.GetAssetPath(o);
        int start = path.IndexOf("/Resources/") + 11;
        int end = path.LastIndexOf(".");
        EditorGUIUtility.systemCopyBuffer = path.Substring(start, end - start);
    }

    [MenuItem("Assets/Copy Resource Path", true)]
    public static bool CopyResourcePathValidate()
    {
        Object o = Selection.activeObject;
        if (o == null) return false;

        switch (PrefabUtility.GetPrefabType(o))
        {
            case PrefabType.None:
            case PrefabType.Prefab:
            case PrefabType.ModelPrefab:
                return AssetDatabase.GetAssetPath(o).Contains("/Resources/");
            default:
                return false;
        }
    }
}
#endif