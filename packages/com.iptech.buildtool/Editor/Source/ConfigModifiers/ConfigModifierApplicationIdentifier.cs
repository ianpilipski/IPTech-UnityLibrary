using IPTech.BuildTool.Processors;

namespace IPTech.BuildTool {
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
