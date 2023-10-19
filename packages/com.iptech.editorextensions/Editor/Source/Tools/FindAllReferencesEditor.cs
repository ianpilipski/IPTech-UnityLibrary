using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;

namespace IPTech.EditorTools {
	public class FindAllReferencesEditor : EditorWindow {

		private string outputPath;

		[MenuItem("IPTech/Project/Find All References")]
		public static void MenuItemFindAllReferencesEditor() {
			FindAllReferencesEditor win = EditorWindow.GetWindow<FindAllReferencesEditor>();
			win.Show();
		}

		private void OnGUI() {
			EditorGUILayout.HelpBox("This utility will export all referenced items to a folder of your choice, for the selected folder in the project hierarchy.", MessageType.Info);
			if(GUILayout.Button("Copy all references for selected folder.")) {
				CopyAllReferencesForSelectedFolder();
			}
		}

		private void CopyAllReferencesForSelectedFolder() {
			string path = GetSelectedPath();
			if(EditorUtility.DisplayDialog("Are you sure", path, "Ok", "Cancel")) {
				this.outputPath = EditorUtility.SaveFolderPanel("Save Copy To Folder", Application.dataPath, "copy_of");
				if(!string.IsNullOrEmpty(this.outputPath)) {
					string[] referencedItems = CollectAllReferencedItems(path);
					try {
						EditorUtility.DisplayProgressBar("Copying Files", "", 0F);
						float inc = 1.0f / referencedItems.Length;
						float progress = 0F;
						foreach(string itemPath in referencedItems) {
							progress += inc;
							EditorUtility.DisplayProgressBar("Copying Files", itemPath, progress);
							if(File.Exists(itemPath) && !Directory.Exists(itemPath)) {
								string outputFolder = Path.Combine(this.outputPath, Path.GetDirectoryName(itemPath));
								string destPath = Path.Combine(this.outputPath, itemPath);
								Directory.CreateDirectory(outputFolder);
								//AssetDatabase.CopyAsset(itemPath, destPath);
								File.Copy(itemPath, destPath);
								File.Copy(itemPath + ".meta", destPath + ".meta");
							}
						}
					} catch(Exception e) {
						throw e;
					} finally {
						EditorUtility.ClearProgressBar();
					}
				}
			}
		}

		private string GetSelectedPath() {
			string path = null;
			foreach(UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets)) {
				path = AssetDatabase.GetAssetPath(obj);
				if(File.Exists(path)) {
					path = Path.GetDirectoryName(path);
				}
				break;
			}
			return path;
		}

		private string[] CollectAllReferencedItems(string path) {
			List<string> assetPaths = new List<string>();
			string[] assetGuids = AssetDatabase.FindAssets("", new string[] { path });
			foreach(string guid in assetGuids) {
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				assetPaths.Add(assetPath);
			}

			string[] deps = AssetDatabase.GetDependencies(assetPaths.ToArray(), true);

			return deps;
		}
	}
}