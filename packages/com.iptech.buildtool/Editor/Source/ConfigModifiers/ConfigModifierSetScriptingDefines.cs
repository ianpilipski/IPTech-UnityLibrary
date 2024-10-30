using System.Collections.Generic;
using IPTech.BuildTool.Processors;

namespace IPTech.BuildTool {
    public class ConfigModifierSetScriptingDefines : ConfigModifier {
        public List<string> ScriptingDefines;

        public override BuildProcessor ConvertToBuildProcessor() {
            var bp = CreateInstance<SetScriptingDefines>();
            bp.name = nameof(SetScriptingDefines);
            bp.ScriptingDefines = ScriptingDefines;
            return bp;
        }
    }
}
