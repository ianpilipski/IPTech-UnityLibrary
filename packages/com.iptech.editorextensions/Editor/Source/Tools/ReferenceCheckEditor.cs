using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace IPTech.EditorTools {
	public class ReferenceCheckEditor : EditorWindow {

		const string HELP_MESSAGE = "This tool will open each scene in the project and iterate over each GameObject " + 
			"in the scene to find if any components are missing a reference to the object they should have been pointing at.";

		private Action currentAction = null;
		private string[] Scenes = null;
		private IList<string> MissingReferences = null;

		[MenuItem("IPTech/Project/Reference Check Editor ...")]
		public static void MenuItemReferenceCheckEditor() {
			ReferenceCheckEditor editorWindow = EditorWindow.GetWindow<ReferenceCheckEditor>();
			editorWindow.Show();
		}

		private void Update() {
			ConditionalPerformAction();
		}

		private void ConditionalPerformAction() {
			if(HasAction()) {
				InvokeAction();
				ClearAction();
			}
		}

		private bool HasAction() {
			return this.currentAction != null;
		}

		private void InvokeAction() {
			currentAction.Invoke();
		}

		private void ClearAction() {
			this.currentAction = null;
		}

		private void OnGUI() {
			EditorGUILayout.HelpBox(HELP_MESSAGE, MessageType.Info);

			if(GUILayout.Button("Run Reference Check")) {
				currentAction = RunReferenceCheck;
			}

			if(this.MissingReferences != null) {
				if(this.MissingReferences.Count == 0) {
					GUILayout.Label("Did not detect any missing references!");
				} else {
					GUILayout.Label("Missing References:");
					foreach(string missingRef in this.MissingReferences) {
						GUILayout.Label(string.Format("{0} has a missing reference", missingRef));
					}
				}
			}
		}

		private void RunReferenceCheck() {
			if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
				this.MissingReferences = new List<string>();
				IterateScenes();
			}
		}

		private void IterateScenes() {
			PopulateScenesArray();
			foreach(string scenePath in Scenes) {
				EditorSceneManager.OpenScene(scenePath);
				GameObject[] gameObjects = FindObjectsOfType<GameObject>();
				foreach(GameObject go in gameObjects) {
					Component[] c = go.GetComponents<Component>();
					if(c == null) {
						this.MissingReferences.Add(go.name);
					}
				}
			}
		}

		private void PopulateScenesArray() {
			string[] sceneObjectGuids = AssetDatabase.FindAssets("t:scene");

			List<string> paths = new List<string>();
			foreach(string guid in sceneObjectGuids) {
				paths.Add(AssetDatabase.GUIDToAssetPath(guid));
			}
			this.Scenes = paths.ToArray();
		}

	}
}