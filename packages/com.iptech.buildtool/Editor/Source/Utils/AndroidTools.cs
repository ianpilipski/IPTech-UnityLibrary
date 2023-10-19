using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool
{
    public static class AndroidTools {
		public static void AddGradleWrapperToPath(string outputPath) {
			Console.Out.WriteLine("Adding gradle wrapper to exported project at " + outputPath);
			string gradleLauncherPath;

			if(!AlreadyHasGradleWrapper()) {
				FindUnityGradleLauncher();
				AddGradleSettingsFile();
				ExecuteGradleWrapper();
			} else {
				Console.Out.WriteLine("Detected a gradlew file already present in exported project at " + outputPath);
			}

			void FindUnityGradleLauncher() {
				string unityGradlePath = Path.Combine(EditorApplication.applicationPath, "..", "PlaybackEngines", "AndroidPlayer", "Tools", "gradle", "lib");
				if(Application.platform == RuntimePlatform.LinuxEditor) {
					unityGradlePath = Path.Combine(EditorApplication.applicationPath, "..", "Data", "PlaybackEngines", "AndroidPlayer", "Tools", "gradle", "lib");
				}

				string[] launcherSearch = Directory.GetFiles(unityGradlePath, "gradle-launcher-*.jar");
				if(launcherSearch != null && launcherSearch.Length > 0) {
					gradleLauncherPath = launcherSearch[0];
					return;
				}
				throw new FileNotFoundException("Could not find the unity gradle files");
			}

			void AddGradleSettingsFile() {
				string gradleSettingsFile = Path.Combine(outputPath, "settings.gradle");
				if(!File.Exists(gradleSettingsFile)) {
					File.WriteAllText(gradleSettingsFile,
						"rootProject.name='" + PlayerSettings.productName + "'"
#if UNITY_2019_1_OR_NEWER
							+ "\ninclude \"unityLibrary\""
						+ "\ninclude \"launcher\""
#endif
						);
				}
			}

			bool AlreadyHasGradleWrapper() {
				return File.Exists(Path.Combine(outputPath, "gradlew"));
			}

			void ExecuteGradleWrapper() {
				int exitCode = ShellCommand.ExecBash(
					string.Format("java -classpath \"{0}\" org.gradle.launcher.GradleMain wrapper", gradleLauncherPath),
					outputPath
				);

				if(exitCode != 0) throw new Exception("Failed to generate gradle wrapper for exported project.");
			}
		}
	}
}
