using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class EditorSceneCameraToIso
{
	static bool ToggleISOOrTopDown = false;

	#if UNITY_EDITOR_OSX

	[MenuItem("Tools/Force Editor Scene Camera to Iso %;", false, 10)]

	#else

	[MenuItem("Tools/Force Editor Scene Camera to Iso %h", false, 10)]

	#endif

	static void ToIso()
	{
		ArrayList sceneViews = SceneView.sceneViews;
		if (sceneViews.Count == 0) return;

		// Get selection, then get transforms from them
		Object[] selected = Selection.objects;
		List<Transform> trans = new List<Transform>();
		foreach (Object o in selected)
		{
			GameObject go = o as GameObject;
			if (go != null) trans.Add(go.transform);
		}
		Vector3 pivot = FindThePivot(trans);
		
		// For each editor scene camera, set their views
		for (int i = 0; i < sceneViews.Count; i++)
		{
			SceneView sc = (SceneView)sceneViews[i];
			sc.orthographic = true;

			if(ToggleISOOrTopDown)
			{
				sc.rotation = Quaternion.Euler(90, 0, 0);
			}
			else
			{
				sc.rotation = Quaternion.Euler(30, 315, 0);
			}

			if (trans.Count > 0) sc.LookAt(pivot);
		}
		ToggleISOOrTopDown = !ToggleISOOrTopDown;
	}

	static Vector3 FindThePivot(List<Transform> trans)
	{
		if (trans == null || trans.Count == 0)
			return Vector3.zero;
		if (trans.Count == 1)
			return trans[0].position;

		float minX = Mathf.Infinity;
		float minY = Mathf.Infinity;
		float minZ = Mathf.Infinity;
		float maxX = -Mathf.Infinity;
		float maxY = -Mathf.Infinity;
		float maxZ = -Mathf.Infinity;
		foreach (Transform tr in trans)
		{
			if (tr.position.x < minX)
				minX = tr.position.x;
			if (tr.position.y < minY)
				minY = tr.position.y;
			if (tr.position.z < minZ)
				minZ = tr.position.z;
			if (tr.position.x > maxX)
				maxX = tr.position.x;
			if (tr.position.y > maxY)
				maxY = tr.position.y;
			if (tr.position.z > maxZ)
				maxZ = tr.position.z;
		}
		return new Vector3((minX+maxX)/2.0f,(minY+maxY)/2.0f,(minZ+maxZ)/2.0f);
	}
}
