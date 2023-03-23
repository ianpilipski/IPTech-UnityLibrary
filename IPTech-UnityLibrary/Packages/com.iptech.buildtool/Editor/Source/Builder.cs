using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    public static class Builder {
        public static void Build() {
            BuildWithArguments(Environment.CommandLine);
        }

        public static void BuildWithArguments(string commandlineArguments) {
            var args = ParseCommandlineArgs(commandlineArguments);
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

        static IDictionary<string,string> ParseCommandlineArgs(string cmdLine) {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            
            if(!string.IsNullOrEmpty(cmdLine)) {
                var args = cmdLine.Split(" -");
                
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
