
namespace IPTech.BuildTool {
    

    public abstract class ConfigModifier : BuildProcessor {
        public abstract BuildProcessor ConvertToBuildProcessor();
    }
}
