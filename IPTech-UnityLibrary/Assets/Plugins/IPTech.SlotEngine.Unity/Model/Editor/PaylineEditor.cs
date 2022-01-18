using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Unity.Model.Editor;
using IPTech.SlotEngine.Unity.Model.Editor.Api;
using IPTech.SlotEngine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	class PaylineEditor : IPaylineEditor
	{
		[Inject]
		public IPaylineEntryEditor paylineEntryEditor { get; set; }

		private Vector2 scrollPosition = new Vector2();

		public PaylineEditor() {
			
		}

		public bool OnInspectorGUI(object targetObject) {
			return OnInspectorGUI((IPayline)targetObject);
		}

		private bool OnInspectorGUI(IPayline payline) {
			bool modified = false;

			if (payline.PaylineEntries != null) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField("Column", GUILayout.MaxWidth(50.0f));
				EditorGUILayout.LabelField("Row", GUILayout.MaxWidth(50.0f));
				EditorGUILayout.EndVertical();

				this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
				EditorGUILayout.BeginHorizontal();
				for (int i = 0; i < payline.PaylineEntries.Count; i++) {
					EditorGUILayout.BeginVertical();
					IPaylineEntry paylineEntry = payline.PaylineEntries[i];
					try {
						modified = this.paylineEntryEditor.OnInspectorGUI(paylineEntry) || modified;
					} catch { }
					if (GUILayout.Button("-")) {
						payline.PaylineEntries.RemoveAt(i);
						modified = true;
					}
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndScrollView();

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Add", GUILayout.MinHeight(50.0f))) {
					payline.PaylineEntries.Add(new PaylineEntry());
					modified = true;
				}
				EditorGUILayout.EndHorizontal();
			}

			return modified;
		}
	}
}
