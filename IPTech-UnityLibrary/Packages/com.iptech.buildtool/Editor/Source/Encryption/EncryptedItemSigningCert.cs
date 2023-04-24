

namespace IPTech.BuildTool.Encryption {
    public class EncryptedItemSigningCert : EncryptedItem<EncryptedItemSigningCert> {
        public string Password;

        [FileType("p12")]
        public FileData SigningCert;

        public override string NameHintPropertyPath => $"{nameof(SigningCert)}.{nameof(FileData.fileName)}";

        protected override void InternalDeserialize(EncryptedItemReader reader) {
            if(SigningCert==null) SigningCert = new FileData();

            Password = reader.ReadString(nameof(Password));
            SigningCert.data = reader.ReadBytes(nameof(SigningCert));
        }

        protected override void InternalSerialize(EncryptedItemWriter writer) {
            writer.WriteString(nameof(Password), Password);
            writer.WriteBytes(nameof(SigningCert), SigningCert.data);
        }
    }
}