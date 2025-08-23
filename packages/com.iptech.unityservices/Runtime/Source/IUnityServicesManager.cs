using System;

namespace IPTech.UnityServices {
    using Platform;

    public interface IUnityServicesManager : IIPTechPlatformService {
        IIPTechPlatform Platform { get; }
        
        INetworkDetector Network { get; }
        IAuthentication Authentication { get; }
        IAnalyticsManager AnalyticsManager { get; }
        IAdsManager AdsManager { get; }
        ILeaderboardsManager LeaderboardsManager { get; }
        IRemoteConfigManager RemoteConfigManager { get; }

        event Action<bool> ApplicationPaused;
        event Action<ConsentInfo> ConsentValueChanged;
        ConsentInfo Consent { get; set; }
    }
}