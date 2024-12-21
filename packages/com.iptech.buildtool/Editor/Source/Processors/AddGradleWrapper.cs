using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IPTech.BuildTool.Processors
{
    public class AddGradleWrapper : BuildProcessor {
        const string MSG_CREATE_GRADLE_SETTINGS = "Adding a placeholder settings.gradle file so this project will build without detecting the parent gradle project during warmup";
		const string GRADLE_SETTINGS = "// placeholder settings.gradle file so that this gradle project does not detect the parent gradle project during warmup\n" +
			"include 'StagingArea'\n" +
			"include 'gradleOut'\n" +
			"include 'StagingArea:gradleWarmupArea'";

		BuildReport buildReport;
		
		public override void PostGenerateGradleAndroidProject(string path) {
			try {
				string outputPath = CalculateWrapperOutputPath();
				AndroidTools.AddGradleWrapperToPath(outputPath);
			} catch(Exception e) {
				BuildToolLogger.LogException(e);
				if(!UnityEditorInternal.InternalEditorUtility.inBatchMode) {
					EditorUtility.DisplayDialog("Error Post Processing Project", e.Message, "Ok");
				} else {
					EditorApplication.Exit(1);
				}
				throw new BuildFailedException(e.Message);
			}

			string CalculateWrapperOutputPath() {
#if UNITY_2019_1_OR_NEWER
				return Path.GetFullPath(Path.Combine(path, ".."));
#else
					return path;
#endif
			}
		}

		public override void PreprocessBuild(BuildReport report) {
			try {
				buildReport = report;
				if(IsAndroidBuild()) {
					CreateTempGradleSettingsFile();
				}
			} catch(Exception e) {
				if(!UnityEditorInternal.InternalEditorUtility.inBatchMode) {
					//EditorUtility.DisplayDialog("Error Preprocessing Project", e.Message, "Ok");
				} else {
					//EditorApplication.Exit(1);
				}
				throw new BuildFailedException(e);
			}

			bool IsAndroidBuild() {
				return buildReport.summary.platform == BuildTarget.Android;
			}

		}

		void CreateTempGradleSettingsFile() {
			if(!HasGradleSettingsFile()) {
				Console.Out.WriteLine(MSG_CREATE_GRADLE_SETTINGS);
				Directory.CreateDirectory(GetGradleSettingsDir());
				File.WriteAllText(GetGradleSettingsPath(), GRADLE_SETTINGS);
			}

			bool HasGradleSettingsFile() {
				return File.Exists(GetGradleSettingsPath());
			}

			string GetGradleSettingsDir() {
				return Path.Combine(Application.dataPath, "..", "Temp");
			}

			string GetGradleSettingsPath() {
				return Path.Combine(GetGradleSettingsDir(), "settings.gradle");
			}
		}

		class TmpFiles : IDisposable {
			string[] filePaths;
			public TmpFiles(params string[] filePaths) {
				this.filePaths = filePaths;
			}

			public void Dispose() {
				if(filePaths != null) {
					foreach(var filePath in filePaths) {
						try {
							if(File.Exists(filePath)) {
								File.Delete(filePath);
							}
						} catch { }
					}
				}
			}
		}
	}
}
