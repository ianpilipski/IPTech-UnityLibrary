using System;
using System.Collections.Generic;
using UnityEngine;
using USA = Unity.Services.Analytics;

namespace IPTech.UnityServices {
    using Platform;

    public class AnalyticsManager : IAnalyticsManager  {
        USA.IAnalyticsService service;
        readonly IUnityServicesManager unityServicesManager;
        readonly List<Action> queuedEvents;
        bool isInitialized;

        public AnalyticsManager(IUnityServicesManager ipTechUnityServices) {
            this.queuedEvents = new List<Action>();
            this.unityServicesManager = ipTechUnityServices;
            HookInitialize();
        }

        void HookInitialize() {
            if(this.unityServicesManager.State != EServiceState.Initializing) {
                HandleInitialized();
            } else {
                this.unityServicesManager.Initialized += HandleInitialized;
            }
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
                    if(consent.Consent == EConsentValue.Accepted && unityServicesManager.State == EServiceState.Online) {
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
                if(this.unityServicesManager.State == EServiceState.Online) {
                    service = USA.AnalyticsService.Instance;
                }
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
            service.AdImpression(Convert(adImpression));
        }

        Unity.Services.Analytics.AdImpressionParameters Convert(AdImpressionParameters parameters) {
            return new Unity.Services.Analytics.AdImpressionParameters() {
                AdCompletionStatus = Convert(parameters.AdCompletionStatus),
                AdEcpmUsd = parameters.AdEcpmUsd,
                AdHasClicked = parameters.AdHasClicked,
                AdImpressionID = parameters.AdImpressionID,
                AdLengthMs = parameters.AdLengthMs,
                AdMediaType = parameters.AdMediaType,
                AdProvider = Convert(parameters.AdProvider),
                AdSource = parameters.AdSource,
                AdStatusCallback = parameters.AdStatusCallback,
                AdStoreDstID = parameters.AdStoreDstID,
                AdTimeCloseButtonShownMs = parameters.AdTimeCloseButtonShownMs,
                AdTimeWatchedMs = parameters.AdTimeWatchedMs,
                PlacementID = parameters.PlacementID,
                PlacementName = parameters.PlacementName,
                PlacementType = Convert(parameters.PlacementType),
                SdkVersion = parameters.SdkVersion
            };
        }

        Unity.Services.Analytics.AdCompletionStatus Convert(AdCompletionStatus status) {
            return status switch {
                AdCompletionStatus.Completed => USA.AdCompletionStatus.Completed,
                AdCompletionStatus.Incomplete => USA.AdCompletionStatus.Incomplete,
                AdCompletionStatus.Partial => USA.AdCompletionStatus.Partial,
                _ => throw new ArgumentOutOfRangeException(nameof(status), $"not expecting AdCompletionStatus {status}")
            };
        }

        Unity.Services.Analytics.AdProvider Convert(AdProvider provider) {
            return provider switch {
                AdProvider.AdColony => USA.AdProvider.AdColony,
                AdProvider.AdMob => USA.AdProvider.AdMob,
                AdProvider.Amazon => USA.AdProvider.Amazon,
                AdProvider.AppLovin => USA.AdProvider.AppLovin,
                AdProvider.ChartBoost => USA.AdProvider.ChartBoost,
                AdProvider.Facebook => USA.AdProvider.Facebook,
                AdProvider.Fyber => USA.AdProvider.Fyber,
                AdProvider.Hyprmx => USA.AdProvider.Hyprmx,
                AdProvider.Inmobi => USA.AdProvider.Inmobi,
                AdProvider.IrnSource => USA.AdProvider.IrnSource,
                AdProvider.Maio => USA.AdProvider.Maio,
                AdProvider.Other => USA.AdProvider.Other,
                AdProvider.Pangle => USA.AdProvider.Pangle,
                AdProvider.Tapjoy => USA.AdProvider.Tapjoy,
                AdProvider.UnityAds => USA.AdProvider.UnityAds,
                AdProvider.Vungle => USA.AdProvider.Vungle,
                _ => throw new ArgumentOutOfRangeException(nameof(provider), $"not expecteing AdProvider {provider}")
            };
        }

        USA.AdPlacementType? Convert(AdPlacementType? placementType) {
            if(placementType == null) return null;
            return placementType switch {
                AdPlacementType.BANNER => USA.AdPlacementType.BANNER,
                AdPlacementType.INTERSTITIAL => USA.AdPlacementType.INTERSTITIAL,
                AdPlacementType.REWARDED => USA.AdPlacementType.REWARDED,
                AdPlacementType.OTHER => USA.AdPlacementType.OTHER,
                _ => throw new ArgumentOutOfRangeException(nameof(placementType), $"not expecting AdPlacementType {placementType}")
            };
        }
    }
}