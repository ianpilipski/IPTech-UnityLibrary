using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Platform {
    public interface IRuntimeConfig : IReadOnlyDictionary<string, object> {
        public ConfigSource Source { get; }
    }

    public enum ConfigSource {
        Default,
        Cached,
        Remote
    }
}
