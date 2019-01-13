using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(TextBoxAttribute))]
public class TextBoxAttributeDrawer : PropertyDrawer
{
	static float width = 100;
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (property.propertyType == SerializedPropertyType.String)
			return EditorStyles.textArea.CalcHeight(new GUIContent(property.stringValue), width - 30) + 16;

		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		width = position.width;
		SerializedPropertyType t = property.propertyType;
		if (t == SerializedPropertyType.String)
		{
			Rect r = position;
			r.height = 16;
			if (property.hasMultipleDifferentValues)
			{
				label.text += " (multiple values)";
				EditorGUI.LabelField(r, label);
			}
			else
				EditorGUI.LabelField(r, label);

			r.height = position.height - 16;
			r.y += 16;
			property.stringValue = EditorGUI.TextField(r, property.stringValue, EditorStyles.textArea);
		}
		else
		{
			EditorGUI.PropertyField(position, property, label, true);
		}
	}
}
