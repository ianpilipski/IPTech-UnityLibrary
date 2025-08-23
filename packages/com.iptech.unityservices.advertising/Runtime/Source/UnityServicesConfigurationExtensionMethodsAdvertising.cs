using IPTech.Platform;
using UnityEngine;

namespace IPTech.UnityServices
{
    public static class UnityServicesConfigurationExtensionMethodsAdvertising
    {
        public static UnityServicesConfig ConfigureAds(this UnityServicesConfig unityServicesConfig, AdsManager.Config config)
        {
            unityServicesConfig.RegisterFactory<IAdsManager>((usm) =>
            {
                return new AdsManager(usm, config);
            });
        
            return unityServicesConfig;
        }
    }
}
