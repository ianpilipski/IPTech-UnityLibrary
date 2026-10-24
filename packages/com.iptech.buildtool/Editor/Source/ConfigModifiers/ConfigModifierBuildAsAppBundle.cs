using System;
using IPTech.BuildTool.Processors;
using UnityEditor;

namespace IPTech.BuildTool {
    [Obsolete("ConfigModifierBuildAsAppBundle is obsolete. Use SetBuildAsAppBundle instead.")]
    public class ConfigModifierBuildAsAppBundle : ConfigModifier {
        public bool BuildAsAppBundle;

        public override BuildProcessor ConvertToBuildProcessor() {
            var bp = CreateInstance<SetBuildAsAppBundle>();
            bp.name = nameof(SetBuildAsAppBundle);
            bp.BuildAsAppBundle = BuildAsAppBundle;
            return bp;
        }
    }
}
