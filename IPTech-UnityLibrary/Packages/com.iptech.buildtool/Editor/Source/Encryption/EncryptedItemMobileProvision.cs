namespace IPTech.BuildTool
{
    public class EncryptedItemMobileProvision : EncryptedItem<EncryptedItemMobileProvision> {
        [FileType("mobileprovision")]
        public FileData MobileProvision;

        public override string NameHintPropertyPath => $"{nameof(MobileProvision)}.{nameof(FileData.fileName)}";
        
        protected override void InternalDeserialize(EncryptedItemReader reader) {
            if(MobileProvision==null) MobileProvision = new FileData();
            
            MobileProvision.data = reader.ReadBytes(nameof(MobileProvision));
        }

        protected override void InternalSerialize(EncryptedItemWriter writer) {
            writer.WriteBytes(nameof(MobileProvision), MobileProvision.data);
        }
    }
}