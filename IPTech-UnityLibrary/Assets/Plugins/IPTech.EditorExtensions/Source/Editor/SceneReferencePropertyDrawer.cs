using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.EditorExtensions {
	[CustomPropertyDrawer(typeof(SceneReference))]
	public class SceneReferencePropertyDrawer : PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			PropertyBag bag = new PropertyBag(property);

			label = EditorGUI.BeginProperty(position, label, property);
			EditorGUI.ObjectField(position, bag.SceneProperty, label);
			bag.UpdateScenePath();
			EditorGUI.EndProperty();
		}

		class PropertyBag {
			readonly SerializedProperty ScenePathProperty;
			public readonly SerializedProperty SceneProperty;

			public PropertyBag(SerializedProperty property) {
				SceneProperty = property.FindPropertyRelative("_scene");
				ScenePathProperty = property.FindPropertyRelative("_scenePath");
			}

			public void UpdateScenePath() {
				if(SceneProperty.objectReferenceValue == null) {
					ScenePathProperty.stringValue = null;
				} else {
					ScenePathProperty.stringValue = AssetDatabase.GetAssetPath(SceneProperty.objectReferenceValue);
				}
			}
		}
	}
}
