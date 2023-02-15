using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IPTech.BuildTool {
    public class PlayerBuildConfig : BuildConfig {
        public string OutputPath;
        public BuildTarget BuildTarget;
        public bool DevelopmentBuild;
        public bool AddGradleWrapper;

        public override void Build(IDictionary<string, string> args) {
            AssertEditorPlatformMatchesBuildTarget();
            using(new ConfigureGradleWrapperScope(AddGradleWrapper)) {
                SetBuildNumber(args);
                BuildPlayerOptions options = GetBuildPlayerOptions(args);
                BuildReport buildReport = BuildPipeline.BuildPlayer(options);
                if(buildReport.summary.result != BuildResult.Succeeded) {
                    throw new System.Exception("Build Failed");
                }
            }
        }

        
        protected void SetBuildNumber(IDictionary<string,string> args) {
            if(args.TryGetValue("-buildNumber", out string val)) {
                PlayerSettings.iOS.buildNumber = val;
                PlayerSettings.Android.bundleVersionCode = int.Parse(val);
            }
        }

        protected virtual BuildPlayerOptions GetBuildPlayerOptions(IDictionary<string, string> args) {
            return new BuildPlayerOptions {
                locationPathName = GetOutputPath(),
                options = GetBuildOptions(),
                scenes = GetScenes(),
                target = BuildTarget
            };
        }
        
        protected string GetOutputPath() {
            string retVal = OutputPath;
            if(string.IsNullOrEmpty(retVal)) {
                retVal = $"build/{name}";
            }

            return retVal;
        }

        protected BuildOptions GetBuildOptions() {
            BuildOptions options = BuildOptions.None;
            options |= DevelopmentBuild ? BuildOptions.Development : BuildOptions.None;
            options |= EditorUserBuildSettings.symlinkSources ? BuildOptions.SymlinkSources : BuildOptions.None;
            options |= EditorUserBuildSettings.connectProfiler ? BuildOptions.ConnectWithProfiler : BuildOptions.None;
            options |= EditorUserBuildSettings.buildWithDeepProfilingSupport ? BuildOptions.EnableDeepProfilingSupport : BuildOptions.None;
            options |= EditorUserBuildSettings.allowDebugging ? BuildOptions.AllowDebugging : BuildOptions.None;
            options |= EditorUserBuildSettings.buildScriptsOnly ? BuildOptions.BuildScriptsOnly : BuildOptions.None;
            return options;
        }

        protected virtual string[] GetScenes() {
            return EditorBuildSettings.scenes.Select(s => s.path).ToArray();
        }
    

        protected void AssertEditorPlatformMatchesBuildTarget() {
            if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget) {
                throw new System.Exception("The editor buildTarget does not match the buildConfig buildTarget, launch the editor with -buildTarget");
            }
        }

        protected class ConfigureGradleWrapperScope : IDisposable {
            readonly bool origSetting;

            public ConfigureGradleWrapperScope(bool enabled) {
                origSetting = AndroidBuildProcessor.Enabled;
                AndroidBuildProcessor.Enabled = enabled;
            }

            public void Dispose() {
                AndroidBuildProcessor.Enabled = origSetting;
            }
        }
    }
}
