using IPTech.BuildTool.Processors;

namespace IPTech.BuildTool
{
    public class ConfigModifierOverrideMaxTextureSize : ConfigModifier {
        public EMaxTextureSize OverrideMaxTextureSize;

        public enum EMaxTextureSize {
            NoOverride = 0,
            Max64 = 64,
            Max128 = 128,
            Max256 = 256,
            Max512 = 512,
            Max1024 = 1024,
            Max2048 = 2048
        }

        public override BuildProcessor ConvertToBuildProcessor() {
            var bp = CreateInstance<SetOverrideMaxTextureSize>();
            bp.name = nameof(SetOverrideMaxTextureSize);
            bp.OverrideMaxTextureSize = (SetOverrideMaxTextureSize.EMaxTextureSize)(int)OverrideMaxTextureSize;
            return bp;
        }
    }
}