using IPTech.Platform;

namespace IPTech.UnityServices {
    public static class IPTechConfigurationExtensionMethods {
        public static IIPTechPlatformConfig ConfigureUnityServices(this IIPTechPlatformConfig config, UnityServicesConfig unityServicesConfig) {
            config.RegisterFactory<IUnityServicesManager>(new IPTechServiceFactory<IUnityServicesManager>((p) => {
                return new UnityServicesManager(p, unityServicesConfig);
            }));

#if UNITY_AUTHSERVICE_INSTALLED
            config.RegisterFactory<IAuthentication>(new IPTechServiceFactory<IAuthentication>(p => {
                return p.Services.GetService<IUnityServicesManager>().Authentication;
            }));
#else
#error you need to install unity Authorization services to work with one of the packages you have installed
#endif

            unityServicesConfig.Visit(config);
            
            return config;
        }
    }
}