using System;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace IPTech.EditorExtensions {
	[ScriptedImporter(1, "tt")]
	public class T4Importer : ScriptedImporter {
		public enum ProcessingType {
			Generator,
			Preprocessor
		}

		public ProcessingType CustomTool;
		public string CustomToolNamespace;

		public override void OnImportAsset(AssetImportContext ctx) {
			//AssetDatabase.
		}
	}
}
