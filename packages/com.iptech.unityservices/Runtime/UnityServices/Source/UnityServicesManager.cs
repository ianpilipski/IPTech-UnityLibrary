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

        bool alreadyCalledSignIn;
        bool alreadyCalledInitialize;
        EOnlineState onlineState;
        readonly List<Action<ConsentInfo>> consentListeners = new();
        readonly List<Action<EServiceState>> initializedListeners = new();
        readonly IIPTechPlatform platform;
        readonly AuthenticationManager authenticationManager;

        public event Action<EOnlineState> OnlineStateChanged;

        public INetworkDetector Network => platform.Network;
        public IAuthentication Authentication => authenticationManager;        
        public IAnalyticsManager AnalyticsManager { get; }
        public IRemoteConfigManager RemoteConfigManager { get; }

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

#if IPTECH_UNITYREMOTECONFIG_INSTALLED
            this.RemoteConfigManager = new IPTech.UnityServices.RemoteConfig.RemoteConfigManager(this);
#endif

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

                Debug.Log($"[IPTech.UnityServices] initializing...");
                try {
                    State = await InitializeUnityServices();
                    if(State == EServiceState.Initialized) {
                        if(platform.Network.State == ENetworkState.Reachable) {
                            OnlineState = EOnlineState.Online;
                        }
                    }
                    platform.Network.NetworkStateChanged += HandleNetworkStateChanged;
                } catch(Exception e) {
                    if(e is OperationCanceledException) {
                        Debug.LogWarning($"UnityServices Initialization Cancelled: {e.Message}");
                    } else { 
                        Debug.LogException(e);
                    }
                    State = EServiceState.FailedToInitialize;
                }
                Debug.Log($"[IPTech.UnityServices] initialization completed with status: {State}");

                foreach(var l in initializedListeners) {
                    try {
                        l.Invoke(State);
                    } catch(Exception e) {
                        Debug.LogException(e);
                    }
                }

                EnsureSignedIn();
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }

        private void HandleNetworkStateChanged(ENetworkState state) {
            OnlineState = state == ENetworkState.Reachable ? EOnlineState.Online : EOnlineState.Offline;
            EnsureSignedIn();
        }

        async void EnsureSignedIn() {
            try {
                if(alreadyCalledSignIn) return;
                if(OnlineState == EOnlineState.Online) {
                    if(authenticationManager.IsInstalled && !authenticationManager.IsSignedIn) {
                        alreadyCalledSignIn = true;
                        await authenticationManager.SignedInAnonymously();
                    }
                }
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }

        async Task<EServiceState> InitializeUnityServices() {
            if(Application.isPlaying) {
                try {
                    var options = new InitializationOptions();

                    options.SetEnvironmentName(EnvironmentID);
                    Debug.Log($"[IPTech.UnityServices] intializing unity services for environment = {EnvironmentID}");
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