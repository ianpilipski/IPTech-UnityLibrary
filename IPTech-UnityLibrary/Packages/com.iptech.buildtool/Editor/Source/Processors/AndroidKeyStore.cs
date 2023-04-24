using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

namespace IPTech.BuildTool.Processors {
    using System;
    using System.Threading.Tasks;
    using Encryption;

    public class AndroidKeyStore : BuildProcessor {
        public EncryptedItemAndroidKeyStoreSettings.Reference KeyStoreSettings;

        public override void PreprocessBuild(BuildReport report) { 
            var es = BuildToolsSettings.instance.EncryptedStorage;
            if(es.IsUnlocked) {
                if(es.TryGetDecryptedValue(KeyStoreSettings.Name, out EncryptedItem encItem)) {
                    var settings = (EncryptedItemAndroidKeyStoreSettings)encItem;

                    string keyStoreFilePath = SaveTempKeyStore(settings.KeyStoreFile);
                    
                    var origSettings = new OrigSettings() {
                        useCustomKeystore = PlayerSettings.Android.useCustomKeystore,
                        keyStorePass = PlayerSettings.Android.keystorePass,
                        keyStoreAlias = PlayerSettings.Android.keyaliasName,
                        keyStoreAliasPass = PlayerSettings.Android.keyaliasPass
                    };

                    PlayerSettings.Android.useCustomKeystore = true;
                    PlayerSettings.Android.keystorePass = settings.KeyStorePassword;
                    PlayerSettings.Android.keyaliasName = settings.KeyStoreAlias;
                    PlayerSettings.Android.keyaliasPass = settings.KeyStoreAliasPassword;
                    PlayerSettings.Android.keystoreName = keyStoreFilePath;
                    
                    CleanupAfterBuild(keyStoreFilePath, origSettings);
                } else {
                    throw new System.Exception("There was an issue trying to get the encrypted keystore");
                }
            } else {
                throw new System.Exception("You must unlock the buildtools encrypted storage to perform this action.");
            }
        }

        async void CleanupAfterBuild(string keyStoreFilePath, OrigSettings origSettings) {
            try {
                if(BuildPipeline.isBuildingPlayer) {
                    while(BuildPipeline.isBuildingPlayer) {
                        await Task.Yield();
                    }

                    PlayerSettings.Android.useCustomKeystore = true;
                    PlayerSettings.Android.keystorePass = origSettings.keyStorePass;
                    PlayerSettings.Android.keyaliasName = origSettings.keyStoreAlias;
                    PlayerSettings.Android.keyaliasPass = origSettings.keyStoreAliasPass;
                    PlayerSettings.Android.keystoreName = origSettings.keyStoreFilePath;

                    File.Delete(keyStoreFilePath);
                } else {
                    throw new Exception("Sensitive information will not be deleted after the build!!!");
                }
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }

        string SaveTempKeyStore(FileData fileData) {
            var tmpFileName = Path.GetRandomFileName();
            var tmpDir = Path.Combine(Application.dataPath, "..", "Temp");
            var tmpFilePath = Path.Combine(tmpDir, tmpFileName);

            if(!Directory.Exists(tmpDir)) {
                Directory.CreateDirectory(tmpDir);
            }

            File.WriteAllBytes(tmpFilePath, fileData.data);
            return tmpFilePath;
        }

        struct OrigSettings {
            public bool useCustomKeystore;
            public string keyStoreFilePath;
            public string keyStorePass;
            public string keyStoreAlias;
            public string keyStoreAliasPass;
        }
    }
}