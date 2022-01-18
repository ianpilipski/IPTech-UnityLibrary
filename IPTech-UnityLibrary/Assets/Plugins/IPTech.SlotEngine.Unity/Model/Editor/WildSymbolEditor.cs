using IPTech.SlotEngine.Unity.Model.Editor.Api;
using IPTech.SlotEngine.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	class WildSymbolEditor : IWildSymbolEditor
	{
		public bool OnInspectorGUI(object targetObject) {
			return OnInspectorGUI((IWildSymbol)targetObject);
		}

		public bool OnInspectorGUI(IWildSymbol wildSymbol) {
			bool modified = false;

			string newSymbolID = DrawSymbolEditor(wildSymbol.ID);
			if(newSymbolID!=wildSymbol.ID) {
				wildSymbol.ID = newSymbolID;
				modified = true;
			}

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
