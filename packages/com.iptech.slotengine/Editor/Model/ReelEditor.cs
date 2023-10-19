using UnityEditor;
using UnityEngine;
using IPTech.SlotEngine.Unity.Model.Editor.Api;
using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Model;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	public class ReelEditor : IReelEditor
	{
		private const float SYMBOL_GROUP_MAXWIDTH = 20.0f;

		public ISymbolEditor symbolEditor => Context.Inst.SymbolEditor;

		private Vector2 scrollPosition;

		public ReelEditor() {
			scrollPosition = new Vector2();
		}

		public bool OnInspectorGUI(object targetObject) {
			bool modified = false;

			EditorGUILayout.Separator();

			IReel reel = (IReel)targetObject;

			string newReelID = EditorGUILayout.TextField("Reel ID", reel.ID);
			if(newReelID!=reel.ID) {
				reel.ID = newReelID;
				modified = true;
			}

			EditorGUILayout.BeginHorizontal();
			
			DrawReelHeader();
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));
			EditorGUILayout.BeginHorizontal();
			for(int i=0;i<reel.Count;i++) {
				EditorGUILayout.BeginVertical(GUILayout.MaxWidth(SYMBOL_GROUP_MAXWIDTH));
				ISymbol symbol = reel[i];
				modified = symbolEditor.OnInspectorGUI(symbol) || modified;
				if (GUILayout.Button("-")) {
					reel.Remove(symbol);
					modified = true;
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndScrollView();
			//GUILayout.FlexibleSpace();

			if (GUILayout.Button("Add Symbol", GUILayout.ExpandHeight(true))) {
				reel.Add(new Symbol());
				modified = true;
			}
			EditorGUILayout.EndHorizontal();

			return modified;
		}

		private void DrawReelHeader() {
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("ID", GUILayout.Width(50.0f));
			EditorGUILayout.LabelField("Weight", GUILayout.Width(50.0f));
			EditorGUILayout.EndVertical();
		}
	}
}
