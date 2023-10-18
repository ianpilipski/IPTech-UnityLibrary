using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.UnityServices {
    public interface INetworkDetector {
        ENetworkState State { get; }
        event Action<ENetworkState> NetworkStateChanged;
    }

    public enum ENetworkState {
        Reachable,
        NotReachable
    }

    public class NetworkDetector : INetworkDetector {
        int secondsBetweenChecks;

        public event Action<ENetworkState> NetworkStateChanged;

        public ENetworkState State { get; private set; }

        public NetworkDetector(int secondsBetweenChecks) {
            this.secondsBetweenChecks = secondsBetweenChecks;
            UpdateState();
            StartUpdateMonitor();
        }

        void UpdateState() {
            var newState = Application.internetReachability == NetworkReachability.NotReachable ? ENetworkState.NotReachable : ENetworkState.Reachable;
            bool modified = newState != State;
            if(modified) {
                State = newState;
                try {
                    NetworkStateChanged?.Invoke(State);
                } catch(Exception e) {
                    Debug.LogException(e);
                }
            }
        }

        async void StartUpdateMonitor() {
            try {
                float nextCheck = Time.realtimeSinceStartup + secondsBetweenChecks;
                while(true) {
                    await Task.Yield();

                    if(!Application.isPlaying) return;

                    if(Time.realtimeSinceStartup > nextCheck) {
                        UpdateState();
                        nextCheck = Time.realtimeSinceStartup + secondsBetweenChecks;
                    }
                }
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }

    }
}