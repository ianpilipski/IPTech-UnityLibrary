using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Platform {
    public interface IRuntimeConfig : IReadOnlyDictionary<string, object> {
        ConfigSource Source { get; }
        T Get<T>(string key, T defaultValue = default);
    }

    public enum ConfigSource {
        Default,
        Cached,
        Remote
    }
}
