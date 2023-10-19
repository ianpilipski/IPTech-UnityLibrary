using UnityEngine;
using UnityEditor;
using System;

namespace IPTech.PackageCreator.Editor {
	
	public class CreatePackageModalWindow : EditorWindow {
		PackageInfoScriptableObject packageInfoScriptableObject;
		SerializedObject serializedObject;
		Vector2 scrollPos;

		[MenuItem("Assets/Create/IPTech/New Package")]
		static void Menu() {
			var win = EditorWindow.GetWindow<CreatePackageModalWindow>(true, "Create Package");
            var winSize = new Vector2(640, 640);
            
			win.position = new Rect(new Vector2((Screen.currentResolution.width - winSize.x)/2, (Screen.currentResolution.height - winSize.y)/2), winSize);
			win.ShowModalUtility();
		}

		void Awake() {
			var prodName = Application.productName;
			if(string.IsNullOrEmpty(prodName)) {
				prodName = "noname";
            }
			
			if(packageInfoScriptableObject==null) {
				packageInfoScriptableObject = CreateInstance<PackageInfoScriptableObject>();
			}
			packageInfoScriptableObject.packageInfo = new PackageInfo() {
				name = $"com.{prodName.ToLower()}.newpackage",
				AssemblyDefName = $"{prodName}.NewPackage",
				displayName = $"{prodName}.NewPackage",
			};

			serializedObject = new SerializedObject(packageInfoScriptableObject);
		}

		void OnGUI() {
			serializedObject.Update();
			using(var scroll = new EditorGUILayout.ScrollViewScope(scrollPos)) {
				packageInfoScriptableObject.packageInfo.AssemblyDefName = EditorGUILayout.TextField("AssemblyDefName", packageInfoScriptableObject.packageInfo.AssemblyDefName);
				EditorGUILayout.PropertyField(serializedObject.FindProperty("packageInfo"),true);
				scrollPos = scroll.scrollPosition;
			}
			serializedObject.ApplyModifiedProperties();

			GUILayout.FlexibleSpace();
			using (new EditorGUILayout.HorizontalScope()) {
				if(GUILayout.Button("Cancel")) {
					Close();
				}
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Create")) {
					try {
						PackageGenerator.CreatePackage(packageInfoScriptableObject.packageInfo);
						Close();
					} catch(Exception e) {
						EditorUtility.DisplayDialog("Error", e.ToString(), "ok");
					}
				}
			}
		}
	}
}
