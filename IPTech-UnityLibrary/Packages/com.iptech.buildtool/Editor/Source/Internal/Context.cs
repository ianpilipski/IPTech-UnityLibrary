
namespace IPTech.BuildTool.Internal {
    using Encryption;
    
    public static class Context {
        public static ReflectionListGenerator ListGenerator;

        static Context() {
            ListGenerator = new ReflectionListGenerator(
                typeof(BuildConfig), 
                typeof(EncryptedItem),
                typeof(ConfigModifier));
        }
    }
}