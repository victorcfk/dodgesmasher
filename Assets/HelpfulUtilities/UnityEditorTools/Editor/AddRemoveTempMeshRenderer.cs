using UnityEngine;
using UnityEditor;

public class AddRemoveTempMeshRenderer
{
	[MenuItem("Tools/Add Remove Temp MeshRenderer %i", false, 11)]
	static void AddRemoveTempRenderer()
	{
		// Test for add/remove mode
		bool removeMode = false;
		bool addMode = false;
		Object[] selected = Selection.objects;
		for (int i = 0, selectedLength = selected.Length; i < selectedLength; i++)
		{
			GameObject go = selected[i] as GameObject;
			bool hasCollider = (go.GetComponent<Collider>() != null);
			if (!hasCollider)
				continue;

			bool hasMeshFilter = (go.GetComponent<MeshFilter>() != null);
			bool hasMeshRenderer = (go.GetComponent<MeshRenderer>() != null);
			if (!hasMeshFilter && !hasMeshRenderer)
				addMode = true;
			if (hasMeshFilter && hasMeshRenderer)
				removeMode = true;
		}

		if (addMode)
		{
			Mesh tempCubeMesh = null;
			Mesh tempSphereMesh = null;
			Mesh tempCapsuleMesh = null;
			int count = 0;

			for (int i = 0, selectedLength = selected.Length; i < selectedLength; i++)
			{
				GameObject go = selected[i] as GameObject;
				Collider collider = go.GetComponent<Collider>();
				if (collider == null)
					continue;

				MeshFilter meshFilter = go.GetComponent<MeshFilter>();
				MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
				if (meshFilter == null && meshRenderer == null)
				{
					meshRenderer = Undo.AddComponent<MeshRenderer>(go);
					meshRenderer.materials = new Material[1];

					meshFilter = Undo.AddComponent<MeshFilter>(go);
					if (collider is BoxCollider)
					{
						if (tempCubeMesh == null)
						{
							GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
							tempCubeMesh = tempGO.GetComponent<MeshFilter>().sharedMesh;
							Object.DestroyImmediate(tempGO);
						}
						meshFilter.sharedMesh = tempCubeMesh;
					}
					else if (collider is SphereCollider)
					{
						if (tempSphereMesh == null)
						{
							GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
							tempSphereMesh = tempGO.GetComponent<MeshFilter>().sharedMesh;
							Object.DestroyImmediate(tempGO);
						}
						meshFilter.sharedMesh = tempSphereMesh;
					}
					else if (collider is CapsuleCollider)
					{
						if (tempCapsuleMesh == null)
						{
							GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
							tempCapsuleMesh = tempGO.GetComponent<MeshFilter>().sharedMesh;
							Object.DestroyImmediate(tempGO);
						}
						meshFilter.sharedMesh = tempCapsuleMesh;
					}
					EditorUtility.SetDirty(go);
					count++;
				}
			}
			Debug.Log("Added temporary renderers to " + count + " colliders.");
		}
		else if (removeMode)
		{
			Undo.RecordObjects(selected, "Remove Temp renderers");
			int count = 0;
			for (int i = 0, selectedLength = selected.Length; i < selectedLength; i++)
			{
				GameObject go = selected[i] as GameObject;
				bool hasCollider = (go.GetComponent<Collider>() != null);
				if (!hasCollider)
					continue;

				MeshFilter meshFilter = go.GetComponent<MeshFilter>();
				MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();
				if (meshFilter != null && meshRenderer != null)
				{
					Undo.DestroyObjectImmediate(meshFilter);
					Undo.DestroyObjectImmediate(meshRenderer);
					EditorUtility.SetDirty(go);
					count++;
				}
			}
			Debug.Log("Removed temporary renderers from " + count + " colliders.");
		}
		else
		{
			Debug.Log("No selected object has colliders.");
		}
	}
}
