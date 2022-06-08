using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MSPlayground.Core
{
	[CustomPropertyDrawer(typeof(InspectorReadOnlyAttribute))]
	public class InspectorReadOnlyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = true;
		}
	}
}


