using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace IPTech.BuildTool
{
    public abstract class BuildConfig : ScriptableObject {
        public IEnumerable<ConfigModifier> ConfigModifiers {
            get {
                var subs = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(this));
                return subs.Where(s => s.GetType().IsSubclassOf(typeof(ConfigModifier))).Select(s => (ConfigModifier)s);
            }
        }
        
        public abstract void Build(IDictionary<string,string> commandLineArgs);
        public abstract bool CanBuildWithCurrentEditorBuildTarget();
    }
}
