using IPTech.SlotEngine.Model;
using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Unity.Model.Editor.Api;
using strange.extensions.injector.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	class PayoutTableEditor : IPayoutTableEditor
	{
		[Inject]
		public IInjectionBinder injectionBinder { get; set; }

		[Inject]
		public ObjectToEditorMap<IPayoutTableEntry, IPayoutTableEntryEditor> objectToEditorMap { get; set; }

		public PayoutTableEditor() {
		}

		public bool OnInspectorGUI(object targetObject) {
			return OnInspectorGUI((IPayoutTable)targetObject);
		}

		private bool OnInspectorGUI(IPayoutTable payoutTable) {
			bool modified = false;

			for(int i=0; i<payoutTable.PayoutTableEntries.Count; i++) {
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Delete", GUILayout.MinHeight(40.0f))) {
					payoutTable.PayoutTableEntries.RemoveAt(i);
					modified = true;
				} else {
					EditorGUILayout.BeginVertical();
					IPayoutTableEntry payoutTableEntry = payoutTable.PayoutTableEntries[i];
					IPayoutTableEntryEditor payoutTableEntryEditor = this.objectToEditorMap.GetOrCreateEditorFor(payoutTableEntry);
					modified = payoutTableEntryEditor.OnInspectorGUI(payoutTableEntry) || modified;
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndHorizontal();
			}
			this.objectToEditorMap.RemoveMappingsNotInCollection(payoutTable.PayoutTableEntries);
			
			if(GUILayout.Button("Add Payout Entry")) {
				payoutTable.PayoutTableEntries.Add(new PayoutTableEntry());
				modified = true;
			}

			return modified;
		}
	}
}
