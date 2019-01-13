using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// This represents what a level can be. each object should be in a correct place
/// </summary>
public class LevelTemplate : ScriptableObject {

    public int BaseDifficulty;
    public int MinDifficultyToUse;  //must have
    public int MaxDifficultyToUse;

    [System.Serializable]
    public class SpawnGroup
    {
        //[InspectorReadOnly]
        [SerializeField]
        string _name;
        public int NumInList { get; set; }
        public Transform[] SpawnTransform;
        public GameObject[] SpawnPrefab;

        public void OnValidate()
        {   
            if(SpawnPrefab.Length != SpawnTransform.Length)
            {
                Transform[] arr = new Transform[SpawnPrefab.Length];

                for(int i=0; i< SpawnTransform.Length; i++)
                {
                    if(i <= arr.Length-1)
                        arr[i] = SpawnTransform[i];
                }

                SpawnTransform = arr;
            }
            
            //Generate a correct naming
            //=============================================
                    
            string[] strArray = SpawnPrefab.ListToStringArray(true, true);
            string[] strPosArray = SpawnTransform.ListToStringArray();

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i=0; i< strArray.Length; i++)
            {
                sb.Append(strArray[i].Replace("(UnityEngine.GameObject)", string.Empty));

                strPosArray[i] = strPosArray[i].Replace("(UnityEngine.Transform)", string.Empty);
                strPosArray[i] = strPosArray[i].Replace("SpawnUnits", string.Empty);
                strPosArray[i] = strPosArray[i].Replace("SpawnTerrain", string.Empty);

                sb.Append(strPosArray[i]);
                sb.Append("| ");
            }

            _name = sb.ToString();
        }
    }

    [Space]
    [Header ("Objects")]
    //The environment should not be scattered willy nilly
    public List<SpawnGroup> TopPortionTemplateSets; //one part of the level
    public List<SpawnGroup> BtmPortionTemplateSets; //another part of the level

    public void GetGameObjectGroupsExcluding
        (out List<Transform> inSpawnPos, out List<GameObject> inSpawnObj,
        int inTopListPosToExclude, int inBtmListPosToExclude, 
        out int outUsedTopPointerInList, out int outUsedBtmPointerInList,
        System.Random inRand = null)
    {
        if (inRand == null) inRand = new System.Random();

        outUsedTopPointerInList = inRand.NextExcluding(0, TopPortionTemplateSets.Count, inTopListPosToExclude);
        outUsedBtmPointerInList = inRand.NextExcluding(0, BtmPortionTemplateSets.Count, inBtmListPosToExclude);

        SpawnGroup s1 = TopPortionTemplateSets[outUsedTopPointerInList];
        SpawnGroup s2 = BtmPortionTemplateSets[outUsedBtmPointerInList];
        Debug.Log(name + " Spawned top " + s1.NumInList);
        Debug.Log(name + " Spawned btm " + s2.NumInList);

        inSpawnPos = new List<Transform>();

        foreach (var s in s1.SpawnTransform)
        {
            inSpawnPos.Add(s);
        }
        foreach (var s in s2.SpawnTransform)
        {
            inSpawnPos.Add(s);
        }

        inSpawnObj = new List<GameObject>();

        foreach (var s in s1.SpawnPrefab)
        {
            inSpawnObj.Add(s);
        }
        foreach (var s in s2.SpawnPrefab)
        {
            inSpawnObj.Add(s);
        }
    }

    public void GetGameObjectGroups(out List<Transform> inSpawnPos, out List<GameObject> inSpawnObj, System.Random inRand = null)
    {
        SpawnGroup s1 = TopPortionTemplateSets.GetRandElementInList(inRand);
        SpawnGroup s2 = BtmPortionTemplateSets.GetRandElementInList(inRand);

        inSpawnPos = new List<Transform>();

        foreach (var s in s1.SpawnTransform)
        {
            inSpawnPos.Add(s);
        }
        foreach (var s in s2.SpawnTransform)
        {
            inSpawnPos.Add(s);
        }

        inSpawnObj = new List<GameObject>();

        foreach (var s in s1.SpawnPrefab)
        {
            inSpawnObj.Add(s);
        }
        foreach (var s in s2.SpawnPrefab)
        {
            inSpawnObj.Add(s);
        }
    }

    public void OnValidate()
    {
        for (int j = 0; j < TopPortionTemplateSets.Count; j++)
        {
            var s = TopPortionTemplateSets[j];
            //Make sure the assigning is not errornous
            //================================================
            for (int i=0; i<s.SpawnTransform.Length; i++)
            {
                Transform t = s.SpawnTransform[i];
                if (
                    (!t.name.ToLower().Contains("top"))
                    ||
                    (name.Contains("unit") && !t.name.ToLower().Contains("unit"))
                    ||
                    (name.Contains("terrain") && !t.name.ToLower().Contains("terrain"))
                    )
                {
                    s.SpawnTransform[i] = null;
                }
            }

            s.OnValidate();
            s.NumInList = j;
        }

        //Make sure the assigning is not errornous
        //================================================
        for (int j= 0; j < BtmPortionTemplateSets.Count; j++)
        {
            var s = BtmPortionTemplateSets[j];
            for (int i = 0; i < s.SpawnTransform.Length; i++)
            {
                Transform t = s.SpawnTransform[i];
                if (
                    (!t.name.ToLower().Contains("btm"))
                    ||
                    (name.Contains("unit") && !t.name.ToLower().Contains("unit"))
                    ||
                    (name.Contains("terrain") && !t.name.ToLower().Contains("terrain"))
                    )
                {
                    s.SpawnTransform[i] = null;
                }
            }

            s.OnValidate();
            s.NumInList = j;
        }

        //need addition validattion to prevent the same transform location from being assigned and to recognise top and bottom
    }
}

#if UNITY_EDITOR
public class YourClassAsset
{
    [MenuItem("Assets/Create/LevelObj")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<LevelTemplate>();
    }
}
#endif