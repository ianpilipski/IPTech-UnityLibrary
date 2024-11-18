using System;

namespace IPTech.UnityServices {
    using Platform;

    public interface IUnityServicesManager {
        IAuthentication Authentication { get; }
        IAnalyticsManager AnalyticsManager { get; }
        ILeaderboardsManager LeaderboardsManager { get; }

        event Action<bool> ApplicationPaused;
        event Action<ConsentInfo> ConsentValueChanged;
        event Action Initialized;
        ConsentInfo Consent { get; set; }
        EServiceState State { get; }
    }
}