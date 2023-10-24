using System;

namespace IPTech.Platform {
    public interface IIPTechPlatform {
        INetworkDetector Network { get; }
        void RunOnUnityThread(Action action);
    }
}