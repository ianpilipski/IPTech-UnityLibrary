using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

namespace IPTech.UnityServices {
    using Platform;

    using UAdImpressionParameters = Unity.Services.Analytics.AdImpressionParameters;
    using UAdCompletionStatus = Unity.Services.Analytics.AdCompletionStatus;
    using UAdPlacementType = Unity.Services.Analytics.AdPlacementType;
    using UAdProvider = Unity.Services.Analytics.AdProvider;

    public class AnalyticsManager  {
        IAnalyticsService service;
        readonly IUnityServicesManager unityServicesManager;
        readonly List<Action> queuedEvents;
        bool isInitialized;

        public AnalyticsManager(IUnityServicesManager ipTechUnityServices) {
            this.queuedEvents = new List<Action>();
            this.unityServicesManager = ipTechUnityServices;
            ipTechUnityServices.Initialized += HandleInitialized;
        }

        void HandleInitialized() {
            unityServicesManager.ConsentValueChanged += HandleConsentChanged;
            HandleConsentChanged(unityServicesManager.Consent);
        }

        void HandleConsentChanged(ConsentInfo consent) {
            if(consent.Consent == EConsentValue.Unknown) return;
            if(!isInitialized) {
                Initialize(consent);
            } else {
                ChangeStateBasedOnConsent(consent);
            }
        }

        void ChangeStateBasedOnConsent(ConsentInfo consent) {
            try {
                if(service!=null) {
                    if(consent.Consent == EConsentValue.Accepted && unityServicesManager.State == EPlatformState.Online) {
                        service.StartDataCollection();
                    } else {
                        service.StopDataCollection();
                        if(consent.Consent == EConsentValue.DeclinedDeleteMyData) {
                            service.RequestDataDeletion();
                        }
                    }
                }
            } catch(Exception e) {
                service = null;
                Debug.LogException(e);
            }
        }

        void Initialize(ConsentInfo consent) {
            try {
                service = AnalyticsService.Instance;
                ChangeStateBasedOnConsent(consent);
                ProcessEvents();
            } catch(Exception e) {
                Debug.LogException(e);
            } finally {
                isInitialized = true;
            }
        }

        void ProcessEvents() {
            try {
                isInitialized = true;
                if(service!=null) {
                    foreach(var ev in queuedEvents) {
                        SafeInvoke(ev);
                    }
                }
            } catch(Exception e) {
                Debug.LogException(e);
            } finally {
                queuedEvents.Clear();
            }

            void SafeInvoke(Action action) {
                try {
                    action.Invoke();
                } catch(Exception e) {
                    Debug.LogException(e);
                }
            }
        }

        public void CustomData(string eventName) {
            if(!isInitialized) {
                queuedEvents.Add(() => CustomData(eventName));
                return;
            }
            if(service == null) return;
            service.CustomData(eventName);
        }

        public void CustomData(string eventName, IDictionary<string, object> eventParams) {
            if(!isInitialized) {
                queuedEvents.Add(() => CustomData(eventName, eventParams));
                return;
            }
            if(service == null) return;
            service.CustomData(eventName, eventParams);
        }

        public void AdImpression(AdImpressionParameters adImpression) {
            if(!isInitialized) {
                queuedEvents.Add(() => AdImpression(adImpression));
                return;
            }
            if(service == null) return;
            service.AdImpression(adImpression);
        }

        #if FALSE
        UAdImpressionParameters ConvertToUnity(AdImpressionParameters imp) {
            return new UAdImpressionParameters() {
                AdCompletionStatus = ConvertToUnity(imp.AdCompletionStatus),
                AdProvider = ConvertToUnity(imp.AdProvider),
                PlacementID = imp.PlacementID,
                PlacementName = imp.PlacementName,
                PlacementType = ConvertToUnity(imp.PlacementType),
                AdEcpmUsd = imp.AdEcpmUsd,
                SdkVersion = imp.SdkVersion,
                AdImpressionID = imp.AdImpressionID,
                AdStoreDstID = imp.AdStoreDstID,
                AdMediaType = imp.AdMediaType,
                AdTimeWatchedMs = imp.AdTimeWatchedMs,
                AdTimeCloseButtonShownMs = imp.AdTimeCloseButtonShownMs,
                AdLengthMs = imp.AdLengthMs,
                AdHasClicked = imp.AdHasClicked,
                AdSource = imp.AdSource,
                AdStatusCallback = imp.AdStatusCallback
            };
        }

        UAdCompletionStatus ConvertToUnity(AdCompletionStatus val) {
            switch(val) {
                case AdCompletionStatus.Completed: return UAdCompletionStatus.Completed;
                case AdCompletionStatus.Incomplete: return UAdCompletionStatus.Incomplete;
                case AdCompletionStatus.Partial: return UAdCompletionStatus.Partial;
            }
            throw new Exception("AdCompletionStatus not known, please update the code");
        }

        UAdProvider ConvertToUnity(AdProvider val) {
            switch(val) {
                case AdProvider.AdColony: return UAdProvider.AdColony;
                case AdProvider.AdMob: return UAdProvider.AdMob;
                case AdProvider.Amazon: return UAdProvider.Amazon;
                case AdProvider.AppLovin: return UAdProvider.AppLovin;
                case AdProvider.ChartBoost: return UAdProvider.ChartBoost;
                case AdProvider.Facebook: return UAdProvider.Facebook;
                case AdProvider.Fyber: return UAdProvider.Fyber;
                case AdProvider.Hyprmx: return UAdProvider.Hyprmx;
                case AdProvider.Inmobi: return UAdProvider.Inmobi;
                case AdProvider.Maio: return UAdProvider.Maio;
                case AdProvider.Pangle: return UAdProvider.Pangle;
                case AdProvider.Tapjoy: return UAdProvider.Tapjoy;
                case AdProvider.UnityAds: return UAdProvider.UnityAds;
                case AdProvider.Vungle: return UAdProvider.Vungle;
                case AdProvider.IrnSource: return UAdProvider.IrnSource;
                case AdProvider.Other: return UAdProvider.Other;
            }
            throw new Exception("AdProvider not known, please update the code");
        }

        UAdPlacementType? ConvertToUnity(AdPlacementType? adPlacementType) {
            if(!adPlacementType.HasValue) return null;

            switch(adPlacementType.Value) {
                case AdPlacementType.BANNER: return UAdPlacementType.BANNER;
                case AdPlacementType.REWARDED: return UAdPlacementType.REWARDED;
                case AdPlacementType.INTERSTITIAL: return UAdPlacementType.INTERSTITIAL;
                case AdPlacementType.OTHER: return UAdPlacementType.OTHER;
            }
            throw new Exception("AdPlacementType not known, please update the code");
        }
        #endif
    }
}