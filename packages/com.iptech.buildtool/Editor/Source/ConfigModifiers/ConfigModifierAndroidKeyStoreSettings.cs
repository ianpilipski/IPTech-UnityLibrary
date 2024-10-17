using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using System.IO;

namespace IPTech.BuildTool {
    using Encryption;

    public class ConfigModifierAndroidKeyStoreSettings : ConfigModifier {
        public EncryptedItemAndroidKeyStoreSettings.Reference KeyStoreSettings;

        OrigSettings origSettings;
        string tmpKeyStoreFilePath;

        public override void ModifyProject(BuildTarget buildTarget) {
            var settings = GetDecryptedItem(KeyStoreSettings);
            
            tmpKeyStoreFilePath = SaveTempKeyStore(settings.KeyStoreFile);
            
            origSettings = new OrigSettings() {
                useCustomKeystore = PlayerSettings.Android.useCustomKeystore,
                keystoreName = PlayerSettings.Android.keystoreName,
                keystorePass = PlayerSettings.Android.keystorePass,
                keyaliasName = PlayerSettings.Android.keyaliasName,
                keyaliasPass = PlayerSettings.Android.keyaliasPass
            };

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystorePass = settings.KeyStorePassword;
            PlayerSettings.Android.keyaliasName = settings.KeyStoreAlias;
            PlayerSettings.Android.keyaliasPass = settings.KeyStoreAliasPassword;
            PlayerSettings.Android.keystoreName = tmpKeyStoreFilePath;
        }

        public override void RestoreProject(BuildTarget buildTarget) {
            PlayerSettings.Android.useCustomKeystore = origSettings.useCustomKeystore;
            PlayerSettings.Android.keystorePass = origSettings.keystorePass;
            PlayerSettings.Android.keyaliasName = origSettings.keyaliasName;
            PlayerSettings.Android.keyaliasPass = origSettings.keyaliasPass;
            PlayerSettings.Android.keystoreName = origSettings.keystoreName;

            File.Delete(tmpKeyStoreFilePath);
        }
        

        string SaveTempKeyStore(FileData fileData) {
            var tmpFileName = Path.GetRandomFileName();
            var tmpDir = Path.Combine(Application.dataPath, "..", "Temp");
            var tmpFilePath = Path.GetFullPath(Path.Combine(tmpDir, tmpFileName));

            if(!Directory.Exists(tmpDir)) {
                Directory.CreateDirectory(tmpDir);
            }

            File.WriteAllBytes(tmpFilePath, fileData.data);
            return tmpFilePath;
        }

        struct OrigSettings {
            public bool useCustomKeystore;
            public string keystoreName;
            public string keystorePass;
            public string keyaliasName;
            public string keyaliasPass;
        }
    }
}
