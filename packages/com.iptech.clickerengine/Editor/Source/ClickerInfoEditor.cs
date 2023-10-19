using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace IPTech.ClickerLibrary {
	[CustomEditor(typeof(ClickerInfo))]
	public class ClickerInfoEditor : Editor {
		
		override public void OnInspectorGUI() {
			
			//DrawDefaultInspector();
			
			//Show Chart
			GUI.changed = false;
			ClickerInfo clicker = (ClickerInfo)target;
			clicker.ClickerName = EditorGUILayout.TextField("Name",clicker.ClickerName);
			clicker.ManualClicker = EditorGUILayout.Toggle("Manual Clicker", clicker.ManualClicker);
			clicker.Group = EditorGUILayout.TextField("Group", clicker.Group);

			EditorGUILayout.LabelField("Clicks Per Second");
			EditorGUILayout.LabelField("(" + clicker.CPSA.ToString() + "x * " + clicker.CPSA.ToString() + "x) + " + clicker.CPSB.ToString() + "x + " + clicker.CPSC.ToString());
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("A", GUILayout.Width(20.0f));
			clicker.CPSA = EditorGUILayout.FloatField(clicker.CPSA);// GUILayout.Width(50.0f));
			EditorGUILayout.LabelField("B", GUILayout.Width(20.0f));
			clicker.CPSB = EditorGUILayout.FloatField(clicker.CPSB);//, GUILayout.Width(50.0f));
			EditorGUILayout.LabelField("C", GUILayout.Width(20.0f));
			clicker.CPSC = EditorGUILayout.FloatField(clicker.CPSC);//, GUILayout.Width(50.0f));
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("Cost");
			EditorGUILayout.LabelField("(" + clicker.A.ToString () + "x * " + clicker.A.ToString () + "x) + " + clicker.B.ToString () + "x + " + clicker.C.ToString ());
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("A", GUILayout.Width(20.0f));
			clicker.A = EditorGUILayout.FloatField(clicker.A);// GUILayout.Width(50.0f));
			EditorGUILayout.LabelField("B", GUILayout.Width(20.0f));
			clicker.B = EditorGUILayout.FloatField(clicker.B);//, GUILayout.Width(50.0f));
			EditorGUILayout.LabelField("C", GUILayout.Width(20.0f));
			clicker.C = EditorGUILayout.FloatField(clicker.C);//, GUILayout.Width(50.0f));
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Separator();
			EditorGUILayout.LabelField("Calculated Values");
			for(int i=1;i<100;i++) {
				float cost = clicker.GetCost(i);
				float clicksPerSecond = clicker.GetClicksPerSecond(i);
				EditorGUILayout.LabelField(i.ToString () + " CPS=" + (clicksPerSecond).ToString (), cost.ToString ());
			}
			
			if(GUI.changed) {
				EditorUtility.SetDirty(target);
			}
		}
	}
}
