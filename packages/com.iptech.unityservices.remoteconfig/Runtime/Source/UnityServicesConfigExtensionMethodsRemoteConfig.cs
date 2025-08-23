using UnityEngine;
using IPTech.Platform;
    
namespace IPTech.UnityServices
{
    using RemoteConfig;
    public static class UnityServicesConfigExtensionMethodsRemoteConfig
    {
        public static UnityServicesConfig ConfigureRemoteConfig(this UnityServicesConfig config)
        {
            config.RegisterFactory<IRemoteConfigManager>((usm) => new RemoteConfigManager(usm));
            return config;
        }
    }
}
