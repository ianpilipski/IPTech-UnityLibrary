
using System;

namespace IPTech.UnityServices {
    public enum EState {
        Initializing,
        NotOnline,
        Online
    }

    public enum EConsentValue {
        Unknown,
        Accepted,
        Declined,
        DeclinedDeleteMyData
    }

    public interface IUnityServicesManager {
        event Action<bool> ApplicationPaused;
        event Action<EConsentValue> ConsentValueChanged;
        event Action Initialized;
        EConsentValue Consent { get; set; }
        EState State { get; }
    }
}