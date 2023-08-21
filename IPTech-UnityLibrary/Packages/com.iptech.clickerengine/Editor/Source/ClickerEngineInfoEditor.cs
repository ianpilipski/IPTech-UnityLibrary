using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace IPTech.ClickerLibrary {
	[CustomEditor(typeof(ClickerEngineInfo))]
	public class ClickerEngineInfoEditor : Editor {
		
		[MenuItem("Assets/Create/ClickerEngine/Create New Engine")]
		static void CreateNewClickerEngine() {
			CreateNewAsset<ClickerEngineInfo>("NewClickerEngineInfo");
		}
		
		[MenuItem("Assets/Create/ClickerEngine/Create New Clicker")]
		static void CreateNewClicker() {
			CreateNewAsset<ClickerInfo>("NewClickerInfo");
		}
		
		private Vector2 scrollPos;
		override public void OnInspectorGUI() {
			DrawDefaultInspector();
			
			ClickerEngineInfo info = (ClickerEngineInfo)target;
			EditorGUILayout.Separator();
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
			EditorGUILayout.LabelField("Calculated Values");
			for(int i=1;i<100;i++) {
				float cost = info.GetLevelCost(i);
				EditorGUILayout.LabelField(i.ToString (), cost.ToString ());
			}
			EditorGUILayout.EndScrollView();
		}
		
		#region helper functions
		static void CreateNewAsset<AssetType>(string AssetName) where AssetType : ScriptableObject {
			ScriptableObject asset = ScriptableObject.CreateInstance<AssetType>();
			string path = GetCurrentAssetPath();
			
			int count = 1;
			string newname = string.Concat(AssetName, ".asset");
			while(File.Exists(Path.Combine(path,newname))) {
				newname = string.Concat (AssetName, count, ".asset");
				count++;
			}
			
			AssetDatabase.CreateAsset(asset,Path.Combine(path,newname));
			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
		}
		
		static string GetCurrentAssetPath() {
			string path = "Assets";
			foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
			{
				path = AssetDatabase.GetAssetPath(obj);
				if (File.Exists(path))
				{
					path = Path.GetDirectoryName(path);
				}
				break;
			}
			return path;
		}
		#endregion
	}
}
