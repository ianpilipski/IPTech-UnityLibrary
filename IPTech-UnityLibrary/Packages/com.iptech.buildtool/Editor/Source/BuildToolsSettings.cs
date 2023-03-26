using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IPTech.BuildTool {
    [Serializable]
    public class BuildToolsSettings {
        static BuildToolsSettings inst;

        public string DefaultConfigPath = "BuildConfigs";
        public bool AddGradleWrapper;
        public bool UsesNonExemptEncryption;
        public List<string> BuildInEditorArguments = new List<string>() { "-buildNumber", "0001" };

        private BuildToolsSettings() {
            AddGradleWrapper = true;
        }

        public static void Save() {
            File.WriteAllText(SettingsFilePath, JsonUtility.ToJson(Inst));
        }

        public static BuildToolsSettings Inst {
            get {
                try {
                    if(inst == null) {
                        if(File.Exists(SettingsFilePath)) {
                            string json = File.ReadAllText(SettingsFilePath);
                            inst = JsonUtility.FromJson<BuildToolsSettings>(json);
                        }
                    }
                } catch(Exception e) {
                    Debug.LogException(e);
                }

                if(inst == null) {
                    inst = new BuildToolsSettings();
                }
                return inst;
            }
        }

        static string SettingsFilePath {
            get {
                return Path.Combine("ProjectSettings", "IPTechBuildToolSettings.json");
            }
        }
    }
}
