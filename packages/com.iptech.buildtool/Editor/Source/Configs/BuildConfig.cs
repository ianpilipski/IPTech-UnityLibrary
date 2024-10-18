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
                    Debug.LogError("found sub that was null - scriptable object is missing, was it removed?");
                }
            }
        }
        
        public abstract void Build(IDictionary<string,string> commandLineArgs);
        public abstract bool CanBuildWithCurrentEditorBuildTarget();

        protected virtual bool IsSubAssetValid(Object obj) { return true; }

        protected virtual void OnEnable() {
            CleanupSubAssets();
        }

        protected virtual void OnValidate() {
            CleanupSubAssets();
        }

        void CleanupSubAssets() {
            var subAssets = PopulateSubAssetsList();
            var removedBps = subAssets.Where(sa => !IsSubAssetValid(sa));
            if(removedBps.Any()) {
                EditorApplication.delayCall += () => {
                    foreach(var bp in removedBps) {
                        if(bp != null) {
                            AssetDatabase.RemoveObjectFromAsset(bp);
                            EditorUtility.SetDirty(bp);
                            AssetDatabase.SaveAssetIfDirty(bp);
                        } else {
                            Debug.LogWarning("bp is null");
                        }
                    }
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssetIfDirty(this);
                };
            }


        }

        IEnumerable<UnityEngine.Object> PopulateSubAssetsList() {
            var subs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
            if(subs != null) {
                return subs.Where(s => s != this && s != null);
            }
            return EmptySubAssets;
        }
    }
}
