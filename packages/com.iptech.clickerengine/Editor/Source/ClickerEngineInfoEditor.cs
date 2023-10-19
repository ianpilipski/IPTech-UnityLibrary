using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace IPTech.ClickerLibrary {
	[CustomEditor(typeof(ClickerEngineInfo))]
	public class ClickerEngineInfoEditor : Editor {
		Vector2 scrollPos;

		override public void OnInspectorGUI() {
			DrawDefaultInspector();
			
			ClickerEngineInfo info = (ClickerEngineInfo)target;
			EditorGUILayout.Separator();
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			EditorGUILayout.LabelField("Calculated Values");
			for(int i=1;i<100;i++) {
				float cost = info.GetLevelCost(i);
				EditorGUILayout.LabelField(i.ToString (), cost.ToString ());
			}
			EditorGUILayout.EndScrollView();
		}
	}
}
