using System;

namespace IPTech.Platform {
    public class ApiUnavailableException : Exception {
        public ApiUnavailableException(string name) : base($"The {name} api is unavailable") { }
    }
}
