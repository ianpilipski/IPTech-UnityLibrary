using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IPTech.BuildTool
{
    public abstract class BuildConfig : ScriptableObject {
        public abstract void Build(IDictionary<string,string> commandLineArgs);
        public abstract bool CanBuildWithCurrentEditorBuildTarget();
    }
}
