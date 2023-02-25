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
        public bool ExportGradleProject;
        public bool UsesNonExemptEncryption;
        public string BundleId;

        protected virtual void OnEnable() {
            if(string.IsNullOrEmpty(BundleId)) {
                BundleId = PlayerSettings.GetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup);
            }
        }
        public override void Build(IDictionary<string, string> args) {
            AssertEditorPlatformMatchesBuildTarget();
            
            using(new CurrentBuildSettings.Scoped()) {
                CurrentBuildSettings.Inst.AddGradlewWrapper = AddGradleWrapper;
                CurrentBuildSettings.Inst.UsesNonExemptEncryption = UsesNonExemptEncryption;

                ModifyEditorProperties(args);
                BuildPlayerOptions options = GetBuildPlayerOptions(args);
                BuildReport buildReport = BuildPipeline.BuildPlayer(options);
                if(buildReport.summary.result != BuildResult.Succeeded) {
                    throw new System.Exception("Build Failed");
                }
            }
        }

        protected virtual void ModifyEditorProperties(IDictionary<string,string> args) {
            SetBuildNumber(args);
            SetBundleId(args);
            EditorUserBuildSettings.exportAsGoogleAndroidProject = ExportGradleProject;
        }

        protected void SetBuildNumber(IDictionary<string,string> args) {
            if(args.TryGetValue("-buildNumber", out string val)) {
                PlayerSettings.iOS.buildNumber = val;
                PlayerSettings.Android.bundleVersionCode = int.Parse(val);
            }
        }

        protected void SetBundleId(IDictionary<string,string> args) {
            PlayerSettings.SetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup, BundleId);
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
            if(!CanBuildWithCurrentEditorBuildTarget()) {
                throw new System.Exception($"The editor buildTarget does not match the buildConfig buildTarget.\n" +
                    $"Editor: {EditorUserBuildSettings.activeBuildTarget}\n" +
                    $"BuildConfig:{BuildTarget}\n"+
                    "Switch or launch the editor with -buildTarget");
            }
        }

        public override bool CanBuildWithCurrentEditorBuildTarget() {
            return EditorUserBuildSettings.activeBuildTarget == BuildTarget;
        }

    }
}
