#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;

public class DebugDrawGizmo : MonoBehaviour {

	public enum GizmoType
	{
		CircleOutline,
		Line
	}

	public GizmoType type;
	public float magnitude = 5;
	public Color gizmoColor = Color.cyan;
	public bool useObjectForward = false;
	public bool drawCenter = true;

	public void OnDrawGizmos()
	{
		Vector3 normal = Vector3.up;
		if (useObjectForward)
			normal = transform.forward;

		Handles.color = gizmoColor;

        if (drawCenter)
        {
            Handles.SphereHandleCap(0, transform.position, Quaternion.identity, 0.5f,EventType.DragUpdated);
        }

		switch (type)
		{
			case GizmoType.CircleOutline:
				Handles.DrawWireDisc(transform.position, normal, magnitude);
				break;
			case GizmoType.Line:
				Handles.DrawLine(transform.position, transform.position + magnitude * transform.forward);
				break;
		}
	}
}

#endif