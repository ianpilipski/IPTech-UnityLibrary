using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Unity.Model.Editor.Api;
using UnityEditor;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	class PaylineEntryEditor : IPaylineEntryEditor
	{
		public PaylineEntryEditor() { }

		public bool OnInspectorGUI(object targetObject) {
			return OnInspectorGUI((IPaylineEntry)targetObject);
		}

		public bool OnInspectorGUI(IPaylineEntry paylineEntry) {
			bool modified = false;

			EditorGUILayout.BeginVertical();
			int newReelColumn = EditorGUILayout.IntField(paylineEntry.ReelColumn);
			if(newReelColumn!=paylineEntry.ReelColumn) {
				paylineEntry.ReelColumn = newReelColumn;
				modified = true;
			}

			int newReelRow = EditorGUILayout.IntField(paylineEntry.ReelRow);
			if(newReelRow!=paylineEntry.ReelRow) {
				paylineEntry.ReelRow = newReelRow;
				modified = true;
			}
			EditorGUILayout.EndVertical();

			return modified;
		}
	}
}
