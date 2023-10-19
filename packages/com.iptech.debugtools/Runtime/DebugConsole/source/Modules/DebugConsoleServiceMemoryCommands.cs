#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System;
using System.Text;
using System.Linq;
using IPTech.DebugConsoleService.Api;

namespace IPTech.DebugConsoleService {
    public static class DebugConsoleServiceMemoryCommands {

        const string CATEGORY = "Memory";
        const string COMMANDUSAGE = "Command usage: mem [tex | texReport]";

        public static void RegisterDebugCommands(IDebugConsoleService debugConsoleService) {
            debugConsoleService.RegisterCommand("mem", mem, CATEGORY, "Memory command to report on different memory aspects.");
            debugConsoleService.RegisterAlias("mem.texture.report", "mem texReport", "Texture Report", CATEGORY, "Generates a full texture memory report for the currently loaded textures.");
            debugConsoleService.RegisterAlias("mem.texture", "mem tex", "Texture Mem", CATEGORY, "Reports the total texture memory usage.");
        }

        private static void mem(string[] args, Action<string> result) {
            if(args.Length>1) {
                if(args[1].Equals("tex",StringComparison.InvariantCultureIgnoreCase)) {
                    memTexture(result);
                    return;
                }

                if(args[1].Equals("texReport", StringComparison.InvariantCultureIgnoreCase)) {
                    memTextureReport(result);
                    return;
                }
            }
            result(COMMANDUSAGE);
        }

    	private static void memTexture(Action<string> result) {
            MemoryDataCollection mdc = TextureMemoryAnalyzer.CalculateMemoryForAllTextures();
            result(FormatMemoryDataForOutput(mdc));
        }

        private static void memTextureReport(Action<string> result) {
            StringBuilder sb = new StringBuilder();
            MemoryDataCollection mdc = TextureMemoryAnalyzer.CalculateMemoryForAllTextures();
            foreach(MemoryData md in mdc.OrderBy( o => o.SizeInBytes )) {
                sb.AppendLine(FormatMemoryDataForOutput(md));
            }
            sb.AppendLine(FormatMemoryDataForOutput(mdc));
            result(sb.ToString());
        }

        private static string FormatMemoryDataForOutput(MemoryData md) {
            return string.Format("{0:F} MB : {1} bytes : {2} : {3}", md.SizeInMegabytes, md.SizeInBytes, md.Status, md.Name);
        }

    }
}
#endif
