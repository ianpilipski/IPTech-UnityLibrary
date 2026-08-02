using IPTech.Platform;

namespace IPTech.AgeVerification
{
    public static class IPTechConfigurationExtensionMethods
    {
        public static void ConfigureAgeVerification(this IIPTechPlatformConfig config)
        {
            config.RegisterFactory<IAgeVerificationService>(new IPTechServiceFactory<IAgeVerificationService>((p) => {
                return new AgeVerificationService();
            }));
        }

        public static IAgeVerificationService GetAgeVerification(this IIPTechPlatform platform)
        {
            try 
            {
                return platform.Services.GetService<IAgeVerificationService>();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception("Failed to get AgeVerificationManager from platform services. Make sure you have called ConfigureAgeVerification() on the platform config before initializing the platform.", ex);
            }
        }
    }    
}