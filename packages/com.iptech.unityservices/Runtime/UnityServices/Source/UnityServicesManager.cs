using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using System.Collections.Generic;

namespace IPTech.UnityServices {
    using Internal;
    using Authentication;
    using Platform;

    using UEServices = global::Unity.Services.Core.UnityServices;

    public class UnityServicesManager : IUnityServicesManager {
        const string CONSENT_KEY = "_iptech_unityservices_consentvalue";

        bool alreadyCalledInitialize;
        EOnlineState onlineState;
        readonly List<Action<ConsentInfo>> consentListeners = new();
        readonly List<Action<EServiceState>> initializedListeners = new();
        readonly IIPTechPlatform platform;
        readonly AuthenticationManager authenticationManager;

        public event Action<EOnlineState> OnlineStateChanged;

        public IAuthentication Authentication => authenticationManager;        
        public IAnalyticsManager AnalyticsManager { get; }

#if IPTECH_UNITYADVERTISING_INSTALLED
        public AdsManager AdsManager { get; }
#endif

        public ILeaderboardsManager LeaderboardsManager { get;}

        public UnityServicesManager(IIPTechPlatform platform) {
            State = EServiceState.Initializing;
            onlineState = EOnlineState.Offline;

            this.platform = platform;
            this.authenticationManager = new AuthenticationManager(this);

#if IPTECH_UNITYANALYTICS_INSTALLED
            this.AnalyticsManager = new AnalyticsManager(this);
#endif

#if IPTECH_UNITYADVERTISING_INSTALLED
            this.AdsManager = new AdsManager(platform);
#endif

#if IPTECH_UNITYLEADERBOARDS_INSTALLED
            this.LeaderboardsManager = new IPTech.UnityServices.Leaderboards.LeaderboardsManager(this);
#endif

            var go = new GameObject("IPTechUnityServicesApplicationEvents");
            var appEvents =  go.AddComponent<ApplicationEvents>();
            go.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(go);
            Initialize();
        }

        public EOnlineState OnlineState {
            get {
                return onlineState;
            }
            private set {
                if(value != onlineState) {
                    onlineState = value;
                    OnOnlineStateChanged();
                }
                return;

                void OnOnlineStateChanged() {
                    OnlineStateChanged?.Invoke(onlineState);
                }
            }
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

        async void Initialize() {
            try {
                if(alreadyCalledInitialize) return;
                alreadyCalledInitialize = true;

                try {
                    await WaitUntilOnline();
                    State = await InitializeUnityServices();
                    if(State == EServiceState.Initialized) {
                        if(platform.Network.State == ENetworkState.Reachable) {
                            OnlineState = EOnlineState.Online;
                        }
                    }
                    platform.Network.NetworkStateChanged += HandleNetworkStateChanged;
                } catch(Exception e) {
                    if(!(e is OperationCanceledException)) {
                        Debug.LogException(e);
                    }
                    State = EServiceState.FailedToInitialize;
                }

                foreach(var l in initializedListeners) {
                    try {
                        l.Invoke(State);
                    } catch(Exception e) {
                        Debug.LogException(e);
                    }
                }

                if(OnlineState == EOnlineState.Online) {
                    if(authenticationManager.IsInstalled) {
                        await authenticationManager.EnsureSignedInAnonymously();
                    }
                }
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }

        private void HandleNetworkStateChanged(ENetworkState state) {
            OnlineState = state == ENetworkState.Reachable ? EOnlineState.Online : EOnlineState.Offline;
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

                    return EServiceState.Initialized;
                } catch(Exception e) {
                    Debug.LogException(e);
                }
            }
            return EServiceState.FailedToInitialize;
        }

        public bool IsInitialized => State != EServiceState.Initializing;

        public event Action<EServiceState> Initialized {
            add {
                initializedListeners.Add(value);
                if(IsInitialized) {
                    value.Invoke(State);
                }
            }
            remove {
                initializedListeners.Remove(value);
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