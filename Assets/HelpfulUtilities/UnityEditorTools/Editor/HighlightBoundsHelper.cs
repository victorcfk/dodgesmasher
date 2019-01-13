/// <summary>
/// Highlight helper. Taken from http://www.toxicfork.com/181/unity3d-highlight-helper-script
/// </summary>
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class HighlightBoundsHelper {

	static readonly Type HierarchyWindowType;

	static int _hoveredInstance;

	static readonly Color HoverColor = new Color(0, 1, 1, 0.75f);
	static readonly Color DragColor = new Color(1, 0, 0, 1);

	static bool highlightHelperEngaged = false;

	[MenuItem("Tools/Toggle Highlight Bounds Helper %m", false, 12)]
	static void toggleHighlightBoundsHelper()
	{
		highlightHelperEngaged = !highlightHelperEngaged;
	}

	static HighlightBoundsHelper() {
		EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
		
		EditorApplication.update += EditorUpdate;
		
		SceneView.onSceneGUIDelegate += OnSceneGUIDelegate;
		
		Assembly editorAssembly = typeof (EditorWindow).Assembly;
		HierarchyWindowType = editorAssembly.GetType("UnityEditor.SceneHierarchyWindow");
	}
	
	static void EditorUpdate() {
		var currentWindow = EditorWindow.mouseOverWindow;
		if (currentWindow && currentWindow.GetType() == HierarchyWindowType) {
			if (!currentWindow.wantsMouseMove) {
				//allow the hierarchy window to use mouse move events!
				currentWindow.wantsMouseMove = true;
			}
		} else {
			_hoveredInstance = 0;
		}
	}
	
	static void OnSceneGUIDelegate(SceneView sceneView) {
		switch (Event.current.type) {
			case EventType.DragUpdated:
			case EventType.DragPerform:
			case EventType.DragExited:
				sceneView.Repaint();
				break;
		}
		
		if (Event.current.type == EventType.Repaint) {
			var drawnInstanceIDs = new HashSet<int>();
			
			Color handleColor = Handles.color;
			
			Handles.color = DragColor;
			foreach (var objectReference in DragAndDrop.objectReferences) {
				var gameObject = objectReference as GameObject;
				
				if (gameObject)// && gameObject.activeInHierarchy)
				{
					DrawObjectBounds(gameObject);
					
					drawnInstanceIDs.Add(gameObject.GetInstanceID());
				}
			}
			
			Handles.color = HoverColor;

			if (_hoveredInstance != 0 && !drawnInstanceIDs.Contains(_hoveredInstance)) {
				GameObject sceneGameObject = EditorUtility.InstanceIDToObject(_hoveredInstance) as GameObject;
				
				if (sceneGameObject) {
					DrawObjectBounds(sceneGameObject);
				}
			}
			
			Handles.color = handleColor;
		}
	}
	
	static void DrawObjectBounds(GameObject sceneGameObject) {
		if(!highlightHelperEngaged) return;

		if(sceneGameObject.transform.childCount >0)
		{
			for(int i=0; i < sceneGameObject.transform.childCount; i++)
			{
				DrawObjectBounds((sceneGameObject.transform.GetChild(i)).gameObject);
			}
		}

		var bounds = new Bounds(sceneGameObject.transform.position, Vector3.one);

		if(sceneGameObject.GetComponents<Collider> ().Length > 0)
		for (int i = 0; i < sceneGameObject.GetComponents<Collider> ().Length; i++) {
			Collider collider = sceneGameObject.GetComponents<Collider> () [i];

			bounds.Encapsulate (collider.bounds);
		}
		else
		for (int i = 0; i < sceneGameObject.GetComponents<Renderer> ().Length; i++) {
			Renderer renderer = sceneGameObject.GetComponents<Renderer> () [i];
			
			bounds.Encapsulate (renderer.bounds);
		}

		if(sceneGameObject.GetComponents<Collider>() != null)
			DrawBoundingBox(bounds);

//		float onePixelOffset = HandleUtility.GetHandleSize(bounds.center)*1/64f;
//		
//		float circleSize = bounds.size.magnitude*0.5f;
//
//		Quaternion angles;
//		if(Camera.current)
//			angles = Camera.current.transform.rotation;
//		else
//			angles =  Quaternion.Euler(90,0,90);

//		Handles.ArrowCap(0,bounds.center,angles,10);

		//Handles.color = Color.cyan;
//		Handles.RectangleCap(0,bounds.center,sceneGameObject.transform.rotation,bounds.size.magnitude*0.5f);
//		Handles.RectangleCap(0,bounds.center,sceneGameObject.transform.rotation,bounds.size.magnitude*0.5f+onePixelOffset);
//		Handles.RectangleCap(0,bounds.center,sceneGameObject.transform.rotation,bounds.size.magnitude*0.5f-onePixelOffset);

//		Handles.
//		Handles.CircleCap(0, bounds.center,
//		                  angles, circleSize - onePixelOffset);
//		Handles.CircleCap(0, bounds.center,
//		                  angles, circleSize + onePixelOffset);
//		Handles.CircleCap(0, bounds.center, angles, circleSize);
	}

	static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect) {
		var current = Event.current;
		
		switch (current.type) {
			case EventType.MouseMove:
				if (selectionRect.Contains(current.mousePosition)) {
					if (_hoveredInstance != instanceID) {
						_hoveredInstance = instanceID;
						if (SceneView.lastActiveSceneView) {
							SceneView.lastActiveSceneView.Repaint();
						}
					}
				} else {
					if (_hoveredInstance == instanceID) {
						_hoveredInstance = 0;
						if (SceneView.lastActiveSceneView) {
							SceneView.lastActiveSceneView.Repaint();
						}
					}
				}
				break;
			case EventType.MouseDrag:
			case EventType.DragUpdated:
			case EventType.DragPerform:
			case EventType.DragExited:
				if (SceneView.lastActiveSceneView) {
					SceneView.lastActiveSceneView.Repaint();
				}
				break;
		}
	}

	static void DrawBoundingBox(Bounds bounds)
	{
		Vector3 v3Center = bounds.center;
		Vector3 v3Extents = bounds.extents;
		
		Vector3 v3FrontTopLeft      = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  
		Vector3 v3FrontTopRight     = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  
		Vector3 v3FrontBottomLeft   = bounds.min;//new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  
		Vector3 v3FrontBottomRight  = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  
		
		Vector3 v3BackTopLeft       = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top left corner
		Vector3 v3BackTopRight      = bounds.max;//new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  
		Vector3 v3BackBottomLeft    = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  
		Vector3 v3BackBottomRight   = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  
		
		Vector3[] list = new Vector3[24];
		
		list[0] = (v3FrontTopLeft);
		list[1] = (v3FrontTopRight);   
		list[2] = (v3FrontTopRight);
		list[3] = (v3FrontBottomRight);    
		list[4] = (v3FrontBottomRight);
		list[5] = (v3FrontBottomLeft);
		list[6] = (v3FrontBottomLeft);
		list[7] = (v3FrontTopLeft);
		list[8] = (v3BackTopLeft);
		list[9] = (v3BackTopRight);
		list[10] = (v3BackTopRight);
		list[11] = (v3BackBottomRight);
		list[12] = (v3BackBottomRight);
		list[13] = (v3BackBottomLeft);
		list[14] = (v3BackBottomLeft);
		list[15] = (v3BackTopLeft);
		list[16] = (v3FrontTopLeft);
		list[17] = (v3BackTopLeft);    
		list[18] = (v3FrontTopRight);
		list[19] = (v3BackTopRight);
		list[20] = (v3FrontBottomRight);
		list[21] = (v3BackBottomRight);
		list[22] = (v3FrontBottomLeft);
		list[23] = (v3BackBottomLeft);
		
		//			points[0] = bounds.min;
		//			points[1] = new Vector3(bounds.min.x + bounds.extents.x*2,bounds.min.y,bounds.min.z);
		//			points[2] = new Vector3(bounds.min.x,bounds.min.y+ bounds.extents.y*2,bounds.min.z);
		//			points[3] = new Vector3(bounds.min.x,bounds.min.y,bounds.min.z+ bounds.extents.z*2);
		//
		//			points[4] = new Vector3(bounds.max.x - bounds.extents.x*2,bounds.max.y,bounds.max.z);
		//			points[5] = new Vector3(bounds.max.x,bounds.max.y - bounds.extents.y*2 ,bounds.max.z);
		//			points[6] = new Vector3(bounds.max.x,bounds.max.y,bounds.max.z - bounds.extents.z*2);
		//			points[7] = bounds.max;
		
		Handles.DrawLines(list);
	}
}