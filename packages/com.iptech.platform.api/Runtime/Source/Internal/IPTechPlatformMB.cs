using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Platform.Internal {
    public class IPTechPlatformMB : MonoBehaviour {
        public event Action OnUpdate;
        public event Action<bool> OnAppPaused;

        void Update() {
            OnUpdate?.Invoke();
        }

        private void OnApplicationPause(bool pause) {
            OnAppPaused?.Invoke(pause);
        }
    }
}
