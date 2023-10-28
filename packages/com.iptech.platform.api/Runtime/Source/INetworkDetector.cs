using System;

namespace IPTech.Platform {
    public interface INetworkDetector {
        ENetworkState State { get; }
        event Action<ENetworkState> NetworkStateChanged;
    }
}
