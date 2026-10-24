using System;
using IPTech.BuildTool.Processors;

namespace IPTech.BuildTool {
    [Obsolete("ConfigModifierApplicationIdentifier is obsolete. Use SetApplicationIdentifier instead.")]
    public class ConfigModifierApplicationIdentifier : ConfigModifier {
        public string ApplicationIdentifier;

        public override BuildProcessor ConvertToBuildProcessor() {
            var bp = CreateInstance<SetApplicationIdentifier>();
            bp.name = nameof(SetApplicationIdentifier);
            bp.ApplicationIdentifier = ApplicationIdentifier;
            return bp;
        }
    }
}
