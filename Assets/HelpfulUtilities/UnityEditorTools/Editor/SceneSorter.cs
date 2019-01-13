using UnityEditor;
using UnityEngine;

public class SceneSorter
{
    static string[] keywords =
    {
        "spawn",
        "gameworld",
        "gamemanager",
        "main hud gui",
        "canvasfacadeback_lower",
        "canvasfacadeback",
        "canvasfacadefront",
        "canvasfacadeback_upper",
        "maplimits",
        "ground",
        "groundcolliders",
        "environmentfx",
        "environmentalfx",
        "fogobject",
        "fogobjects",
        "objectcolliders",
        "object colliders",
        "colliders",
        "walls",
        "wall",
        "wallcolliders",
        "walltemp",
        "walltemps",
    };

    [MenuItem("Tools/Sort Scene Hierachy", false, 13)]
    static void SortHierachy()
    {
        GameObject[] roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        System.Array.Sort(roots, Compare);
        for (int i = 0; i < roots.Length; i++)
        {
            Undo.SetTransformParent(roots[i].transform, null, "Sort Scene Hierachy"); // This is needed to record the sibling indexes before each change
            roots[i].transform.SetSiblingIndex(i);
            EditorUtility.SetDirty(roots[i].transform);
        }
    }

    public static int Compare(GameObject lhs, GameObject rhs)
    {
        if (lhs == rhs) return 0;

        string lhsName = lhs.name.ToLower();
        string rhsName = rhs.name.ToLower();
        for (int i = 0; i < keywords.Length; i++)
        {
            if (lhsName == keywords[i] && rhsName != keywords[i]) return -1;
            if (lhsName != keywords[i] && rhsName == keywords[i]) return 1;
        }

        // Keep current scene ordering
        return (lhs.transform.GetSiblingIndex() < rhs.transform.GetSiblingIndex()) ? -1 : 1;

        //return EditorUtility.NaturalCompare(lhs.name, rhs.name);
    }
}
