using System;
using UnityEngine;

namespace IPTech.UnityServices.Internal {
    public class ApplicationEvents : MonoBehaviour
    {
        public event Action<bool> ApplicationPaused;

        void OnApplicationPause(bool paused) {
            ApplicationPaused?.Invoke(paused);    
        }
    }
}
