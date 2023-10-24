using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.Platform {    
    using Internal;

    public class IPTechPlatform : IIPTechPlatform {
        static int unityThreadId;
        static IPTechPlatformMB mbInst;

        readonly INetworkDetector networkDetector;
        readonly IConsentHandler consentHandler;

        List<Action> queudActions = new();
        public event Action Initialized;
        public event Action<bool> ApplicationPaused;

        public IPTechPlatform() {
            consentHandler = new ConsentHandler();
            networkDetector = new NetworkDetector(this, 30);

            HookMbInst();
        }

        public INetworkDetector Network => networkDetector;

        public EServiceState State { get; private set; } //TODO: implement state with init

        public ConsentInfo Consent {
            get => consentHandler.Consent;
            set => consentHandler.Consent = value;
        }

        public event Action<ConsentInfo> ConsentValueChanged {
            add => consentHandler.ConsentValueChanged += value;
            remove => consentHandler.ConsentValueChanged -= value;
        }

        async void HookMbInst() {
            try {
                await WaitForMBInst();
                mbInst.OnUpdate += HandleOnUpdate;
                mbInst.OnAppPaused += HandleAppPaused;

                Initialized?.Invoke(); //TODO: fix this.. init will be based on the integrated services...
            } catch(OperationCanceledException) {
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RuntimeInitializeOnLoad() {
            unityThreadId = Thread.CurrentThread.ManagedThreadId;
            CreatePlatformGameObject();
        }

        static void CreatePlatformGameObject() {
            var go = new GameObject("IPTechPlatformMB");
            go.hideFlags = HideFlags.HideAndDontSave;
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