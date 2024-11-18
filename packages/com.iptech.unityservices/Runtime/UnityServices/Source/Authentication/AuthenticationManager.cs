#if UNITY_AUTHSERVICE_INSTALLED || UNITY_LEADERBOARDS_INSTALLED || UNITY_REMOTECONFIG_INSTALLED
#if UNITY_AUTHSERVICE_INSTALLED

using System;
using System.Threading.Tasks;
using IPTech.Platform;
using Unity.Services.Authentication;
using UnityEngine;

namespace IPTech.UnityServices.Authentication {
    public class AuthenticationManager : IAuthentication {
        private readonly IUnityServicesManager _unityServicesManager;
        private IAuthenticationService _instance;
        private bool _isInitialized;

        public AuthenticationManager(IUnityServicesManager unityServicesManager) {
            _unityServicesManager = unityServicesManager;
            if(_unityServicesManager.State == EServiceState.Initializing) {
                _unityServicesManager.Initialized += HandleInitialized;
                return;
            }
            HandleInitialized();
        }

        public bool IsInstalled => true;
        public string PlayerId => _instance.PlayerId;
        public string PlayerName => _instance.PlayerName;
        public bool IsSignedIn => _instance?.IsSignedIn == true;
        public event Action SignInChanged;

        public async Task EnsureSignedInAnonymously() {
            await WaitUntilInitialized();
            if(_instance == null) return;
            if(!_instance.IsSignedIn) {
                await _instance.SignInAnonymouslyAsync();
                OnSignInChanged();
            }
        }

        private void HandleInitialized() {
            _isInitialized = true;
            if(_unityServicesManager.State == EServiceState.Initialized) {
                _instance = Unity.Services.Authentication.AuthenticationService.Instance;
            }
        }

        async Task WaitUntilInitialized() {
            while(!_isInitialized) {
                await Task.Yield();
                if(!Application.isPlaying) throw new OperationCanceledException("unity exited playmode");
            }
        }

        void OnSignInChanged() {
            try {
                SignInChanged.Invoke();
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }
    }
}
#else
namespace IPTech.UnityServices.Authentication {
    public class AuthenticationManager : AuthenticationManagerNotInstalled {
        public AuthenticationManager(IUnityServicesManager unityServicesManager) { }
    }
}
#endif
#endif

