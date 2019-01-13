/// <summary>
/// Taken from http://forum.unity3d.com/threads/little-script-apply-and-revert-several-prefab-at-once.295311/
///
/// with credits : bapisteLar and 10fingerArmy
///Hi,
///
///In a last project i had a lot of prefabs to modify every day, since it would have been too long to do it by hand, i made a little script that can apply and revert selected prefabs.
///
///	I share it to you, it's not heavely tested but it suited my need.
///
///HIW : Select all your prefabs and use either shortcut or Tools menu item to apply and revert.
///
///(You can change shortcuts in the script if you want, currently it uses ctrl+shft+a to apply and ctrl+shft+r to revert)
///
///
/// </summary>

using UnityEditor;
using UnityEngine;

public class ApplySelectedPrefabs : EditorWindow
{
    public delegate void ApplyOrRevert(GameObject _goCurrentGo, Object _ObjPrefabParent, ReplacePrefabOptions _eReplaceOptions);

    [MenuItem ("Tools/Selected prefabs -- Apply all",false,1)]//%#q")]
    static void ApplyPrefabs()
    {
        SearchPrefabConnections (ApplyToSelectedPrefabs);
    }

    [MenuItem ("Tools/Selected prefabs -- Revert all",false,1)]// %#e")]
    static void ResetPrefabs()
    {
        SearchPrefabConnections (RevertToSelectedPrefabs);
    }

    //Look for connections
    static void SearchPrefabConnections(ApplyOrRevert _applyOrRevert)
    {
        GameObject[] tSelection = Selection.gameObjects;

        if (tSelection.Length > 0)
        {
            GameObject goPrefabRoot;
            //GameObject goParent;
            GameObject goCur;
            bool bTopHierarchyFound;
            int iCount=0;
            PrefabType prefabType;
            bool bCanApply;
            //Iterate through all the selected gameobjects
            foreach(GameObject go in tSelection)
            {
                prefabType = PrefabUtility.GetPrefabType(go);
                //Is the selected gameobject a prefab?
                if(prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance)
                {
                    //Prefab Root;
                    goPrefabRoot = ((GameObject)PrefabUtility.GetPrefabParent(go)).transform.root.gameObject;
                    goCur = go;
                    bTopHierarchyFound = false;
                    bCanApply = true;
                    //We go up in the hierarchy to apply the root of the go to the prefab
                    while(goCur.transform.parent != null && !bTopHierarchyFound)
                    {
                        //Are we still in the same prefab?
                        if (PrefabUtility.GetPrefabParent(goCur.transform.parent.gameObject) != null && (goPrefabRoot == ((GameObject)PrefabUtility.GetPrefabParent(goCur.transform.parent.gameObject)).transform.root.gameObject))
                        {
                            goCur = goCur.transform.parent.gameObject;
                        }
                        else
                        {
                            //The gameobject parent is another prefab, we stop here
                            bTopHierarchyFound = true;
                            if(goPrefabRoot !=  ((GameObject)PrefabUtility.GetPrefabParent(goCur)))
                            {
                                //Gameobject is part of another prefab
                                bCanApply = false;
                            }
                        }
                    }

                    if(_applyOrRevert != null && bCanApply)
                    {
                        iCount++;
                        _applyOrRevert(goCur, PrefabUtility.GetPrefabParent(goCur),ReplacePrefabOptions.ConnectToPrefab);
                    }
                }
            }
            Debug.Log(iCount + " prefab" + (iCount>1 ? "s" : "") + " updated");
        }
    }

    //Apply
    static void ApplyToSelectedPrefabs(GameObject _goCurrentGo, Object _ObjPrefabParent, ReplacePrefabOptions _eReplaceOptions)
    {
        PrefabUtility.ReplacePrefab(_goCurrentGo, _ObjPrefabParent,_eReplaceOptions);
    }

    //Revert
    static void RevertToSelectedPrefabs(GameObject _goCurrentGo, Object _ObjPrefabParent, ReplacePrefabOptions _eReplaceOptions)
    {
        PrefabUtility.ReconnectToLastPrefab(_goCurrentGo);
        PrefabUtility.RevertPrefabInstance(_goCurrentGo);
    }


}
