using UnityEditor;

namespace IPTech.BuildTool {
    public class EncryptedItemFile : EncryptedItem<EncryptedItemFile> {
        public FileData File;

        public override string NameHintPropertyPath => $"{nameof(File)}.{nameof(FileData.fileName)}";

        protected override void InternalDeserialize(EncryptedItemReader reader) {
            if(File==null) File=new FileData();
            
            File.data = reader.ReadBytes(nameof(File));
        }

        protected override void InternalSerialize(EncryptedItemWriter writer) {
            writer.WriteBytes(nameof(File), this.File.data);
        }
    }
}