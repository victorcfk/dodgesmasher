using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SingleUnityLayer))]
public class SingleUnityLayerPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, GUIContent.none, property);
		SerializedProperty layerIndex = property.FindPropertyRelative("m_LayerIndex");
		if (layerIndex.hasMultipleDifferentValues) label.text += " (Multi values)";
		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
		if (layerIndex != null)
		{
			EditorGUI.BeginChangeCheck();
			int tempInt = EditorGUI.LayerField(position, layerIndex.intValue);
			if (EditorGUI.EndChangeCheck()) layerIndex.intValue = tempInt;
			EditorGUI.showMixedValue = false;
		}
		EditorGUI.EndProperty();
	}
}
