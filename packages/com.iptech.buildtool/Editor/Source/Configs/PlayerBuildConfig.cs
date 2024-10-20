using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IPTech.BuildTool
{
    public class PlayerBuildConfig : BuildConfig {
        static IEnumerable<UnityEngine.Object> EmptySubAssets = new List<UnityEngine.Object>();
        public static PlayerBuildConfig CurrentlyBuildingConfig;

        public string OutputPath;
        public BuildTarget BuildTarget;
        public BuildOptions BuildOptions;
        [InlineCreation]
        public List<BuildProcessor> BuildProcessors;
        [InlineCreation]
        public List<ConfigModifier> ConfigModifiers;
        
        Stack<ConfigModifier> undoModifiers = new Stack<ConfigModifier>();
        List<ConfigModifier> cachedConfigModifiers;

        protected override void OnValidate() {
            Debug.LogWarning("on validate was called");
            // migration code for older configs
            if(ConfigModifiers == null || ConfigModifiers.Count == 0) {
                Debug.LogWarning("attempting migrating configmodifiers");
                var mods = LoadConfigModifiers();
                if(mods.Any()) {
                    Debug.LogWarning($"migrating configmodifiers {mods.Select(m => m.name).Aggregate((a,b) => $"{a}, {b}")}");
                    ConfigModifiers = new List<ConfigModifier>(mods);
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssetIfDirty(this);
                }
            }
            base.OnValidate();
        }

        protected override bool IsSubAssetValid(UnityEngine.Object subAsset) {
            if(subAsset is ConfigModifier cm) {
                return ConfigModifiers?.Contains(cm) == true;
            } else if(subAsset is BuildProcessor bp) {
                return BuildProcessors?.Contains(bp) == true;
            }
            return false;
        }

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
            if(ConfigModifiers == null) return;

            //Doing this allows the configModifiers list to survive after the build and the editor
            //does a domain reload.  If we referenced the saved scriptable objects they would null out
            cachedConfigModifiers = ConfigModifiers.Select(cm => {
                var inst = (ConfigModifier)ScriptableObject.Instantiate(cm);
                inst.hideFlags = (HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave) & ~HideFlags.NotEditable;
                return inst;
            }).ToList();
        }

        void DestroyConfigModifierInstances() {
            foreach(var cm in cachedConfigModifiers) {
                DestroyImmediate(cm);
            }
            cachedConfigModifiers = null;
        }

        protected virtual BuildPlayerOptions ModifyEditorProperties(IDictionary<string,string> args, BuildPlayerOptions options) {
            SetBuildNumber(args);
            
            foreach(var cm in cachedConfigModifiers) {
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
                Console.Out.WriteLine("Setting buildNumber / bundleVersionCode from -buildNumber commandline argument to " + val);
                
                int i = int.Parse(val);
                if(i<=0) i=1;

                PlayerSettings.iOS.buildNumber = val;
                PlayerSettings.Android.bundleVersionCode = i;
            }
        }

        protected virtual BuildPlayerOptions GetBuildPlayerOptions(IDictionary<string, string> args) {
            return new BuildPlayerOptions {
                locationPathName = GetLocationPathName(args),
                options = BuildOptions,
                scenes = GetScenes(),
                target = BuildTarget
            };
        }
        
        protected virtual string GetLocationPathName(IDictionary<string, string> args) {
            string outPath = OutputPath;
            if(string.IsNullOrWhiteSpace(outPath)) {
                outPath = Path.Combine("build", name);
            }

            if(args.TryGetValue("-outputDir", out string value)) {
                string buildName = Path.GetFileName(outPath);
                return Path.Combine(value, buildName);        
            }

            return outPath;
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
