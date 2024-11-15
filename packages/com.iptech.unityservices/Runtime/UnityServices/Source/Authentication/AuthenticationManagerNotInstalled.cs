using System.Threading.Tasks;
using IPTech.Platform;

namespace IPTech.UnityServices.Authentication {
    public class AuthenticationManagerNotInstalled : IAuthentication {
        const string INSTALLMSG = "you need to install unity Authorization services to work with this api";

        public bool IsInstalled => false;
        public string PlayerId => throw new System.NotImplementedException(INSTALLMSG);
        public string PlayerName => throw new System.NotImplementedException(INSTALLMSG);
        public bool IsSignedIn => throw new System.NotImplementedException(INSTALLMSG);

        public Task EnsureSignedInAnonymously() {
            throw new System.NotImplementedException(INSTALLMSG);
        }
    }
}
