using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.Platform {
    
    using Internal;

    public interface INetworkDetector {
        ENetworkState State { get; }
        event Action<ENetworkState> NetworkStateChanged;
    }

    public enum ENetworkState {
        Reachable,
        NotReachable
    }

    public interface IIPTechPlatform {
        INetworkDetector Network { get; }
        void RunOnUnityThread(Action action);
    }

    public class IPTechPlatform : IIPTechPlatform {
        static int unityThreadId;
        static IPTechPlatformMB mbInst;

        readonly INetworkDetector networkDetector;

        List<Action> queudActions = new();

        public IPTechPlatform() {
            HookMbInst();

            networkDetector = new NetworkDetector(this, 30);
        }

        public INetworkDetector Network => networkDetector;

        async void HookMbInst() {
            try {
                await WaitForMBInst();
                mbInst.OnUpdate += HandleOnUpdate;
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