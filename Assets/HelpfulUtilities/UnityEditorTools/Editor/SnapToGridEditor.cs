/*
 * http://www.alanzucconi.com/2015/07/22/how-to-snap-to-grid-in-unity3d/
 */
using UnityEngine;
using UnityEditor;
using System.Collections;

[InitializeOnLoad]
[CustomEditor(typeof(SnapToGrid), true)]
[CanEditMultipleObjects]
public class SnapToGridEditor : Editor {
	
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		
		SnapToGrid actor = target as SnapToGrid;

		if (actor.snapToGrid)
			actor.transform.position = RoundTransform (actor.transform.position, actor.snapValue);
		
		if (actor.sizeToGrid)
			actor.transform.localScale = RoundTransform(actor.transform.localScale, actor.sizeValues);

		if (actor.rotateToGrid)
		{
			actor.transform.rotation = Quaternion.Euler(
				RoundTransformSnapXZ(actor.transform.rotation.eulerAngles, actor.rotValue));

		}

		if (actor.snapXAxis)
			actor.transform.position = new Vector3(actor.snapXAxisValue,
			                                       actor.transform.position.y,
			                                       actor.transform.position.z);
		
		if (actor.snapYAxis)
			actor.transform.position = new Vector3(actor.transform.position.x,
			                                       actor.snapYAxisValue,
			                                       actor.transform.position.z);
		
		if (actor.snapZAxis)
			actor.transform.position = new Vector3(actor.transform.position.x,
			                                       actor.transform.position.y,
			                                       actor.snapZAxisValue);
	}
	
	// The snapping code
	Vector3 RoundTransform (Vector3 v, float snapValue)
	{
		return new Vector3
			(
				snapValue * Mathf.Round(v.x / snapValue),
				snapValue * Mathf.Round(v.y / snapValue),
				snapValue * Mathf.Round(v.z / snapValue)
				);
	}

	Vector3 RoundTransformSnapXZ (Vector3 v, float snapYValue)
	{
		return new Vector3
			(
				0,
				snapYValue * Mathf.Round(v.y / snapYValue),
				0
				);
	}
}