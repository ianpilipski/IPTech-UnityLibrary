using System;

namespace IPTech.UnityServices {
    using Platform;

    public interface IUnityServicesManager : IIPTechPlatformService {
        INetworkDetector Network { get; }
        IAuthentication Authentication { get; }
        IAnalyticsManager AnalyticsManager { get; }
        ILeaderboardsManager LeaderboardsManager { get; }
        IRemoteConfigManager RemoteConfigManager { get; }

        event Action<bool> ApplicationPaused;
        event Action<ConsentInfo> ConsentValueChanged;
        ConsentInfo Consent { get; set; }
    }
}