
using System;

namespace IPTech.BuildTool {
    
    [Obsolete("ConfigModifier is obsolete. Use BuildProcessor instead.")]
    public abstract class ConfigModifier : BuildProcessor {
        public abstract BuildProcessor ConvertToBuildProcessor();
    }
}
