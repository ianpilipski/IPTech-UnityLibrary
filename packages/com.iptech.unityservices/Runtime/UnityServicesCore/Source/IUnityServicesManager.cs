using System;

namespace IPTech.UnityServices {
    using Platform;

    public interface IUnityServicesManager : IIPTechPlatformService {
        IAuthentication Authentication { get; }
        IAnalyticsManager AnalyticsManager { get; }
        ILeaderboardsManager LeaderboardsManager { get; }

        event Action<bool> ApplicationPaused;
        event Action<ConsentInfo> ConsentValueChanged;
        ConsentInfo Consent { get; set; }
    }
}