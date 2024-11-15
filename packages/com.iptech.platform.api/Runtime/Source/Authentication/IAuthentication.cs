using System;
namespace IPTech.Platform {
    public interface IAuthentication {
        bool IsSignedIn { get; }
        string PlayerName { get; }
        string PlayerId { get; }
    }
}