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
        public bool UsesNonExemptEncryption;
        public string BundleId;
        public EMaxTextureSize OverrideMaxTextureSize;

        public enum EMaxTextureSize {
            NoOverride = 0,
            Max64 = 64,
            Max128 = 128,
            Max256 = 256,
            Max512 = 512,
            Max1024 = 1024,
            Max2048 = 2048
        }

        Stack<ConfigModifier> undoModifiers = new Stack<ConfigModifier>();

        protected virtual void OnEnable() {
            if(string.IsNullOrEmpty(BundleId)) {
                BundleId = PlayerSettings.GetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup);
            }
        }
        public override void Build(IDictionary<string, string> args) {
            AssertEditorPlatformMatchesBuildTarget();

            using(new CurrentBuildSettings.Scoped()) {
                try {
                    CurrentBuildSettings.Inst.AddGradlewWrapper = AddGradleWrapper;
                    CurrentBuildSettings.Inst.UsesNonExemptEncryption = UsesNonExemptEncryption;

                    ModifyEditorProperties(args);
                    BuildPlayerOptions options = GetBuildPlayerOptions(args);
                    BuildReport buildReport = BuildPipeline.BuildPlayer(options);
                    if(buildReport.summary.result != BuildResult.Succeeded) {
                        throw new System.Exception("Build Failed");
                    }

                    UndoModifications();
                } finally {
                    UndoModifications(false);
                }
            }
        }

        protected virtual void ModifyEditorProperties(IDictionary<string,string> args) {
            SetBuildNumber(args);
            SetBundleId(args);
            EditorUserBuildSettings.overrideMaxTextureSize = (int)OverrideMaxTextureSize;

            foreach(var cm in ConfigModifiers) {
                try {
                    cm.ModifyProject();
                    undoModifiers.Push(cm);
                } catch {
                    UndoModifications(false);
                    throw;
                }
            }
        }

        void UndoModifications(bool throwOnError = true) {
            bool hadError = false;
            while(undoModifiers.Count>0) {
                var cm = undoModifiers.Pop();
                try {
                    cm.RestoreProject();
                } catch(Exception e) {
                    hadError = true;
                    Debug.LogException(e);
                }
            }

            if(hadError && throwOnError) {
                throw new Exception("Failed to undo some of the project modifications!!!!");
            }
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
                locationPathName = GetOutputPath(args),
                options = GetBuildOptions(),
                scenes = GetScenes(),
                target = BuildTarget
            };
        }
        
        protected string GetOutputPath(IDictionary<string,string> args) {
            string retVal = OutputPath;
            
            if(args.TryGetValue("-outputDir", out string value)) {
                retVal = value;
            }

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
