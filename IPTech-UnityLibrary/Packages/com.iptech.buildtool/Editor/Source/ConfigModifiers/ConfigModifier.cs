using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public abstract class ConfigModifier : ScriptableObject {
        public abstract void ModifyProject(BuildTarget buildTarget);
        public abstract void RestoreProject(BuildTarget buildTarget);
        public virtual BuildPlayerOptions ModifyBuildPlayerOptions(BuildPlayerOptions options) { return options; }
    }
}
