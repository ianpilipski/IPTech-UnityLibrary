using System;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;

namespace IPTech.UnityServices {
    using Platform;

    public class AnalyticsManager  {
        IAnalyticsService service;
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
    }
}