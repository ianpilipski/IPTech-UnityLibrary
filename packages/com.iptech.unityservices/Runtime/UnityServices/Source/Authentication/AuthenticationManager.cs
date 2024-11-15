#if UNITY_AUTHSERVICE_INSTALLED || UNITY_LEADERBOARDS_INSTALLED || UNITY_REMOTECONFIG_INSTALLED
#if UNITY_AUTHSERVICE_INSTALLED

using System.Threading.Tasks;
using IPTech.Platform;
using Unity.Services.Authentication;

namespace IPTech.UnityServices.Authentication {
    public class AuthenticationManager : IAuthentication {
        private IAuthenticationService _instance;

        public AuthenticationManager() {
            _instance = Unity.Services.Authentication.AuthenticationService.Instance;
        }

        public bool IsInstalled => true;
        public string PlayerId => _instance.PlayerId;
        public string PlayerName => _instance.PlayerName;
        public bool IsSignedIn => _instance.IsSignedIn;

        public async Task EnsureSignedInAnonymously() {
            if(!_instance.IsSignedIn) {
                await _instance.SignInAnonymouslyAsync();
            }
        }
    }
}
#else
#error you need to install unity Authorization services to work with one of the packages you have installed
namespace IPTech.UnityServices.Authentication {
    public class AuthenticationManager : AuthenticationManagerNotInstalled {
    }
}
#endif
#endif

