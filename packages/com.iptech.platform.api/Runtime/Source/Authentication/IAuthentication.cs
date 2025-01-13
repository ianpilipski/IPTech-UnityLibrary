using System;
using System.Threading;
using System.Threading.Tasks;

namespace IPTech.Platform {
    public interface IAuthentication {
        bool IsSignedIn { get; }
        string PlayerName { get; }
        string PlayerId { get; }

        event Action SignInChanged;

        Task SignedInAnonymously(CancellationToken ct=default);
    }
}