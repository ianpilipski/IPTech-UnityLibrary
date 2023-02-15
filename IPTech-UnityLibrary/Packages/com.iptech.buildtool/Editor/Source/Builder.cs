using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public static class Builder {
        public static void Build() {
            var args = ParseCommandlineArgs();
            if(args.TryGetValue("-buildConfig", out string configName)) {
                ExecuteBuildForConfigName(configName, args);
            } else {
                throw new Exception("Could not find commanline argument -buildConfig <name>");
            }
        }

        private static void ExecuteBuildForConfigName(string configName, IDictionary<string,string> args) {
            var assets = AssetDatabase.FindAssets($"t:BuildConfig {configName}");
            foreach(var asset in assets) {
                var bc = AssetDatabase.LoadAssetAtPath<BuildConfig>(AssetDatabase.GUIDToAssetPath(asset));
                if(bc!=null && bc.name == configName) {
                    bc.Build(args);
                    return;
                }
            }
            throw new Exception($"Could not find BuildConfig with the name {configName}");
        }

        static IDictionary<string,string> ParseCommandlineArgs() {
            Dictionary<string, string> retVal = new Dictionary<string, string>();

            var cmdLine = Environment.CommandLine;
            if(!string.IsNullOrEmpty(cmdLine)) {
                var args = cmdLine.Split('-');
                
                foreach(var arg in args) {
                    string key = arg;
                    string val = "";
                    var i = arg.IndexOf(' ');
                    if(i>0) {
                        key = arg.Substring(0, i);
                        val = arg.Substring(i, arg.Length - i).Trim();
                    }

                    retVal["-" + key] = val;
                }
            }
            return retVal;
        }
    }
}
