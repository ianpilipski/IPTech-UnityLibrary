#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IPTech.DebugConsoleService.Api;
using UnityEngine;

namespace IPTech.DebugConsoleService {
    public static class DebugConsoleServiceUtilCommands {
        const string COMMANDUSAGE = "Command usage: utils screenshot";
        private static List<object> dontGarbageCollect = new();

        public static void RegisterDebugCommands(IDebugConsoleService debugConsoleService) {
#if UNITY_EDITOR
            DebugCommandCallback callback = (args, result) => utils(debugConsoleService, args, result);
            dontGarbageCollect.Add(callback);
            debugConsoleService.RegisterCommand("utils", callback, "Utils", "Take a screenshot of the app");
            debugConsoleService.RegisterAlias("screenshot", "utils screenshot", "screenshot", "Utils", "Take a screenshot of the app");
#endif
        }

        private static void utils(IDebugConsoleService dc, string[] args, Action<string> result) {
            if(args.Length > 1) {
                if(args[1].Equals("screenshot", StringComparison.InvariantCultureIgnoreCase)) {
                    ScreenShot(dc, result);
                    return;
                }
            }
            result(COMMANDUSAGE);
        }

        private static int counter;
        private static long startticks = DateTime.Now.Ticks;
        private static async void ScreenShot(IDebugConsoleService dc, Action<string> result) {
            try {
                dc.ShowView(false);
                await Task.Yield();
                var dirpath = Path.Combine(Application.dataPath, "..", "screenshots", $"{startticks}");
                Directory.CreateDirectory(dirpath);
                ScreenCapture.CaptureScreenshot(Path.Combine(dirpath, $"screenshot{counter++}.png"));
                await Task.Yield();
                result("screen shot captured");
            } catch(Exception e) {
                UnityEngine.Debug.LogException(e);
                result("failed to capture screenshot");
            } finally {
                dc.ShowView(true);
            }
        }
    }
}
#endif
