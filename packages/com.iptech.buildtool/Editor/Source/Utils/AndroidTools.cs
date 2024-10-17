using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool
{
    public static class AndroidTools {
		const string BEGIN_REPLACEBUNDLEDEBUG = "// BEGIN IPTECH BUILDTOOL (replace bundleDebug with bundleRelease)";
		const string END_REPLACEBUNDLEDEBUG = "// END IPTECH BUILDTOOL (replace bundleDebug with bundleRelease)";

		const string BUNDLEDEBUGMAKESBUNDLERELEASECONTENT =
BEGIN_REPLACEBUNDLEDEBUG + @"
def tstamp = new Date().format('yyyy-MM-dd_HH-mm-ss')
def buildLogDir = ""${rootDir
	}/build/logs""
mkdir(""${buildLogDir}"")
def buildLog = new File(""${buildLogDir}/${tstamp}_buildLog.log"")

tasks.create('renameBundleRelease', Copy) {
    from(""${buildDir}/outputs/bundle/release/"")

	include 'launcher-release.aab'
    destinationDir file(""${buildDir}/outputs/bundle/debug/"")

	rename 'launcher-release.aab', 'launcher-debug.aab'
    dependsOn('bundleRelease')
}

tasks.all {
	if(name == 'bundleDebug') {
		dependsOn 'renameBundleRelease'

	}
	doFirst {
		buildLog.append(""${name}\n"")

	}
}
project.gradle.startParameter.excludedTaskNames.add('signDebugBundle')
" + END_REPLACEBUNDLEDEBUG;

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

		public static void MoveKeyStoreTo(string outputPath) {
			if(PlayerSettings.Android.useCustomKeystore) {
				var src = PlayerSettings.Android.keystoreName;
				if(!string.IsNullOrEmpty(src)) {
					if(File.Exists(src)) {
						File.Copy(src, Path.Combine(outputPath, Path.GetFileName(src)));
						var gradleFile = Path.Combine(outputPath, "build.gradle");
						if(File.Exists(gradleFile)) {
							ReplaceStoreFileSetting(gradleFile, Path.GetFileName(src));
                        }
                    } else {
						throw new FileNotFoundException($"could not find the keystore file: {src}");
                    }
                }
			}

			void ReplaceStoreFileSetting(string gradleFile, string storeFilePath) {
				var resultFile = new StringBuilder();
				int inSigningConfigs = 0;
				var lines = File.ReadAllLines(gradleFile);
				var found = false;
				foreach(var line in lines) {
					if(line.Trim().StartsWith("signingConfigs")) {
						inSigningConfigs++;
					} else if(line.Trim().EndsWith("{")) {
						inSigningConfigs++;
					} else if(line.Trim().StartsWith("}")) {
						inSigningConfigs--;
                    }

					if(inSigningConfigs>0 && line.Trim().StartsWith("storeFile ") && line.Contains(storeFilePath)) {
						found = true;
						var i = line.IndexOf("storeFile ");
						var prefix = line.Substring(0, i);
						resultFile.AppendLine($"{prefix}storeFile file('{storeFilePath}')");
                    } else {
						resultFile.AppendLine(line);
                    }
                }

				if(found) {
					File.WriteAllText(gradleFile, resultFile.ToString());
                } else {
					Debug.LogWarning("could not find singingConfigs pointing to the keystore that you wanted to move");
                }
            }
		}

		public static void MakeBundleDebugBuildBundleReleaseInsead(string unityLibraryOutputPath) {
			var launcherGradleBuildFile = Path.GetFullPath(Path.Combine(unityLibraryOutputPath, "..", "launcher", "build.gradle"));
			AddMakeBundleDebugBuildBundleReleaseToGradleFile(launcherGradleBuildFile);
        }

		public static void RemoveMakeBundleDebugBuildBundleReleaseFromGradleFile(string gradleBuildFilePath) {
			RemoveContentFromFile(gradleBuildFilePath, BEGIN_REPLACEBUNDLEDEBUG, END_REPLACEBUNDLEDEBUG);
		}

		public static void AddMakeBundleDebugBuildBundleReleaseToGradleFile(string gradleBuildFilePath) {
			AddContentToFile(gradleBuildFilePath, BEGIN_REPLACEBUNDLEDEBUG, END_REPLACEBUNDLEDEBUG, BUNDLEDEBUGMAKESBUNDLERELEASECONTENT);
		}

		public static void AddContentToFile(string filePath, string beginMarker, string endMarker, string content) {
			ModifyFileContent(filePath, beginMarker, endMarker, () => content);
        }

		public static void RemoveContentFromFile(string filePath, string beginMarker, string endMarker) {
			ModifyFileContent(filePath, beginMarker, endMarker, null);
		}

		public static void ModifyFileContent(string filePath, string beginMarker, string endMarker, Func<string> content) {
			var inSection = false;
			var found = false;
			var sb = new StringBuilder();
			var gradleBuildFileContents = File.ReadAllLines(filePath);
			foreach(var line in gradleBuildFileContents) {
				if(line.Trim().Equals(beginMarker, StringComparison.OrdinalIgnoreCase)) {
					inSection = true;
					found = true;
					AddContent();
				} else if(inSection && line.Trim().Equals(endMarker, StringComparison.OrdinalIgnoreCase)) {
					inSection = false;
				} else {
					sb.AppendLine(line);
				}
			}

			if(inSection) {
				throw new Exception($"modifying file content found the begin marker but did not find the end marker: '{endMarker}'");
            }

			if(found || AddContent()) {
				File.WriteAllText(filePath, sb.ToString());
			}
			return;

			bool AddContent() {
				if(content != null) {
					var newContent = content();
					if(!string.IsNullOrEmpty(newContent)) {
						sb.AppendLine(content());
						return true;
					}
				}
				return false;
			}
		}
	}
}
