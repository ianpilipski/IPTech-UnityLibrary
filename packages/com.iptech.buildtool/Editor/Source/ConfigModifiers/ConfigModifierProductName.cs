using IPTech.BuildTool.Processors;

namespace IPTech.BuildTool
{
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
