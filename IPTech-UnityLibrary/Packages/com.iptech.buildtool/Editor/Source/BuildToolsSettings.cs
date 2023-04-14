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

        public string DefaultConfigPath = "BuildConfigs";
        public bool EnableBuildWindowIntegration = true;
        public List<string> BuildInEditorArguments = new List<string>() { "-buildNumber", "0001" };

        [AllowRefOrInst]
        public List<BuildProcessor> BuildProcessors;

        EncryptedStorage _encStorage;
        public EncryptedStorage EncryptedStorage {
            get {
                if(_encStorage==null) {
                    _encStorage = new EncryptedStorage("ProjectSettings/IPTechBuildToolSettings/Storage");
                }
                return _encStorage;
            }
        }
        
        private void OnEnable() {
            if(LoadPrevVersion()) {
                Save();
            }
            hideFlags &= ~HideFlags.NotEditable;
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
