using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace IPTech.BuildTool.Processors
{
	[Tooltip("This move the keystore used for signing into the output gradle project, if a custom keystore is used")]
	public class MoveKeyStoreToGradleProject : BuildProcessor {
        public override void PostGenerateGradleAndroidProject(string path) {
			try {
				string outputPath = CalculateKeyStoreOutputPath();
				AndroidTools.MoveKeyStoreTo(outputPath);
			} catch(Exception e) {
				BuildToolLogger.LogException(e);
				if(!UnityEditorInternal.InternalEditorUtility.inBatchMode) {
					EditorUtility.DisplayDialog("Error Post Processing Project", e.Message, "Ok");
				} else {
					EditorApplication.Exit(1);
				}
				throw new BuildFailedException(e.Message);
			}

			string CalculateKeyStoreOutputPath() {
#if UNITY_2019_1_OR_NEWER
				return Path.GetFullPath(Path.Combine(path, "..", "launcher"));
#else
					return path;
#endif
			}
		}
	}
}
