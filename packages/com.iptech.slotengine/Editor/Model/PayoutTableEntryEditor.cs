using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Unity.Model.Editor.Api;
using UnityEditor;
using UnityEngine;
using System;


namespace IPTech.SlotEngine.Unity.Model.Editor
{
	class PayoutTableEntryEditor : IPayoutTableEntryEditor
	{
		private Vector2 scrollPosition;

		public enum ESymbolID
		{
			A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z
		}

		public bool OnInspectorGUI(object targetObject) {
			return OnInspectorGUI((IPayoutTableEntry)targetObject);
		}

		private bool OnInspectorGUI(IPayoutTableEntry payoutTableEntry) {
			bool modified = false;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();

			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
			EditorGUILayout.BeginHorizontal();
			for(int i=0; i<payoutTableEntry.SymbolIDList.Count; i++) {
				EditorGUILayout.BeginVertical();
				string symbolID = payoutTableEntry.SymbolIDList[i];
				string newSymbolID = EditSymbolLayout(symbolID);
				if(newSymbolID!=symbolID) {
					payoutTableEntry.SymbolIDList[i] = newSymbolID;
				}

				if (GUILayout.Button("-")) {
					payoutTableEntry.SymbolIDList.RemoveAt(i);
					modified = true;
				}
				EditorGUILayout.EndVertical();
			}

			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndScrollView();

			float newPayoutMultiplier = EditorGUILayout.FloatField("Payout Multiplier", payoutTableEntry.PayoutMultiplier);
			if (newPayoutMultiplier != payoutTableEntry.PayoutMultiplier) {
				payoutTableEntry.PayoutMultiplier = newPayoutMultiplier;
				modified = true;
			}
			EditorGUILayout.EndVertical();

			if (GUILayout.Button("Add", GUILayout.MinHeight(40.0f))) {
				payoutTableEntry.SymbolIDList.Add(ESymbolID.A.ToString());
				modified = true;
			}
			
			EditorGUILayout.EndHorizontal();

			

			return modified;
		}

		private string EditSymbolLayout(string symbolID) {
			ESymbolID symbolEnum = ESymbolID.A;
			symbolEnum = (ESymbolID)Enum.Parse(typeof(ESymbolID), symbolID.ToUpper());
			ESymbolID newSymbolEnum = (ESymbolID)EditorGUILayout.EnumPopup(symbolEnum);
			return newSymbolEnum.ToString();
		}
	}
}
