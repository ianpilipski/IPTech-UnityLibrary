
using System;

namespace IPTech.UnityServices {
    public enum EState {
        Initializing,
        NotOnline,
        Online
    }

    public enum EConsentAge {
        Unknown,
        Under13,
        Over13
    }

    public enum EConsentValue {
        Unknown,
        Accepted,
        Declined,
        DeclinedDeleteMyData
    }

    public interface IUnityServicesManager {
        event Action<bool> ApplicationPaused;
        event Action<ConsentInfo> ConsentValueChanged;
        event Action Initialized;
        ConsentInfo Consent { get; set; }
        EState State { get; }
       //T GetService<T>();
    }

    [Serializable]
    public struct ConsentInfo {
        public EConsentValue Consent;
        public EConsentAge AgeInfo;
    }
}