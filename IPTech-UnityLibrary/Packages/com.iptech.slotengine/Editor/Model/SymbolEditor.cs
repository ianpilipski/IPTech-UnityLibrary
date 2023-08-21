using System;
using UnityEditor;
using IPTech.SlotEngine.Unity.Model.Editor.Api;
using IPTech.SlotEngine.Model.Api;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	public class SymbolEditor : ISymbolEditor
	{
		public bool OnInspectorGUI(object targetObject) {
			return OnInspectorGUI((ISymbol)targetObject);
		}

		public bool OnInspectorGUI(ISymbol symbol) {
			bool modified = false;

			EditorGUILayout.BeginVertical();
			
			string newSymbolID = DrawSymbolEditor(symbol.ID);
			if(newSymbolID!=symbol.ID) {
				symbol.ID = newSymbolID;
				modified = true;
			}

			float newWeight = EditorGUILayout.FloatField(symbol.Weight);
			if(newWeight!=symbol.Weight) {
				symbol.Weight = newWeight;
				modified = true;
			}

			EditorGUILayout.EndVertical();
			return modified;
		}

		private string DrawSymbolEditor(string symbolID) {
			ESymbolID symbolEnum = ESymbolID.A;
			try {
				symbolEnum = (ESymbolID)Enum.Parse(typeof(ESymbolID), symbolID.ToUpper());
			} catch { }
			ESymbolID newSymbolEnum = (ESymbolID)EditorGUILayout.EnumPopup(symbolEnum);
			return newSymbolEnum.ToString();
		}
	}
}
