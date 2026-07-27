using System;

namespace IPTech.ConsentScreen {
    public enum EConsentValue {
        Unknown,
        Accepted,
        Declined,
        DeclinedDeleteMyData
    }

    public enum EConsentAge {
        Unknown,
        Child,
        Adult
    }

    public enum EIOSAppTrackingStatus {
        NotDetermined,
        Authorized,
        Denied,
        Restricted
    }

    [Serializable]
    public struct ConsentInfo {
        public EConsentValue Consent;
        public EConsentAge AgeInfo;
        public EIOSAppTrackingStatus IOSAppTrackingStatus;
    }
}
