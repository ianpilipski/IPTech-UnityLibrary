using UnityEngine;
using UnityEditor;

namespace IPTech.EditorExtensions {
	[CustomPropertyDrawer(typeof(ScenePickerAttribute))]
	public class ScenePickerPropertyDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if(property.propertyType == SerializedPropertyType.String) {
				DrawField(position, property, label);
			}
		}

		void DrawField(Rect position, SerializedProperty property, GUIContent label) {
			var currentSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);
			//label = EditorGUI.BeginProperty(position, label, property);
			EditorGUI.BeginChangeCheck();
			Rect rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			if(IsMissingReference(property)) {
				EditorGUI.LabelField(rect, "Missing Reference", property.stringValue);
				rect.y += EditorGUIUtility.singleLineHeight;
			}
			var newSceneAsset = EditorGUI.ObjectField(rect, label, currentSceneAsset, typeof(SceneAsset), false);
			if(EditorGUI.EndChangeCheck()) {
				property.stringValue = AssetDatabase.GetAssetPath(newSceneAsset);
			}
			//EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			float baseHeight = base.GetPropertyHeight(property, label);
			if(IsMissingReference(property)) {
				return baseHeight * 2F;
			}
			return baseHeight;
		}

		bool IsMissingReference(SerializedProperty property) {
			return (!string.IsNullOrEmpty(property.stringValue) && GetSceneAssetFromPropertyString(property) == null) ;
		}

		SceneAsset GetSceneAssetFromPropertyString(SerializedProperty property) {
			return AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);
		}
	}
}

