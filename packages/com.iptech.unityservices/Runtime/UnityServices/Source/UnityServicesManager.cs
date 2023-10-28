
using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using System.Collections.Generic;

namespace IPTech.UnityServices {
    using Internal;
    using Platform;

    using UEServices = global::Unity.Services.Core.UnityServices;

    public class UnityServicesManager : IUnityServicesManager {
        const string CONSENT_KEY = "_iptech_unityservices_consentvalue";

        bool alreadyCalledInitialize;
        readonly List<Action<ConsentInfo>> consentListeners = new();
        readonly List<Action> initializedListeners = new();
        readonly IIPTechPlatform platform;
        
        #if IPTECH_UNITYANALYTICS_INSTALLED
        public readonly AnalyticsManager AnalyticsManager;
        #endif
        #if IPTECH_UNITYADVERTISING_INSTALLED
        public readonly AdsManager AdsManager;
        #endif
        
        public UnityServicesManager(IIPTechPlatform platform) {
            State = EServiceState.Initializing;
            this.platform = platform;
            
            #if IPTECH_UNITYANALYTICS_INSTALLED
            //services.Add(typeof(IAnalyticsManager), new AnalyticsManager(this));
            this.AnalyticsManager = new AnalyticsManager(this);
            #endif

            #if IPTECH_UNITYADVERTISING_INSTALLED
            this.AdsManager = new AdsManager(platform);
            #endif

            var go = new GameObject("IPTechUnityServicesApplicationEvents");
            var appEvents =  go.AddComponent<ApplicationEvents>();
            go.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(go);
        }

        public string EnvironmentID {
            get {
                if(Debug.isDebugBuild) {
                    return "development";
                }
                return "production";
            }
        }

        public EServiceState State { get; private set; }

        public async Task Initialize() {
            if(alreadyCalledInitialize) return;
            alreadyCalledInitialize = true;

            try {
                await WaitUntilOnline();
                State = await InitializeUnityServices();
            } catch(Exception e) {
                Debug.LogException(e);
                State = EServiceState.NotOnline;
            }

            foreach(var l in initializedListeners) {
                SafeInvoke(l);
            }
        }

        async Task WaitUntilOnline() {
            var networkDetector = platform.Network;
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

        async Task<EServiceState> InitializeUnityServices() {
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

                    return EServiceState.Online;
                } catch(Exception e) {
                    Debug.LogException(e);
                }
            }
            return EServiceState.NotOnline;
        }

        public bool IsInitialized => State != EServiceState.Initializing;

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


        public ConsentInfo Consent {
            get => platform.Consent;
            set => platform.Consent = value;
        }

        public event Action<bool> ApplicationPaused {
            add => platform.ApplicationPaused += value;
            remove => platform.ApplicationPaused -= value;
        }

        public event Action<ConsentInfo> ConsentValueChanged {
            add => platform.ConsentValueChanged += value;
            remove => platform.ConsentValueChanged -= value;
        }
    }
}