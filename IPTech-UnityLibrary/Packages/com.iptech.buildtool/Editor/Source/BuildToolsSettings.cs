
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool {
    using System;
    using Encryption;
    using IPTech.BuildTool.Editors;
    using Unity.Collections.LowLevel.Unsafe;

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

        Internal.EncryptedStorage<EncryptedItem> _encStorage;
        public IEncryptedStorage<EncryptedItem> EncryptedStorage {
            get {
                if(_encStorage==null) {
                    _encStorage = new Internal.EncryptedStorage<EncryptedItem>("ProjectSettings/IPTechBuildToolSettings/Storage");
                }
                return _encStorage;
            }
        }
        
        protected BuildToolsSettings() : base() {
            EditorApplication.delayCall += instance.CheckCtor;
        }

        void CheckCtor() {
            if(hideFlags.HasFlag(HideFlags.NotEditable)) {
                hideFlags &= ~HideFlags.NotEditable;
            }

            if(LoadPrevVersion()) {
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


        public T GetDecryptedItem<T>(EncryptedItem<T>.Reference item) where T : EncryptedItem<T> {
            using(new ConditionallyUnlockStorage(true)) { 
                if(EncryptedStorage.TryGetDecryptedValue(item.Name, out EncryptedItem value)) {
                    return (T)value;
                }
                throw new Exception("had a problem decrypting storage to retrieve item for build");
            }
        }

        public void ImportEncryptedItem(string fullTypeName) {
            var t = Type.GetType(fullTypeName);
            if(t!=null) {
                ImportEncryptedItem(t);
            } else {
                throw new Exception("could not find type " + fullTypeName);
            }
        }

        public void ImportEncryptedItem(Type type) {
            using(new ConditionallyUnlockStorage(false)) {
                if(EncryptedStorage.IsUnlocked) {
                    EncryptedStorageImportDialog.ImportItem(type);
                }
            }
        }

        class ConditionallyUnlockStorage : IDisposable {
            bool relock;
            
            public ConditionallyUnlockStorage(bool assertOnFailure) {
                var es = BuildToolsSettings.instance.EncryptedStorage;
                if(!es.IsUnlocked) {
                    if(!Application.isBatchMode) {
                        var pw = PasswordInputDialog.Display();
                        if(!string.IsNullOrEmpty(pw)) {
                            es.Unlock(pw);
                        } else if(assertOnFailure) {
                            throw new Exception("cancelled encrypted storage password prompt");
                        }
                    } else if(assertOnFailure) {
                        throw new Exception("you must unlock the encrypted storage by setting the IPTECH_BUILDTOOL_PASSWORD environment variable");
                    }
                }
            }

            public void Dispose() {
                if(relock) {
                    BuildToolsSettings.instance.EncryptedStorage.Lock();
                }
            }
        }
    }
}
