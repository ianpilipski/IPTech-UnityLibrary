
using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using System.Collections.Generic;

namespace IPTech.UnityServices {
    using Internal;

    using UEServices = global::Unity.Services.Core.UnityServices;

    public class UnityServicesManager : IUnityServicesManager {
        const string CONSENT_KEY = "_iptech_unityservices_consentvalue";

        bool alreadyCalledInitialize;
        readonly List<Action<ConsentInfo>> consentListeners = new();
        readonly List<Action> initializedListeners = new();
        readonly INetworkDetector networkDetector;
        readonly Dictionary<Type, object> services = new();
        
        public event Action<bool> ApplicationPaused;

        #if IPTECH_UNITYANALYTICS_INSTALLED
        public readonly AnalyticsManager AnalyticsManager;
        #endif
        #if IPTECH_UNITYADVERTISING_INSTALLED
        public readonly AdsManager AdsManager;
        #endif
        
        public UnityServicesManager() : this(new NetworkDetector(10)) {}
        private UnityServicesManager(INetworkDetector networkDetector) {
            State = EState.Initializing;
            this.networkDetector = networkDetector;
            
            #if IPTECH_UNITYANALYTICS_INSTALLED
            //services.Add(typeof(IAnalyticsManager), new AnalyticsManager(this));
            this.AnalyticsManager = new AnalyticsManager(this);
            #endif

            #if IPTECH_UNITYADVERTISING_INSTALLED
            this.AdsManager = new AdsManager(this);
            #endif

            var go = new GameObject("IPTechUnityServicesApplicationEvents");
            var appEvents =  go.AddComponent<ApplicationEvents>();
            appEvents.ApplicationPaused += HandleApplicationPaused;
            go.hideFlags = HideFlags.HideAndDontSave;
            GameObject.DontDestroyOnLoad(go);
        }

        T GetService<T>() {
            if(services.TryGetValue(typeof(T), out var value)) return (T)value;
            return default;
        }

        void HandleApplicationPaused(bool paused) {
            ApplicationPaused?.Invoke(paused);
        }

        public string EnvironmentID {
            get {
                if(Debug.isDebugBuild) {
                    return "development";
                }
                return "production";
            }
        }

        public EState State { get; private set; }

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

        public ConsentInfo Consent {
            get {
                var s = PlayerPrefs.GetString(CONSENT_KEY, "");
                if(!string.IsNullOrWhiteSpace(s)) {
                    try {
                        return JsonUtility.FromJson<ConsentInfo>(s);
                    } catch(Exception e) {
                        Debug.LogException(e);
                    }
                }
                return new ConsentInfo();
            }

            set {
                if(!value.Equals(Consent)) {
                    PlayerPrefs.SetString(CONSENT_KEY, JsonUtility.ToJson(value));
                    PlayerPrefs.Save();
                    foreach(var l in consentListeners) {
                        SafeInvoke(() => l.Invoke(value));
                    }
                }
            }
        }

        public event Action<ConsentInfo> ConsentValueChanged {
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