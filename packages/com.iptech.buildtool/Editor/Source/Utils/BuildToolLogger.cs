using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.BuildTool {
    public static class BuildToolLogger {
        const string Prefix = "[IPTech.BuildTool]";

        public static void Log(string message) {
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, $"{Prefix} {message}");
        }

        public static void LogWarning(string message) {
            Debug.LogFormat(LogType.Warning, LogOption.NoStacktrace, null, $"{Prefix} {message}");
        }

        public static void LogError(string message) {
            Debug.LogFormat(LogType.Error, LogOption.NoStacktrace, null, $"{Prefix} {message}");
        }

        public static void LogException(Exception e) {
            Debug.LogFormat(LogType.Exception, LogOption.NoStacktrace, null, $"{Prefix} {e.Message}\n{e.StackTrace}");
        }
    }
}
