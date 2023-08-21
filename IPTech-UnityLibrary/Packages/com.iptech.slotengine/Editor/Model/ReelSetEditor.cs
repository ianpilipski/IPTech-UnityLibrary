using UnityEngine;
using UnityEditor;
using IPTech.SlotEngine.Unity.Model.Editor.Api;
using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Model;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	public class ReelSetEditor : IReelSetEditor
	{
		public ObjectToEditorMap<IReel,IReelEditor> objectToEditorMap => Context.Inst.ReelEditorMap;

		public bool OnInspectorGUI(object targetObject) {
			return OnInspectorGUI((IReelSet)targetObject);
		}

		private bool OnInspectorGUI(IReelSet reelSet) { 
			bool modified = false;

			for (int i=0;i<reelSet.Count;i++) {
				IReel reel = reelSet[i];
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Delete", GUILayout.ExpandHeight(true))) {
					reelSet.Remove(reel);
					modified = true;
				}

				modified = EditorForReel(reel) || modified;
				EditorGUILayout.EndHorizontal();
			}
			this.objectToEditorMap.RemoveMappingsNotInCollection(reelSet);

			if (GUILayout.Button("Add Reel")) {
				reelSet.Add(new Reel());
				modified = true;
			}
			return modified;
		}

		private bool EditorForReel(IReel reel) {
			bool modified = false;
			IReelEditor reelEditor = this.objectToEditorMap.GetOrCreateEditorFor(reel);
			EditorGUILayout.BeginVertical();
			modified = reelEditor.OnInspectorGUI(reel);
			EditorGUILayout.EndVertical();
			return modified;
		}
	}
}
