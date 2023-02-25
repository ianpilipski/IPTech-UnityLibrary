using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IPTech.BuildTool
{
	public class BuildPreprocessor : IPreprocessBuildWithReport
	{
		public int callbackOrder => 0;

		public void OnPreprocessBuild(BuildReport report)
		{
			IPTechBuildInfo buildInfo = IPTechBuildInfo.LoadFromResources() ?? CreateNewIPTechBuildInfo();
			string buildNumber = PlayerSettings.iOS.buildNumber;
			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
			{
				buildNumber = PlayerSettings.Android.bundleVersionCode.ToString();
			}

			buildInfo.BuildNumber = buildNumber;
			EditorUtility.SetDirty(buildInfo);
			AssetDatabase.SaveAssets();
		}

		IPTechBuildInfo CreateNewIPTechBuildInfo() {
			var buildInfo = ScriptableObject.CreateInstance<IPTechBuildInfo>();
			if(!Directory.Exists(Path.Combine("Assets", "Resources"))) {
				Directory.CreateDirectory(Path.Combine("Assets", "Resources"));
			}
			AssetDatabase.CreateAsset(buildInfo, Path.Combine("Assets", "Resources", nameof(IPTechBuildInfo) + ".asset"));
			return buildInfo;
		}
	}
}