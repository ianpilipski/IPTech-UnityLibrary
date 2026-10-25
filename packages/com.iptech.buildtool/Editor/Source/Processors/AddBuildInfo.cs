using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IPTech.BuildTool.Processors {
	[Tooltip("This will add and IPTechBuildInfo.asset file to your Asset/Resources folder. It contains build information you can retrieve at runtime.")]
    public class AddBuildInfo : BuildProcessor {
		bool createdBuildInfo;
		string origBuildNumber;

        public override void PreprocessBuild(BuildReport report) {
			var buildInfo = IPTechBuildInfo.LoadFromResources() ?? CreateNewIPTechBuildInfo();
			string buildNumber = PlayerSettings.iOS.buildNumber;
			if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) {
				buildNumber = PlayerSettings.Android.bundleVersionCode.ToString();
			}

			origBuildNumber = buildInfo.BuildNumber;

			var assetPath = AssetDatabase.GetAssetPath(buildInfo);
			BuildToolLogger.Log($"Settings buildNumber to {buildNumber} on {assetPath}");
			buildInfo.BuildNumber = buildNumber;
			EditorUtility.SetDirty(buildInfo);
			AssetDatabase.SaveAssets();
		}

		IPTechBuildInfo CreateNewIPTechBuildInfo() {
			createdBuildInfo = true;
			BuildToolLogger.Log("Created IPTechBuildInfo asset in resources");
			var buildInfo = ScriptableObject.CreateInstance<IPTechBuildInfo>();
			if(!Directory.Exists(Path.Combine("Assets", "Resources"))) {
				Directory.CreateDirectory(Path.Combine("Assets", "Resources"));
			}
			AssetDatabase.CreateAsset(buildInfo, Path.Combine("Assets", "Resources", nameof(IPTechBuildInfo) + ".asset"));
			return buildInfo;
		}

		public override void PostProcessBuild(BuildReport report) {
			if(createdBuildInfo) {
				BuildToolLogger.Log("Deleting created IPTechBuildInfo asset");
				var buildInfo = IPTechBuildInfo.LoadFromResources();
				if(buildInfo != null) {
					AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(buildInfo));
				}
			}
			else {
				BuildToolLogger.Log($"Reverting IPTechBuildInfo asset build number to {origBuildNumber}");
				var buildInfo = IPTechBuildInfo.LoadFromResources();
				if(buildInfo != null) {
					buildInfo.BuildNumber = origBuildNumber;
					EditorUtility.SetDirty(buildInfo);
					AssetDatabase.SaveAssets();
				}
			}
		}
	}
}
