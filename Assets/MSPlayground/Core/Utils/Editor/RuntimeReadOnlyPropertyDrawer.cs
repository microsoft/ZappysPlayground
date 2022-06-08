using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MSPlayground.Core
{
	[CustomPropertyDrawer(typeof(RuntimeReadOnlyAttribute))] 
	public class RuntimeReadOnlyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = !Application.isPlaying;
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = true;
		}
	}
}


