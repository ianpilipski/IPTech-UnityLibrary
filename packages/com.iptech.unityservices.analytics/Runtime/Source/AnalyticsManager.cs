using System;
using System.Collections.Generic;
using UnityEngine;
using USA = Unity.Services.Analytics;

namespace IPTech.UnityServices {
    using Platform;
    using UnityEngine.UnityConsent;

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
                HandleInitialized(this.unityServicesManager.State);
            } else {
                this.unityServicesManager.Initialized += HandleInitialized;
            }
        }

        void HandleInitialized(EServiceState state) {
            unityServicesManager.ConsentValueChanged += HandleConsentChanged;
            HandleConsentChanged(unityServicesManager.Consent);
        }

        void HandleConsentChanged(ConsentInfo consent) {
            Debug.Log($"[IPTech.UnityServices.Analytics] consent value changed: Consent={consent.Consent}, Age={consent.AgeInfo}");
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
                    if (consent.Consent == EConsentValue.Accepted && unityServicesManager.State == EServiceState.Initialized)
                    {
                        Debug.Log($"[IPTech.UnityServices.Analytics] starting data collection");
                        var cs = EndUserConsent.GetConsentState();
                        if (cs.AnalyticsIntent != ConsentStatus.Granted)
                        {
                            cs.AnalyticsIntent = ConsentStatus.Granted;
                            EndUserConsent.SetConsentState(cs);
                        }
                    }
                    else
                    {
                        Debug.Log($"[IPTech.UnityServices.Analytics] stopping data collection");
                        var cs = EndUserConsent.GetConsentState();
                        if (cs.AnalyticsIntent != ConsentStatus.Denied)
                        {
                            cs.AnalyticsIntent = ConsentStatus.Denied;
                            EndUserConsent.SetConsentState(cs);
                        }
                        if (consent.Consent == EConsentValue.DeclinedDeleteMyData)
                        {
                            Debug.Log($"[IPTech.UnityServices.Analytics] requesting data deletion");
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
                if(this.unityServicesManager.State == EServiceState.Initialized) {
                    Debug.Log($"[IPTech.UnityServices.Analytics] initializing");
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
            service.RecordEvent(new USA.CustomEvent(eventName));
        }

        public void CustomData(string eventName, IDictionary<string, object> eventParams) {
            if(!isInitialized) {
                queuedEvents.Add(() => CustomData(eventName, eventParams));
                return;
            }
            if(service == null) return;
            var customEvent = new USA.CustomEvent(eventName);
            foreach(var kvp in eventParams) {
                customEvent.Add(kvp.Key, kvp.Value);
            }
            service.RecordEvent(customEvent);
        }

        public void AdImpression(AdImpressionParameters adImpression) {
            if(!isInitialized) {
                queuedEvents.Add(() => AdImpression(adImpression));
                return;
            }
            if(service == null) return;
            service.RecordEvent(Convert(adImpression));
        }

        USA.AdImpressionEvent Convert(AdImpressionParameters parameters)
        {
            var retVal = new USA.AdImpressionEvent()
            {
                AdCompletionStatus = Convert(parameters.AdCompletionStatus),
                AdProvider = Convert(parameters.AdProvider)
            };

            if(!string.IsNullOrWhiteSpace(parameters.AdImpressionID)) retVal.AdImpressionId = parameters.AdImpressionID;
            if(!string.IsNullOrWhiteSpace(parameters.AdMediaType)) retVal.AdMediaType = parameters.AdMediaType;
            if(!string.IsNullOrWhiteSpace(parameters.AdSource)) retVal.AdSource = parameters.AdSource;
            if(!string.IsNullOrWhiteSpace(parameters.AdStatusCallback)) retVal.AdStatusCallback = parameters.AdStatusCallback;
            if(!string.IsNullOrWhiteSpace(parameters.AdStoreDstID)) retVal.AdStoreDestinationId = parameters.AdStoreDstID;
            if(!string.IsNullOrWhiteSpace(parameters.PlacementID)) retVal.PlacementId = parameters.PlacementID;
            if(!string.IsNullOrWhiteSpace(parameters.PlacementName)) retVal.PlacementName = parameters.PlacementName;
            if(!string.IsNullOrWhiteSpace(parameters.SdkVersion)) retVal.AdSdkVersion = parameters.SdkVersion;
            if (parameters.AdEcpmUsd.HasValue) retVal.AdEcpmUsd = parameters.AdEcpmUsd.Value;
            if (parameters.AdHasClicked.HasValue) retVal.AdHasClicked = parameters.AdHasClicked.Value;
            if (parameters.AdLengthMs.HasValue) retVal.AdLengthMs = parameters.AdLengthMs.Value;
            if (parameters.AdTimeCloseButtonShownMs.HasValue) retVal.AdTimeCloseButtonShownMs = parameters.AdTimeCloseButtonShownMs.Value;
            if (parameters.AdTimeWatchedMs.HasValue) retVal.AdTimeWatchedMs = parameters.AdTimeWatchedMs.Value;
            if (parameters.PlacementType.HasValue) retVal.PlacementType = Convert(parameters.PlacementType.Value);
            return retVal;
        }

        USA.AdCompletionStatus Convert(AdCompletionStatus status) {
            return status switch {
                AdCompletionStatus.Completed => USA.AdCompletionStatus.Completed,
                AdCompletionStatus.Incomplete => USA.AdCompletionStatus.Incomplete,
                AdCompletionStatus.Partial => USA.AdCompletionStatus.Partial,
                _ => throw new ArgumentOutOfRangeException(nameof(status), $"not expecting AdCompletionStatus {status}")
            };
        }

        USA.AdProvider Convert(AdProvider provider) {
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

        USA.AdPlacementType Convert(AdPlacementType placementType) {
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