#if UNITY_AUTHSERVICE_INSTALLED || UNITY_LEADERBOARDS_INSTALLED || UNITY_REMOTECONFIG_INSTALLED
#if UNITY_AUTHSERVICE_INSTALLED

using System;
using System.Threading;
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
            if(_unityServicesManager.State == EServiceState.Initialized) {
                _instance = AuthenticationService.Instance;
                return;
            }
            _unityServicesManager.Initialized += s => _instance = s == EServiceState.Initialized ? AuthenticationService.Instance : null;
        }

        public bool IsInstalled => true;
        public string PlayerId => _instance?.PlayerId;
        public string PlayerName => _instance?.PlayerName;
        public bool IsSignedIn => _instance?.IsSignedIn == true;
        public event Action SignInChanged;
        private Task _signInTask;

        [Obsolete("Use SignInAnonymously instead")]
        public Task SignedInAnonymously(CancellationToken ct = default)
        {
            return SignInAnonymously(ct);
        }
        
        public async Task SignInAnonymously(CancellationToken ct = default)
        {
            AssertIsOnline();
            if (!_instance.IsSignedIn)
            {
                if (_signInTask != null && !_signInTask.IsCompleted)
                {
                    Debug.LogWarning("[IPTech.UnityServices] sign in already in progress, waiting for completion...");
                    await _signInTask;
                    return;
                }
                _signInTask = PerformSignIn();
            }

            async Task PerformSignIn()
            {
                Debug.Log("[IPTech.UnityServices] attempting anonymous authentication with unity services...");
                await _instance.SignInAnonymouslyAsync();
                await _instance.GetPlayerNameAsync();
                OnSignInChanged();
            }
        }

        void OnSignInChanged() {
            try {
                Debug.Log($"[IPTech.UnityServices] unity services sign in changed (PlayerId={PlayerId})");
                SignInChanged?.Invoke();
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }

        void AssertIsOnline() {
            if(_unityServicesManager.State != EServiceState.Initialized &&
                _unityServicesManager.OnlineState != EOnlineState.Online && _instance!=null)
                throw new InvalidOperationException("UnityServices must be initialized an online to call this api");
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

