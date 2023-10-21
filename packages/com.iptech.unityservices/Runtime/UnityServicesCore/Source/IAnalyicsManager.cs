#if FALSE
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.UnityServices {
    public interface IAnalyticsManager {
        void CustomData(string eventName);
        void CustomData(string eventName, IDictionary<string, object> eventParams);
        void AdImpression(AdImpressionParameters adImpression);
    }

    public struct AdImpressionParameters
    {
        public AdCompletionStatus AdCompletionStatus;
        public AdProvider AdProvider;
        public string PlacementID;
        public string PlacementName;
        public AdPlacementType? PlacementType;
        public double? AdEcpmUsd;
        public string SdkVersion;
        public string AdImpressionID;
        public string AdStoreDstID;
        public string AdMediaType;
        public long? AdTimeWatchedMs;
        public long? AdTimeCloseButtonShownMs;
        public long? AdLengthMs;
        public bool? AdHasClicked;
        public string AdSource;
        public string AdStatusCallback;
    }

    public enum AdCompletionStatus
    {
        Completed,
        Partial,
        Incomplete
    }

    public enum AdProvider
    {
        AdColony,
        AdMob,
        Amazon,
        AppLovin,
        ChartBoost,
        Facebook,
        Fyber,
        Hyprmx,
        Inmobi,
        Maio,
        Pangle,
        Tapjoy,
        UnityAds,
        Vungle,
        IrnSource,
        Other
    }

    public enum AdPlacementType
    {
        BANNER,
        REWARDED,
        INTERSTITIAL,
        OTHER
    }
}
#endif