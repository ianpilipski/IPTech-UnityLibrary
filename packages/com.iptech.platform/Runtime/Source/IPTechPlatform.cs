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

        public IPTechPlatform(Action<IServiceContext> createContext) {
            consentHandler = new ConsentHandler();
            networkDetector = new NetworkDetector(this, 30);
            serviceContext = new ServiceContext();
            serviceContext.AddService<IIPTechPlatform>(this);

            Initialize(createContext);
        }

        public INetworkDetector Network => networkDetector;
        public IServiceLocator Services => serviceContext;

        public EServiceState State { get; private set; } //TODO: implement state with init

        public ConsentInfo Consent {
            get => consentHandler.Consent;
            set => consentHandler.Consent = value;
        }

        public event Action<ConsentInfo> ConsentValueChanged {
            add => consentHandler.ConsentValueChanged += value;
            remove => consentHandler.ConsentValueChanged -= value;
        }

        async void Initialize(Action<IServiceContext> createContext) {
            try {
                Debug.Log("Initializing IPTechPlatform .. ");
                Debug.Log("[IPTechPlatform] waiting for mono behaviour ..");
                await HookMbInst();
                Debug.Log("[IPTechPlatform] found mono behaviour");
                Debug.Log("[IPTechPlatform] Creating Context .. ");
                createContext(serviceContext);
                Debug.Log("[IPTechPlatform] Context Created");
                State = EServiceState.Online;
            } catch(OperationCanceledException) {
                State = EServiceState.NotOnline;
            } catch(Exception e) {
                State = EServiceState.NotOnline;
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
    }
}