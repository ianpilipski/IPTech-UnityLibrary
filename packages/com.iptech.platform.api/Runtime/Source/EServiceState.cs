using System;

namespace IPTech.Platform
{
    public enum EServiceState {
        Initializing,
        FailedToInitialize,
        Initialized,
        [Obsolete("use FailedToInitialize")] NotOnline = FailedToInitialize,
        [Obsolete("use Initialized")] Online = Initialized
    }
}
