using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Platform {
    public static class IPTechExceptions {
        public static void AssertServiceInitialized(EServiceState state, string serviceName) {
            if(state == EServiceState.Initialized) return;
            if(state == EServiceState.FailedToInitialize) throw new ServiceFailedToInitializeException(serviceName);
            throw new ServiceNotYetInitializedException(serviceName);
        }

        public class ServiceFailedToInitializeException : InvalidOperationException {
            public ServiceFailedToInitializeException(string serviceName)
                : base($"The service {serviceName} failed to initialize and the api can not be used.") { }
        }

        public class ServiceNotYetInitializedException : InvalidOperationException {
            public ServiceNotYetInitializedException(string serviceName)
                : base($"The service {serviceName} has not finished initializing, the api can not be called.") { }
        }

        public class ServiceNotRegisteredException : InvalidOperationException {
            public ServiceNotRegisteredException(Type t) : base($"The service {t.Name} has not been registered with the iptech config, please install a package dependency or register the required service") { }
        }
    }
}
