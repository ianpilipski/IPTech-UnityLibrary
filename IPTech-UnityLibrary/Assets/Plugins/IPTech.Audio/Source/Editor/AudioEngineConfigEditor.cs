using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace IPTech.Audio.EditorExtensions {
	[CustomEditor(typeof(AudioEngineConfig))]	
    public class AudioEngineConfigEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			
			if(GUILayout.Button("Generate API For Config")) {
				GenerateApi((AudioEngineConfig)serializedObject.targetObject);
			}
		}

		public static void GenerateApi(AudioEngineConfig config) {
			var acg = new AudioConfigGenerator(config);
			string newClass = acg.TransformText();

			string assetPath = AssetDatabase.GetAssetPath(config);
			string generatedPath = Path.Combine(
				Path.GetDirectoryName(assetPath),
				Path.GetFileNameWithoutExtension(assetPath) + ".Generated.cs"
			);
			File.WriteAllText(generatedPath, newClass);

			AssetDatabase.ImportAsset(generatedPath);
		}
	}
}
