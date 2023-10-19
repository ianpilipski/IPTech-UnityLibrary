
using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using System.Collections.Generic;

namespace IPTech.UnityServices {
    #if IPTECH_UNITYANALYTICS_INSTALLED
    using Analytics;
    #endif

    using UEServices = global::Unity.Services.Core.UnityServices;

    public class IPTechUnityServices : IIPTechUnityServices {
        const string CONSENT_KEY = "_iptech_unityservices_consentvalue";

        bool alreadyCalledInitialize;
        readonly List<Action<EConsentValue>> consentListeners = new();
        readonly List<Action> initializedListeners = new();

        #if IPTECH_UNITYANALYTICS_INSTALLED
        public readonly AnalyticsManager Analytics;
        #endif
        
        readonly INetworkDetector networkDetector;

        public string EnvironmentID {
            get {
                if(Debug.isDebugBuild) {
                    return "development";
                }
                return "production";
            }
        }

        public EState State { get; private set; }

        public IPTechUnityServices() : this(new NetworkDetector(10)) {}
        private IPTechUnityServices(INetworkDetector networkDetector) {
            State = EState.Initializing;
            this.networkDetector = networkDetector;
            
            #if IPTECH_UNITYANALYTICS_INSTALLED
            this.Analytics = new AnalyticsManager(this);
            #endif
        }

        public async Task Initialize() {
            if(alreadyCalledInitialize) return;
            alreadyCalledInitialize = true;

            try {
                await WaitUntilOnline();
                State = await InitializeUnityServices();
            } catch(Exception e) {
                Debug.LogException(e);
                State = EState.NotOnline;
            }

            foreach(var l in initializedListeners) {
                SafeInvoke(l);
            }
        }

        async Task WaitUntilOnline() {
            var online = networkDetector.State == ENetworkState.Reachable;
            if(online) {
                return;
            }

            DateTime timeoutTime = DateTime.Now.AddMinutes(5);
            networkDetector.NetworkStateChanged += HandleNetworkStateChanged;
            while(!online) {
                await Task.Yield();
                if(DateTime.Now > timeoutTime) {
                    throw new OperationCanceledException("timed out waiting to go online");
                }
                if(!Application.isPlaying) {
                    throw new OperationCanceledException("editor left playmode");
                }
            }
            networkDetector.NetworkStateChanged -= HandleNetworkStateChanged;

            void HandleNetworkStateChanged(ENetworkState newState) {
                online = newState == ENetworkState.Reachable;
            }
        }

        async Task<EState> InitializeUnityServices() {
            if(Application.isPlaying) {
                try {
                    var options = new InitializationOptions();

                    options.SetEnvironmentName(EnvironmentID);
                    await UEServices.InitializeAsync(options);

                    #if UNITY_AUTHSERVICE_INSTALLED
                    // remote config requires authentication for managing environment information
                    if(!AuthenticationService.Instance.IsSignedIn) {
                        await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    }
                    #endif

                    return EState.Online;
                } catch(Exception e) {
                    Debug.LogException(e);
                }
            }
            return EState.NotOnline;
        }

        public bool IsInitialized => State != EState.Initializing;

        public EConsentValue Consent {
            get {
                return (EConsentValue)PlayerPrefs.GetInt(CONSENT_KEY, (int)EConsentValue.Unknown);
            }

            set {
                if(value != Consent) {
                    PlayerPrefs.SetInt(CONSENT_KEY, (int)value);
                    PlayerPrefs.Save();
                    foreach(var l in consentListeners) {
                        SafeInvoke(l, value);
                    }
                }

                void SafeInvoke(Action<EConsentValue> action, EConsentValue value) {
                    try {
                        action(value);
                    } catch(Exception e) {
                        Debug.LogException(e);
                    }
                }
            }
        }

        public event Action<EConsentValue> ConsentValueChanged {
            add {
                consentListeners.Add(value);
            }
            remove {
                consentListeners.Remove(value);
            }
        }

        public event Action Initialized {
            add {
                initializedListeners.Add(value);
                if(IsInitialized) {
                    value.Invoke();
                }
            }
            remove {
                initializedListeners.Remove(value);
            }
        }

        void SafeInvoke(Action action) {
            try {
                action();
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }
    }
}