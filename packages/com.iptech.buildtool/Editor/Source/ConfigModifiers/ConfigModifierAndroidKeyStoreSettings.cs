using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using System.IO;

namespace IPTech.BuildTool {
    using Encryption;
    using IPTech.BuildTool.Processors;

    public class ConfigModifierAndroidKeyStoreSettings : ConfigModifier {
        public EncryptedItemAndroidKeyStoreSettings.Reference KeyStoreSettings;

        public override BuildProcessor ConvertToBuildProcessor() {
            var bp = CreateInstance<SetAndroidKeyStoreSettings>();
            bp.name = nameof(SetAndroidKeyStoreSettings);
            bp.KeyStoreSettings = KeyStoreSettings;
            return bp;
        }

        
    }
}
