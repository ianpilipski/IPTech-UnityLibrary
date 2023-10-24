
using System;


namespace IPTech.UnityServices {
    using Platform;

    public interface IUnityServicesManager {
        event Action<bool> ApplicationPaused;
        event Action<ConsentInfo> ConsentValueChanged;
        event Action Initialized;
        ConsentInfo Consent { get; set; }
        EPlatformState State { get; }
    }

    
}