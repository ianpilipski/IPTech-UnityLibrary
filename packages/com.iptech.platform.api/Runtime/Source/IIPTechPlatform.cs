using System;

namespace IPTech.Platform {
    public interface IIPTechPlatform {
        INetworkDetector Network { get; }
        void RunOnUnityThread(Action action);
        event Action Initialized;
        ConsentInfo Consent { get; set; }
        EServiceState State { get; }
        event Action<ConsentInfo> ConsentValueChanged;
        event Action<bool> ApplicationPaused;
    }
}