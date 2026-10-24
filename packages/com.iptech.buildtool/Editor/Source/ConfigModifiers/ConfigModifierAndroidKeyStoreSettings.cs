

namespace IPTech.BuildTool {
    using System;
    using Encryption;
    using IPTech.BuildTool.Processors;

    [Obsolete("ConfigModifierAndroidKeyStoreSettings is obsolete. Use SetAndroidKeyStoreSettings instead.")]
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
