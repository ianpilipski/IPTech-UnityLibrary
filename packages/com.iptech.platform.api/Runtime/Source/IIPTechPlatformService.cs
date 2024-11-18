using System;

namespace IPTech.Platform {
    public interface IIPTechPlatformService {
        EServiceState State { get; }
        EOnlineState OnlineState { get; }
        
        event Action<EServiceState> Initialized;
        event Action<EOnlineState> OnlineStateChanged;
    }
}
