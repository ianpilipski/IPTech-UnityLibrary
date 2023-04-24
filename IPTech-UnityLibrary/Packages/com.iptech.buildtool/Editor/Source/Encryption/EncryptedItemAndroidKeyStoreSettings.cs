
using System;

namespace IPTech.BuildTool.Encryption {
    public class EncryptedItemAndroidKeyStoreSettings : EncryptedItem<EncryptedItemAndroidKeyStoreSettings> {
        public String KeyStorePassword;
        public String KeyStoreAlias;
        public String KeyStoreAliasPassword;
        [FileType("keystore")] public FileData KeyStoreFile; 
        
        protected override void InternalSerialize(EncryptedItemWriter writer) {
            writer.WriteString(nameof(KeyStorePassword), KeyStorePassword);
            writer.WriteString(nameof(KeyStoreAlias), KeyStoreAlias);
            writer.WriteString(nameof(KeyStoreAliasPassword), KeyStoreAliasPassword);
            writer.WriteFileData(nameof(KeyStoreFile), KeyStoreFile);
        }
        protected override void InternalDeserialize(EncryptedItemReader reader) {
            KeyStorePassword = reader.ReadString(nameof(KeyStorePassword));
            KeyStoreAlias = reader.ReadString(nameof(KeyStoreAlias));
            KeyStoreAliasPassword = reader.ReadString(nameof(KeyStoreAliasPassword));
            KeyStoreFile = reader.ReadFileData(nameof(KeyStoreFile));
        }
    }
}