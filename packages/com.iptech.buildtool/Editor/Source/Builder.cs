using System;
using System.Collections.Generic;
using UnityEditor;

namespace IPTech.BuildTool
{
    public static class Builder {
        

        public static void Build() {
            BuildWithArguments(Environment.GetCommandLineArgs());
        }

        public static void BuildWithArguments(string[] commandlineArguments) {
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
                    using(new TryUnlockEncryptedStorage()) {
                        bc.Build(args);
                    }
                    return;
                }
            }
            throw new Exception($"Could not find BuildConfig with the name {configName}");
        }

        class TryUnlockEncryptedStorage : IDisposable {
            bool relock;

            public TryUnlockEncryptedStorage() {
                if(BuildToolsSettings.instance.EncryptedStorage.HasPassword) {
                    if(!BuildToolsSettings.instance.EncryptedStorage.IsUnlocked) {
                        relock = true;
                        var pwd = Environment.GetEnvironmentVariable("IPTECH_BUILDTOOL_PASSWORD");
                        if(!string.IsNullOrEmpty(pwd)) {
                            BuildToolsSettings.instance.EncryptedStorage.Unlock(pwd);  
                        }
                    }
                }
            }

            public void Dispose() {
                if(relock) {
                    BuildToolsSettings.instance.EncryptedStorage.Lock();
                }
            }
        }

        static IDictionary<string,string> ParseCommandlineArgs(string[] cmdLineArgs) {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            
            if(cmdLineArgs.Length>0) {
                var args = cmdLineArgs;

                string prevArg = null;
                int i = 0;
                foreach(var arg in args) {
                    if(i == 0) {
                        retVal["executable"] = arg;
                    } else {
                        if(arg.StartsWith("-")) {
                            retVal[arg] = "";
                            prevArg = arg;
                        } else {
                            if(prevArg != null) {
                                retVal[prevArg] = arg;
                                prevArg = null;
                            }
                        }
                    }
                    i++;
                }
            }
            return retVal;
        }
    }
}
