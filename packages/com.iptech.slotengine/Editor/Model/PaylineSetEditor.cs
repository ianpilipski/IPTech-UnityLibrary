using IPTech.SlotEngine.Model;
using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Unity.Model.Editor.Api;
using UnityEditor;
using UnityEngine;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	class PaylineSetEditor : IPaylineSetEditor
	{
		public ObjectToEditorMap<IPayline, IPaylineEditor> objectToEditorMap => Context.Inst.PaylineEditorMap;

		public bool OnInspectorGUI(object targetObject) {
			return OnInspectorGUI((IPaylineSet)targetObject);
		}

		public bool OnInspectorGUI(IPaylineSet paylineSet) {
			bool modified = false;

			for(int i=0; i<paylineSet.PayLines.Count; i++) {
				EditorGUILayout.BeginHorizontal();
				IPayline payline = paylineSet.PayLines[i];

				if (GUILayout.Button("Delete",GUILayout.MinHeight(50.0f))) {
					paylineSet.PayLines.Remove(payline);
					modified = true;
				}

				EditorGUILayout.BeginVertical();
				IPaylineEditor paylineEditor = this.objectToEditorMap.GetOrCreateEditorFor(payline);
				modified = paylineEditor.OnInspectorGUI(payline) || modified;
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}
			this.objectToEditorMap.RemoveMappingsNotInCollection(paylineSet.PayLines);

			if(GUILayout.Button("Add Payline")) {
				paylineSet.PayLines.Add(new Payline());
			}


			return modified;
		}
	}
}
