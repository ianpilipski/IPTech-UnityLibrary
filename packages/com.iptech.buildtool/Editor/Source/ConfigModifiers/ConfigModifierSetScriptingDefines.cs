using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace IPTech.BuildTool
{
    public class ConfigModifierSetScriptingDefines : ConfigModifier {
        public List<string> ScriptingDefines;

        string[] previousScriptingDefines;

        public override void ModifyProject(BuildTarget buildTarget) {
            previousScriptingDefines = EditorUserBuildSettings.activeScriptCompilationDefines;
            PlayerSettings.SetScriptingDefineSymbols(GetNamedBuildTarget(buildTarget), ScriptingDefines.ToArray());
        }

        public override void RestoreProject(BuildTarget buildTarget) {
            PlayerSettings.SetScriptingDefineSymbols(GetNamedBuildTarget(buildTarget), previousScriptingDefines);
        }

        NamedBuildTarget GetNamedBuildTarget(BuildTarget buildTarget) {
            var btGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(btGroup);
            return namedBuildTarget;
        }
    }
}
