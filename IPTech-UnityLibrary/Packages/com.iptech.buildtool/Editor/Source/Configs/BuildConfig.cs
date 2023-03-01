using System.Collections;
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
    }
}
