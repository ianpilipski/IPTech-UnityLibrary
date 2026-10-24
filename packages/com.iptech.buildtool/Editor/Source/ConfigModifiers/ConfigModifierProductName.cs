using System;
using IPTech.BuildTool.Processors;

namespace IPTech.BuildTool
{
    [Obsolete("ConfigModifierProductName is obsolete. Use SetProductName instead.")]
    public class ConfigModifierProductName : ConfigModifier {
        public string ProductName;

        public override BuildProcessor ConvertToBuildProcessor() {
            var bp = CreateInstance<SetProductName>();
            bp.name = nameof(SetProductName);
            bp.ProductName = ProductName;
            return bp;
        }
    }
}
