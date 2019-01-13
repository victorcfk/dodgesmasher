/*
 * http://www.alanzucconi.com/2015/07/22/how-to-snap-to-grid-in-unity3d/
 */

using UnityEngine;
using System.Collections;

public class SnapToGrid : MonoBehaviour {
	#if UNITY_EDITOR
	[InspectorReadOnlyAttribute]
	public bool snapToGrid = true;
	[InspectorReadOnlyAttribute]
	public float snapValue = 2;
	
	[InspectorReadOnlyAttribute]
	public bool sizeToGrid = true;
	[InspectorReadOnlyAttribute]
	public float sizeValues = 1f;

	[InspectorReadOnlyAttribute]
	public bool snapXAxis = false;
	[InspectorReadOnlyAttribute]
	public float snapXAxisValue = 0;

	[InspectorReadOnlyAttribute]
	public bool snapYAxis = true;
	[InspectorReadOnlyAttribute]
	public float snapYAxisValue = 0;

	[InspectorReadOnlyAttribute]
	public bool snapZAxis = false;
	[InspectorReadOnlyAttribute]
	public float snapZAxisValue = 0;

	[InspectorReadOnlyAttribute]
	public bool rotateToGrid = true;
	[InspectorReadOnlyAttribute]
	public float rotValue = 15;

	#endif
}