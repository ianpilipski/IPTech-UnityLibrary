namespace IPTech.BuildTool.Encryption
{
    public class EncryptedItemPList : EncryptedItem<EncryptedItemPList> {
        [FileType("plist")]
        public FileData PList;

        public override string NameHintPropertyPath => $"{nameof(PList)}.{nameof(FileData.fileName)}";

        protected override void InternalDeserialize(EncryptedItemReader reader) {
            if(PList==null) PList = new FileData();

            PList.data = reader.ReadBytes(nameof(PList));
        }

        protected override void InternalSerialize(EncryptedItemWriter writer) {
            writer.WriteBytes(nameof(PList), PList.data);
        }
    }
}