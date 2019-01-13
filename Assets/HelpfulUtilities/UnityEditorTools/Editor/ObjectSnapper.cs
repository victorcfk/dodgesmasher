//using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;

//[ExecuteInEditMode]
//public class ObjectSnapperEditor : EditorWindow
//{
//    const float positionGranularity = 0.5f;
//    const float scaleGranularity = 1;
//    const float rotationGranularity = 1;

//    public bool activeSnapping = true;
//    public bool onlySnapObjectsOnWallLayer = true;
//    public bool onlySnapObjectsUnderTheSceneInfoWallParentObject = true;

//    //SceneInfo sceneInfo;
//    int wallLayer;
//    string reason;
//    List<BoxCollider> bcs = new List<BoxCollider>();
//    List<CapsuleCollider> ccs = new List<CapsuleCollider>();
//    bool isSnapping;
//    bool hasSphereCollider;

//    [MenuItem("Tools/Wall Collider Snapper", false, 3)]
//    public static void OpenSceneObjectSnapperEditor()
//    {
//        ObjectSnapperEditor window = EditorWindow.GetWindow<ObjectSnapperEditor>("Wall Snapper");
//        window.Show();
//    }

//    void OnEnable()
//    {
//        wallLayer = LayerMask.NameToLayer("Wall");
//        // sceneInfo = FindObjectOfType<SceneInfo>();
//    }

//    void OnSelectionChange()
//    {
//        Repaint();
//    }

//    void OnGUI()
//    {
//        EditorGUILayout.LabelField("Keep this window open doing the below every frame");
//        EditorGUILayout.LabelField("=====================================================");
//        EditorGUILayout.LabelField("Snap XYZ position with granularity: " + positionGranularity);
//        EditorGUILayout.LabelField("Snap XYZ scale granularity: " + scaleGranularity);
//        EditorGUILayout.LabelField("Snap XYZ rotation granularity: " + rotationGranularity);
//        EditorGUILayout.LabelField("Standardize collider properties and layer values.");

//        activeSnapping = EditorGUILayout.ToggleLeft("Active Snapping", activeSnapping);
//        onlySnapObjectsOnWallLayer = EditorGUILayout.ToggleLeft("Only snap if Wall Layer(8)", onlySnapObjectsOnWallLayer);
//        onlySnapObjectsUnderTheSceneInfoWallParentObject = EditorGUILayout.ToggleLeft("Only snap is parent is overall wall parent", onlySnapObjectsUnderTheSceneInfoWallParentObject);

//        EditorGUILayout.LabelField("=====================================================");
//        EditorGUILayout.LabelField("Status: " + reason);
//        if (isSnapping && hasSphereCollider)
//            EditorGUILayout.LabelField("Replace SphereCollider with CapsuleCollider");
//    }

//    void Update()
//    {
//        isSnapping = false;

//        if (Application.isPlaying)
//        {
//            reason = "Not active in play mode";
//            Repaint();
//            return;
//        }

//        // No snapping?
//        if (!activeSnapping)
//        {
//            reason = "Not active";
//            Repaint();
//            return;
//        }

//        // No object?
//        GameObject selectedGameObject = Selection.activeGameObject;
//        if (selectedGameObject == null)
//        {
//            reason = "No selected object";
//            Repaint();
//            return;
//        }

//        // Object must be an instance in scene, and not a prefab file
//        PrefabType prefabType = PrefabUtility.GetPrefabType(selectedGameObject);
//        if (prefabType == PrefabType.Prefab || prefabType == PrefabType.ModelPrefab)
//        {
//            reason = "Selected object not GameOject in scene";
//            Repaint();
//            return;
//        }

//        // Check if object is on wall layer
//        if (onlySnapObjectsOnWallLayer && selectedGameObject.layer != wallLayer)
//        {
//            reason = "Selected object not on Wall Layer";
//            Repaint();
//            return;
//        }

//        // Check if it is child of SceneInfoWallParentObject
//        if (onlySnapObjectsUnderTheSceneInfoWallParentObject)
//        {
//            if (sceneInfo == null)
//                sceneInfo = FindObjectOfType<SceneInfo>();

//            if (sceneInfo == null)
//            {
//                reason = "No SceneInfo in Scene";
//                Repaint();
//                return;
//            }

//            if (sceneInfo.allSceneStaticColliderParent == null)
//            {
//                reason = "No Collider Parent specified in SceneInfo";
//                Repaint();
//                return;
//            }

//            if (!selectedGameObject.transform.IsChildOf(sceneInfo.allSceneStaticColliderParent.transform))
//            {
//                reason = "Selected GameObject not a child of Collider Parent specified in SceneInfo";
//                Repaint();
//                return;
//            }
//        }

//        selectedGameObject.GetComponents<BoxCollider>(bcs);
//        selectedGameObject.GetComponents<CapsuleCollider>(ccs);
//        if (bcs.Count == 0 && ccs.Count == 0)
//        {
//            reason = "No BoxCollider/CapsuleCollider on selected GameObject";
//            Repaint();
//            return;
//        }

//        // Snapping occurs this point onwards
//        reason = "Active snapping on selected GameObject \"" + selectedGameObject.name + "\"";
//        isSnapping = true;
//        bool isDirty = false;
//        Transform t = selectedGameObject.transform;

//        // Snap position
//        Vector3 v = t.position;
//        v.x = positionGranularity * Mathf.Round(v.x / positionGranularity);
//        v.y = positionGranularity * Mathf.Round(v.y / positionGranularity);
//        v.z = positionGranularity * Mathf.Round(v.z / positionGranularity);
//        if (v != t.position)
//        {
//            isDirty = true;
//            t.position = v;
//        }

//        // Snap scale
//        v = t.localScale;
//        v.x = scaleGranularity * Mathf.Round(v.x / scaleGranularity);
//        v.y = scaleGranularity * Mathf.Round(v.y / scaleGranularity);
//        v.z = scaleGranularity * Mathf.Round(v.z / scaleGranularity);
//        if (v != t.localScale)
//        {
//            isDirty = true;
//            t.localScale = v;
//        }

//        // Snap rotation
//        v = t.eulerAngles;
//        v.x = 0;
//        v.y = rotationGranularity * Mathf.Round(v.y / rotationGranularity);
//        v.z = 0;
//        if (v != t.eulerAngles)
//        {
//            isDirty = true;
//            t.eulerAngles = v;
//        }

//        // Snap Box Colliders
//        for (int i = 0, bcsCount = bcs.Count; i < bcsCount; i++)
//        {
//            BoxCollider bc = bcs[i];

//            if (bc.isTrigger) { isDirty = true; bc.isTrigger = false; }
//            if (bc.material) { isDirty = true; bc.material = null; }
//            if (bc.size != Vector3.one) { isDirty = true; bc.size = Vector3.one; }
//            if (bc.center != Vector3.zero) { isDirty = true; bc.center = Vector3.zero; }

//            v = t.localScale;
//            v.y = (v.y >= ProjectGlobal.fullHeightScale) ? ProjectGlobal.fullHeightScale : ProjectGlobal.belowWaistHeightScale;
//            if (t.localScale != v) { isDirty = true; t.localScale = v; }

//            v = t.position;
//            v.y = t.localScale.y / 2;
//            v.y = (v.y >= ProjectGlobal.fullHeightPosition) ? ProjectGlobal.fullHeightPosition : ProjectGlobal.belowWaistHeightPosition;
//            if (t.position != v) { isDirty = true; t.position = v; }
//        }

//        for (int i = 0, ccsCount = ccs.Count; i < ccsCount; i++)
//        {
//            CapsuleCollider cc = ccs[i];

//            if (cc.isTrigger) { isDirty = true; cc.isTrigger = false; }
//            if (cc.material) { isDirty = true; cc.material = null; }
//            //			if (cc.radius != 0.5f) { isDirty = true; cc.radius = 0.5f; }
//            if (cc.transform.localScale.x != 1)
//            {
//                isDirty = true;
//                cc.radius = cc.transform.localScale.x * 0.5f;
//                cc.height = cc.transform.localScale.y;
//                cc.transform.localScale = Vector3.one;
//            }

//            if (cc.height != 2) { isDirty = true; cc.height = 2; }
//            if (cc.center != Vector3.zero) { isDirty = true; cc.center = Vector3.zero; }

//            v = t.localScale;
//            v.y = (v.y >= ProjectGlobal.fullHeightScale) ? ProjectGlobal.fullHeightScale : ProjectGlobal.belowWaistHeightScale;
//            if (t.localScale != v) { isDirty = true; t.localScale = v; }

//            v = t.position;
//            v.y = t.localScale.y / 2;
//            v.y = (v.y >= ProjectGlobal.fullHeightPosition) ? ProjectGlobal.fullHeightPosition : ProjectGlobal.belowWaistHeightPosition;
//            if (t.position != v) { isDirty = true; t.position = v; }
//        }

//        // Sphere Collider
//        hasSphereCollider = t.GetComponent<SphereCollider>() != null;

//        if (isDirty)
//        {
//            EditorUtility.SetDirty(selectedGameObject);
//        }
//    }
//}
