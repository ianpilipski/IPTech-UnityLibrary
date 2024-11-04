using System;

namespace IPTech.Platform {
    using Utils;

    public interface IIPTechPlatform {
        IServiceLocator Services { get; }
        INetworkDetector Network { get; }
        void RunOnUnityThread(Action action);
        event Action Initialized;
        ConsentInfo Consent { get; set; }
        EServiceState State { get; }
        event Action<ConsentInfo> ConsentValueChanged;
        event Action<bool> ApplicationPaused;

        public ILeaderboardsManager Leaderboards { get; }
    }
}