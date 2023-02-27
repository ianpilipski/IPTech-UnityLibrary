using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace IPTech.BuildTool {
    public class ConfigModifierSetScriptingDefines : ConfigModifier {
        public List<string> ScriptingDefines;

        public override void ModifyProject() {
        }

        public override void RestoreProject() {
        }

        public override BuildPlayerOptions ModifyBuildPlayerOptions(BuildPlayerOptions options) {
            if(options.extraScriptingDefines != null && options.extraScriptingDefines.Length > 0) {
                options.extraScriptingDefines = options.extraScriptingDefines.Concat(ScriptingDefines).ToArray();
            } else {
                options.extraScriptingDefines = ScriptingDefines.ToArray();
            }
            return options;
        }
    }
}
