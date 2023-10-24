using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Platform.Internal {
    public class IPTechPlatformMB : MonoBehaviour {
        public event Action OnUpdate;

        void Update() {
            OnUpdate?.Invoke();
        }
    }
}
