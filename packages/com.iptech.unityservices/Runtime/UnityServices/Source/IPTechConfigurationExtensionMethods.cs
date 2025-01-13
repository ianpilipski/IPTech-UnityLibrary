using IPTech.Platform;

namespace IPTech.UnityServices {
    public static class IPTechConfigurationExtensionMethods {
        public static IIPTechPlatformConfig ConfigureUnityServices(this IIPTechPlatformConfig config) {
            config.RegisterFactory<IUnityServicesManager>(new IPTechServiceFactory<IUnityServicesManager>((p) => {
                return new UnityServicesManager(p);
            }));

#if UNITY_AUTHSERVICE_INSTALLED || UNITY_LEADERBOARDS_INSTALLED || UNITY_REMOTECONFIG_INSTALLED
#if UNITY_AUTHSERVICE_INSTALLED
            config.RegisterFactory<IAuthentication>(new IPTechServiceFactory<IAuthentication>(p => {
                return p.Services.GetService<IUnityServicesManager>().Authentication;
            }));
#else
#error you need to install unity Authorization services to work with one of the packages you have installed
#endif
#endif

#if IPTECH_UNITYANALYTICS_INSTALLED
            config.RegisterFactory<IAnalyticsManager>(new IPTechServiceFactory<IAnalyticsManager>((p) => {
                return p.Services.GetService<IUnityServicesManager>().AnalyticsManager;
            }));
#endif

#if IPTECH_UNITYADVERTISING_INSTALLED
            //TODO: add ads
#endif

#if IPTECH_UNITYLEADERBOARDS_INSTALLED
            config.RegisterFactory<ILeaderboardsManager>(new IPTechServiceFactory<ILeaderboardsManager>(p => {
                return p.Services.GetService<IUnityServicesManager>().LeaderboardsManager;
            }));
#endif


#if IPTECH_UNITYREMOTECONFIG_INSTALLED
            config.RegisterFactory<IRemoteConfigManager>(new IPTechServiceFactory<IRemoteConfigManager>(p => {
                return p.Services.GetService<IUnityServicesManager>().RemoteConfigManager;
            }));
#endif


            return config;
        }
    }
}