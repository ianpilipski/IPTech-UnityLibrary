using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Platform {
    public static class IPTechExceptions {
        public class ServiceNotRegisteredException : Exception {
            public ServiceNotRegisteredException(Type t) : base($"The service {t.Name} has not been registered with the iptech config, please install a package dependency or register the required service") { }
        }
    }
}
