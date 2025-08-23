using IPTech.Platform;
using UnityEngine;

namespace IPTech.UnityServices
{
    public static class UnityServicesConfigExtensionMethodsAnalytics
    {
        public static UnityServicesConfig ConfigureAnalytics(this UnityServicesConfig config)
        {
            config.RegisterFactory<IAnalyticsManager>((usm) => new AnalyticsManager(usm));
            return config;
        }
    }
}
