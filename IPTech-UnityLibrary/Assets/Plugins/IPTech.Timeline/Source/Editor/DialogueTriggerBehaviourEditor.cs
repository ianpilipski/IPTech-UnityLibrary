using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace IPTech.Timeline {

	[CustomPropertyDrawer(typeof(DialogueTriggerBehaviour), true)]
	public class DialogueTriggerBehaviourEditor : PropertyDrawer {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label, true) + 16;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			//base.OnGUI(position, property, label);
			EditorGUI.PropertyField(position, property, true);
			if(GUILayout.Button("Set Loop Start")) {
				SerializedProperty sp = property.FindPropertyRelative("LoopOffset");
				sp.doubleValue = DialogueTriggerMixerBehaviour.LastRelativeTime;
			}
			if(GUILayout.Button("Dismiss Dialog")) {
				DialogueTriggerMixerBehaviour.DebugDismissDialog = true;
			}
		}
	}
}
