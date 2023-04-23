using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace IPTech.BuildTool {
    [FilePath("ProjectSettings/IPTechBuildToolSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class BuildToolsSettings : ScriptableSingletonWithSubObjects<BuildToolsSettings> {
        const string ENC_STORAGE_DIR = "ProjectSettings/IPTechBuildToolSettings/Storage";
        public static string GetFullPathToRelativePackagePath(string path) {
            return Path.GetFullPath($"Packages/com.iptech.buildtool/{path}");
        }

        public string DefaultConfigPath = "BuildConfigs";
        public bool EnableBuildWindowIntegration = true;
        public List<string> BuildInEditorArguments = new List<string>() { "-buildNumber", "0001" };

        [InlineCreation]
        public List<BuildProcessor> BuildProcessors;

        EncryptedStorage<EncryptedItem> _encStorage;
        public EncryptedStorage<EncryptedItem> EncryptedStorage {
            get {
                if(_encStorage==null) {
                    _encStorage = new EncryptedStorage<EncryptedItem>("ProjectSettings/IPTechBuildToolSettings/Storage");
                }
                return _encStorage;
            }
        }
        
        private void OnEnable() {
            bool needsSave = false;
            if(hideFlags.HasFlag(HideFlags.NotEditable)) {
                hideFlags &= ~HideFlags.NotEditable;
                needsSave = true;
            }

            if(LoadPrevVersion()) {
                needsSave = true;
            }

            if(needsSave) {
                Save();
            }
        }

        public void Save() {
            Save(true);
        }

        public bool LoadPrevVersion() {
            var file = Path.Combine("ProjectSettings", "IPTechBuildToolSettings.json");
            if(File.Exists(file)) {
                string json = File.ReadAllText(file);
                JsonUtility.FromJsonOverwrite(json, this);
                File.Delete(file);
                return true;
            }
            return false;
        }

        protected override void GetSubObjectsToSave(List<UnityEngine.Object> objs) {
            base.GetSubObjectsToSave(objs);

            if(BuildProcessors!=null) {
                foreach(var pbp in BuildProcessors) {
                    if(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(pbp))) {
                        objs.Add(pbp);
                    }
                }
            }
        }
    }
}
