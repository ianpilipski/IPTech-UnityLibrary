using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace IPTech.BuildTool
{
    public abstract class BuildConfig : ScriptableObject {
        
        public IEnumerable<ConfigModifier> LoadConfigModifiers() {
            var subs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
            foreach(var sub in subs) {
                if(sub != null) {
                    if(sub.GetType().IsSubclassOf(typeof(ConfigModifier))) {
                        yield return (ConfigModifier)sub;
                    }
                } else {
                    BuildToolLogger.LogError("found sub that was null - scriptable object is missing, was it removed?");
                }
            }
        }
        
        public abstract void Build(IDictionary<string,string> commandLineArgs);
        public abstract bool CanBuildWithCurrentEditorBuildTarget();

        protected virtual bool IsSubAssetValid(Object obj) { return true; }

        protected virtual void OnValidate() {
            CleanupSubAssets();
        }

        void CleanupSubAssets() {
            var subs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
            var subAssets = subs.Where(s => s != this && s != null);
            var removedBps = subAssets.Where(sa => !IsSubAssetValid(sa));
            if(removedBps.Any()) {
                BuildToolLogger.LogWarning($"removing sub assets {removedBps.Select(n => n.name).Aggregate((a, b) => $"{a}, {b}")}");
                //EditorApplication.delayCall += () => {
                    foreach(var bp in removedBps) {
                        if(bp != null) {
                            AssetDatabase.RemoveObjectFromAsset(bp);
                            EditorUtility.SetDirty(bp);
                            AssetDatabase.SaveAssetIfDirty(bp);
                        } else {
                            BuildToolLogger.LogWarning("bp is null");
                        }
                    }
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssetIfDirty(this);
                //};
            }
        }
    }
}
