using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
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
        [InlineCreation] public List<BuildProcessor> BuildProcessors;
        
        Stack<BuildProcessor> undoModifiers = new Stack<BuildProcessor>();
        [NonSerialized] public List<BuildProcessor> cachedBuildProcessors;

        
        protected override void OnValidate() {
            // migration code for older configs
            #pragma warning disable 0618
            var mods = LoadConfigModifiers();
            #pragma warning restore 0618
            if(mods.Any()) {
                BuildToolLogger.Log($"migrating configmodifiers {mods.Select(m => m.name).Aggregate((a,b) => $"{a}, {b}")}");
                foreach(var mod in mods) {
                    var bp = mod.ConvertToBuildProcessor();
                    BuildProcessors.Add(bp);
                    AssetDatabase.AddObjectToAsset(bp, this);
                    AssetDatabase.RemoveObjectFromAsset(mod);
                }
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssetIfDirty(this);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
            }
            base.OnValidate();
        }

        protected override bool IsSubAssetValid(UnityEngine.Object subAsset) {
            #pragma warning disable 0618
            if(subAsset is ConfigModifier cm) {
            #pragma warning restore 0618
                return true;
            } else if(subAsset is BuildProcessor bp) {
                return BuildProcessors?.Contains(bp) == true;
            }
            return false;
        }

        public override void Build(IDictionary<string, string> args) {
            AssertEditorPlatformMatchesBuildTarget();
            CurrentlyBuildingConfig = this;

            var readonlyArgs = new ReadOnlyDictionary<string, string>(args);
            using(new CurrentBuildSettings.Scoped()) {
                try {
                    InstanceBuildProcessors();

                    BuildPlayerOptions options = GetBuildPlayerOptions(readonlyArgs);
                    options = ModifyEditorProperties(readonlyArgs, options);
                    BuildReport buildReport = BuildPipeline.BuildPlayer(options);
                    if(buildReport.summary.result != BuildResult.Succeeded) {
                        throw new System.Exception("Build Failed");
                    }

                    UndoModifications(readonlyArgs);
                } finally {
                    UndoModifications(readonlyArgs, false);
                    DestroyBuildProcessorInstances();
                    CurrentlyBuildingConfig = null;
                    AssetDatabase.SaveAssets();
                }
            }
        }

        void InstanceBuildProcessors() {
            var globalBuildProcessors = BuildToolsSettings.instance.BuildProcessors ?? new List<BuildProcessor>();
            var buildConfigBuildProcessors = BuildProcessors ?? new List<BuildProcessor>();
            
            var allBuildProcessors = globalBuildProcessors.Concat(buildConfigBuildProcessors);

            //Doing this allows the buildprocessors to survive after the build and the editor
            //does a domain reload.  If we referenced the saved scriptable objects they would null out
            cachedBuildProcessors = allBuildProcessors.Select(cm => {
                var inst = (BuildProcessor)ScriptableObject.Instantiate(cm);
                inst.hideFlags = (HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave) & ~HideFlags.NotEditable;
                return inst;
            }).ToList();
        }

        void DestroyBuildProcessorInstances() {
            foreach(var cm in cachedBuildProcessors) {
                DestroyImmediate(cm);
            }
            cachedBuildProcessors = null;
        }

        protected virtual BuildPlayerOptions ModifyEditorProperties(IReadOnlyDictionary<string,string> args, BuildPlayerOptions options) {
            SetBuildNumber(args);
            
            foreach(var cm in cachedBuildProcessors) {
                try {
                    options = ApplyModification(cm, options, args);
                } catch {
                    UndoModifications(args, false);
                    throw;
                }
            }
            return options;
        }

        BuildPlayerOptions ApplyModification(BuildProcessor modifier, BuildPlayerOptions options, IReadOnlyDictionary<string,string> args) {
            BuildToolLogger.Log($"[BuildProcessor] {modifier.name} modifying build properties.");
            modifier.ModifyProject(BuildTarget, args);
            undoModifiers.Push(modifier);
            return modifier.ModifyBuildPlayerOptions(options, args);
        }

        void UndoModifications(IReadOnlyDictionary<string,string> args, bool throwOnError = true) {
            bool hadError = false;
            while(undoModifiers.Count>0) {
                var cm = undoModifiers.Pop();
                try {
                    BuildToolLogger.Log($"[BuildProcessor] {cm.name} restoring build properties.");
                    cm.RestoreProject(BuildTarget, args);
                } catch(Exception e) {
                    hadError = true;
                    BuildToolLogger.LogException(e);
                }
            }

            if(hadError && throwOnError) {
                throw new Exception("Failed to undo some of the project modifications!!!!");
            }
        }

        protected void SetBuildNumber(IReadOnlyDictionary<string,string> args) {
            if(args.TryGetValue("-buildNumber", out string val)) {
                BuildToolLogger.Log("Setting buildNumber / bundleVersionCode from -buildNumber commandline argument to " + val);
                
                int i = int.Parse(val);
                if(i<=0) i=1;

                PlayerSettings.iOS.buildNumber = val;
                PlayerSettings.Android.bundleVersionCode = i;
            }
        }

        protected virtual BuildPlayerOptions GetBuildPlayerOptions(IReadOnlyDictionary<string, string> args) {
            return new BuildPlayerOptions {
                locationPathName = GetLocationPathName(args),
                options = BuildOptions,
                scenes = GetScenes(),
                target = BuildTarget
            };
        }
        
        protected virtual string GetLocationPathName(IReadOnlyDictionary<string, string> args) {
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
