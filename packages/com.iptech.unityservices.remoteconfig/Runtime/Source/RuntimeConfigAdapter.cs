using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IPTech.Platform;
using Newtonsoft.Json.Linq;
using Unity.Services.RemoteConfig;
using UnityEngine;

namespace IPTech.UnityServices.RemoteConfig
{
    public class RuntimeConfigAdapter : IRuntimeConfig
    {
        private readonly JObject config;
        

        public RuntimeConfigAdapter(RuntimeConfig config) {
            this.config = (JObject)config.config.DeepClone();

            switch(config.origin) {
                case ConfigOrigin.Cached:
                    Source = ConfigSource.Cached;
                    break;
                case ConfigOrigin.Remote:
                    Source = ConfigSource.Remote;
                    break;
                default:
                    Source = ConfigSource.Default;
                    break;
            }
        }

        public ConfigSource Source { get; }

        public object this[string key] {
            get {
                if(config.ContainsKey(key)) {
                   return config.Value<object>(key);
                }
                throw new KeyNotFoundException($"the key {key} was not found in the collection");
            }
        }

        public IEnumerable<string> Keys => config.Properties().Select(prop => prop.Name).ToArray<string>();

        public IEnumerable<object> Values => config.Values<object>().ToList();

        public int Count => config.Count;

        public bool ContainsKey(string key) => config.ContainsKey(key);
       
        
        public bool TryGetValue(string key, out object value) {
            if(ContainsKey(key)) {
                value = config.Value<object>(key);
                return true;
            }
            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            foreach(var key in Keys) {
                yield return new KeyValuePair<string, object>(key, config.Value<object>(key));
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
