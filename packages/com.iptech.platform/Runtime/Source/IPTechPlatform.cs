using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.Platform {    
    using Internal;
    using Utils;

    public class IPTechPlatform : IIPTechPlatform {
        static int unityThreadId;
        static IPTechPlatformMB mbInst;

        readonly INetworkDetector networkDetector;
        readonly IConsentHandler consentHandler;
        readonly IServiceContext serviceContext;

        List<Action> queudActions = new();
        public event Action Initialized;
        public event Action<bool> ApplicationPaused;

        [Obsolete("use the async constructor for createContext instead")]
        public IPTechPlatform(Action<IServiceContext> createContext) : this((sc) => { createContext(sc); return Task.CompletedTask; }) {
        }

        [Obsolete("use constructure with configuration instead")]
        public IPTechPlatform(Func<IServiceContext, Task> createContext) {
            consentHandler = new ConsentHandler();
            networkDetector = new NetworkDetector(this, 30);
            serviceContext = new ServiceContext();
            serviceContext.AddService<IIPTechPlatform>(this);

            Initialize(createContext);
        }

        public IPTechPlatform(IIPTechPlatformConfig config) {
            serviceContext = new ServiceContext();
            consentHandler = new ConsentHandler();
            networkDetector = new NetworkDetector(this, 30); // TODO: make this configurable
            serviceContext.AddService<IIPTechPlatform>(this);
            Initialize(config);
        }

        public INetworkDetector Network => networkDetector;
        public IServiceLocator Services => serviceContext;

        public IAnalyticsManager Analytics => GetServiceWithConfigurationErrorMessage<IAnalyticsManager>();
        public ILeaderboardsManager Leaderboards => GetServiceWithConfigurationErrorMessage<ILeaderboardsManager>();
        public IAuthentication Authentication => GetServiceWithConfigurationErrorMessage<IAuthentication>();

        public EServiceState State { get; private set; } //TODO: implement state with init

        public ConsentInfo Consent {
            get => consentHandler.Consent;
            set => consentHandler.Consent = value;
        }

        public event Action<ConsentInfo> ConsentValueChanged {
            add => consentHandler.ConsentValueChanged += value;
            remove => consentHandler.ConsentValueChanged -= value;
        }

        async void Initialize(IIPTechPlatformConfig config) {
            try {
                await HookMbInst();
                foreach(var factory in config.Factories) {
                    serviceContext.AddService(factory.CreatesType, new ServiceCreatorCallback((l, t) => {
                        return factory.Create(this);
                    }));
                }
                State = EServiceState.Initialized;
            } catch(Exception e) {
                State = EServiceState.FailedToInitialize;
                Debug.LogException(e);
            } finally {
                OnInitialized();
            }
        }
       
        [Obsolete("use the configuration to create the service context")]
        async void Initialize(Func<IServiceContext, Task> createContext) {
            try {
                Debug.Log("Initializing IPTechPlatform .. ");
                Debug.Log("[IPTechPlatform] waiting for mono behaviour ..");
                await HookMbInst();
                Debug.Log("[IPTechPlatform] found mono behaviour");
                Debug.Log("[IPTechPlatform] Creating Context .. ");
                await createContext(serviceContext);
                Debug.Log("[IPTechPlatform] Context Created");
                State = EServiceState.Initialized;
            } catch(OperationCanceledException) {
                State = EServiceState.FailedToInitialize;
            } catch(Exception e) {
                State = EServiceState.FailedToInitialize;
                Debug.LogException(e);
            } finally {
                OnInitialized();
            }
        }

        void OnInitialized() {
            if(Initialized!=null) {
                foreach(var del in Initialized.GetInvocationList()) {
                    try {
                        del.DynamicInvoke();
                    } catch(Exception e) {
                        Debug.LogException(e);
                    }
                }
            }
        }

        async Task HookMbInst() {
            await WaitForMBInst();
            mbInst.OnUpdate += HandleOnUpdate;
            mbInst.OnAppPaused += HandleAppPaused;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RuntimeInitializeOnLoad() {
            unityThreadId = Thread.CurrentThread.ManagedThreadId;
            CreatePlatformGameObject();
        }

        static void CreatePlatformGameObject() {
            var go = new GameObject("IPTechPlatformMB");
            go.hideFlags = HideFlags.DontSave;
            GameObject.DontDestroyOnLoad(go);
            mbInst = go.AddComponent<IPTechPlatformMB>();
        }

        async Task WaitForMBInst() {
            while(mbInst == null) {
                await Task.Yield();
                if(!Application.isPlaying) throw new OperationCanceledException("playmode state changed");
            }
        }

        void HandleOnUpdate() {
            ExecQueuedActions();
        }

        void HandleAppPaused(bool paused) {
            ApplicationPaused?.Invoke(paused);
        }

        public void RunOnUnityThread(Action action) {
            if(mbInst!=null) {
                if(Thread.CurrentThread.ManagedThreadId == unityThreadId) {
                    action();
                    return;
                }
            }
            lock(queudActions) {
                queudActions.Add(action);
            }
        }

        void ExecQueuedActions() {
            if(queudActions.Count == 0) return;

            List<Action> l = null;
            lock(queudActions) {
                l = new List<Action>(queudActions);
                queudActions.Clear();
            }

            foreach(var a in l) {
                try {
                    a.Invoke();
                } catch(Exception e) {
                    Debug.LogException(e);
                }
            }
        }

        T GetServiceWithConfigurationErrorMessage<T>() {
            if(serviceContext.HasService<T>()) {
                return serviceContext.GetService<T>();
            }

            throw new IPTechExceptions.ServiceNotRegisteredException(typeof(T));
        }
    }
}