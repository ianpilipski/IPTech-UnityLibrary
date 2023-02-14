using UnityEngine;
using UnityEditor;
using System;

namespace IPTech.PackageCreator.Editor {
	
	public class CreatePackageModalWindow : EditorWindow {
		PackageInfo packageInfo;

		[MenuItem("Assets/Create/IPTech/New Package")]
		static void Menu() {
			var win = EditorWindow.GetWindow<CreatePackageModalWindow>(true, "Create Package");
            var winSize = new Vector2(320, 120);
            
			win.position = new Rect(new Vector2((Screen.currentResolution.width - winSize.x)/2, (Screen.currentResolution.height - winSize.y)/2), winSize);
			win.ShowModalUtility();
		}

		void Awake() {
			var prodName = Application.productName;
			if(string.IsNullOrEmpty(prodName)) {
				prodName = "noname";
            }
			packageInfo = new PackageInfo() {
				name = $"com.{prodName.ToLower()}.newpackage",
				AssemblyDefName = $"{prodName}.NewPackage",
				displayName = $"{prodName}.NewPackage",
			};
		}

		void OnGUI() {
			packageInfo.name = EditorGUILayout.TextField("Package Name", packageInfo.name);
			packageInfo.AssemblyDefName = EditorGUILayout.TextField("Assembly Name", packageInfo.AssemblyDefName);
			packageInfo.displayName = EditorGUILayout.TextField("Display Name", packageInfo.displayName);
			GUILayout.FlexibleSpace();
			using (new EditorGUILayout.HorizontalScope()) {
				if(GUILayout.Button("Cancel")) {
					Close();
				}
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Create")) {
					try {
						PackageGenerator.CreatePackage(packageInfo);
						Close();
					} catch(Exception e) {
						EditorUtility.DisplayDialog("Error", e.ToString(), "ok");
					}
				}
			}
		}
	}
}
