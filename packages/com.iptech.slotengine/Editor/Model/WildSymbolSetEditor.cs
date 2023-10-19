using IPTech.SlotEngine.Model;
using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Unity.Model.Editor.Api;
using UnityEditor;
using UnityEngine;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	class WildSymbolSetEditor : IWildSymbolSetEditor
	{
		public ObjectToEditorMap<IWildSymbol,IWildSymbolEditor> objectToEditorMap => Context.Inst.WildSymbolEditorMap;

		public bool OnInspectorGUI(object targetObject) {
			return OnInspectorGUI((IWildSymbolSet)targetObject);
		}

		public bool OnInspectorGUI(IWildSymbolSet wildSymbolSet) {
			bool modified = false;

			for(int i=0;i<wildSymbolSet.WildSymbols.Count;i++) {
				EditorGUILayout.BeginHorizontal();
				IWildSymbol wildSymbol = wildSymbolSet.WildSymbols[i];

				if (GUILayout.Button("Delete",GUILayout.ExpandHeight(true))) {
					wildSymbolSet.WildSymbols.RemoveAt(i);
					modified = true;
				}

				IWildSymbolEditor wildSymbolEditor = objectToEditorMap.GetOrCreateEditorFor(wildSymbol);
				modified = wildSymbolEditor.OnInspectorGUI(wildSymbol) || modified;
				EditorGUILayout.EndHorizontal();
			}
			objectToEditorMap.RemoveMappingsNotInCollection(wildSymbolSet.WildSymbols);

			if(GUILayout.Button("Add Wild Symbol")) {
				wildSymbolSet.WildSymbols.Add(new WildSymbol());
			}

			return modified;
		}
	}
}
