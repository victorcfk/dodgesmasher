using UnityEngine;

public class CamProjector : MonoBehaviour {
	#if UNITY_EDITOR
	[HideInInspector]
	public Vector3 rayCreationPoint;
	[HideInInspector]
	public Vector3 rayDirection;

	public MeshRenderer QuadObjPrefab;
	public bool SkewAndScale;
	public SingleUnityLayer layerOfPrefabToBeCreated = new SingleUnityLayer(9);

	void OnDrawGizmosSelected() 
	{
		if(rayCreationPoint != Vector3.zero && rayDirection != Vector3.zero)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawRay(rayCreationPoint, rayDirection);
		}
	}
	#endif
}