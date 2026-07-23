using IPTech.Platform;
using IPTech.UnityServices;
using UnityEngine;

public class UnityServicesExample : MonoBehaviour
{
    public IIPTechPlatform CreatePlatform()
    {
        var config = new IPTechPlatformConfig();

        ConfigurePlatform(config);

        return new IPTechPlatform(config);
    }

    public void ConfigurePlatform(IIPTechPlatformConfig platformConfig)
    {
        var config = new UnityServicesConfig();
        config.ConfigureAnalytics();
        config.ConfigureAds(new AdsManager.Config(
            "your_appKey", 
            new () { 
                { AdType.Interstitial, "inter_adUnitId" },
                { AdType.Reward, "reward_adUnitId" }
            }));
        config.ConfigureLeaderboards();
        config.ConfigureRemoteConfig();

        platformConfig.ConfigureUnityServices(config);
    }
}
