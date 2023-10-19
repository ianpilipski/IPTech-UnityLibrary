using System;
using UnityEditor;


namespace IPTech.EditorExtensions {
	[UnityEditor.AssetImporters.ScriptedImporter(1, "tt")]
	public class T4Importer : UnityEditor.AssetImporters.ScriptedImporter {
		public enum ProcessingType {
			Generator,
			Preprocessor
		}

		public ProcessingType CustomTool;
		public string CustomToolNamespace;

		public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx) {
			//AssetDatabase.
		}
	}
}
