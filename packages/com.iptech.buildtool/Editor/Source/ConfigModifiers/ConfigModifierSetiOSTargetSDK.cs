using IPTech.BuildTool.Processors;
using UnityEditor;

namespace IPTech.BuildTool
{
    public class ConfigModifierSetiOSTargetSDK : ConfigModifier {
        public iOSSdkVersion TargetSDK;


        public override BuildProcessor ConvertToBuildProcessor() {
            var bp = CreateInstance<SetiOSTargetSDK>();
            bp.name = nameof(SetiOSTargetSDK);
            bp.TargetSDK = TargetSDK;
            return bp;
        }

    }
}