using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IPTech.BuildTool {
    public class PlayerBuildConfig : BuildConfig {
        public static PlayerBuildConfig CurrentlyBuildingConfig;

        public string OutputPath;
        public BuildTarget BuildTarget;
        public BuildOptions BuildOptions;
        public List<BuildProcessor> BuildProcessors;

        Stack<ConfigModifier> undoModifiers = new Stack<ConfigModifier>();
        List<ConfigModifier> configModifiers;


        public override void Build(IDictionary<string, string> args) {
            AssertEditorPlatformMatchesBuildTarget();
            CurrentlyBuildingConfig = this;

            using(new CurrentBuildSettings.Scoped()) {
                try {
                    InstanceConfigModifiers();

                    BuildPlayerOptions options = GetBuildPlayerOptions(args);
                    options = ModifyEditorProperties(args, options);
                    BuildReport buildReport = BuildPipeline.BuildPlayer(options);
                    if(buildReport.summary.result != BuildResult.Succeeded) {
                        throw new System.Exception("Build Failed");
                    }

                    UndoModifications();
                } finally {
                    UndoModifications(false);
                    DestroyConfigModifierInstances();
                    CurrentlyBuildingConfig = null;
                    AssetDatabase.SaveAssets();
                }
            }
        }

        void InstanceConfigModifiers() {
            //Doing this allows the configModifiers list to survive after the build and the editor
            //does a domain reload.  If we referenced the saved scriptable objects they would null out
            configModifiers = new List<ConfigModifier>();
            foreach(var cm in LoadConfigModifiers()) {
                var inst = (ConfigModifier)ScriptableObject.Instantiate(cm);
                inst.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
                configModifiers.Add(inst);
            }
        }

        void DestroyConfigModifierInstances() {
            foreach(var cm in configModifiers) {
                DestroyImmediate(cm);
            }
            configModifiers = null;
        }

        protected virtual BuildPlayerOptions ModifyEditorProperties(IDictionary<string,string> args, BuildPlayerOptions options) {
            SetBuildNumber(args);
            
            foreach(var cm in configModifiers) {
                try {
                    options = ApplyModification(cm, options);
                } catch {
                    UndoModifications(false);
                    throw;
                }
            }
            return options;
        }

        BuildPlayerOptions ApplyModification(ConfigModifier modifier, BuildPlayerOptions options) {
            Debug.Log($"[IPTech.BuildTool] {modifier.name} modifying build properties.");
            modifier.ModifyProject(BuildTarget);
            undoModifiers.Push(modifier);
            return modifier.ModifyBuildPlayerOptions(options);
        }

        void UndoModifications(bool throwOnError = true) {
            bool hadError = false;
            while(undoModifiers.Count>0) {
                var cm = undoModifiers.Pop();
                try {
                    Debug.Log($"[IPTech.BuildTool] {cm.name} restoring build properties.");
                    cm.RestoreProject(BuildTarget);
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

        protected virtual BuildPlayerOptions GetBuildPlayerOptions(IDictionary<string, string> args) {
            return new BuildPlayerOptions {
                locationPathName = GetOutputPath(args),
                options = BuildOptions,
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
