using System;
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
            this.config = config.config;
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
        
        public T Get<T>(string key, T defaultValue = default) {
            try {
                if(config.ContainsKey(key)) {
                    var t = typeof(T);
                    if(t.IsPrimitive || t == typeof(string)) {
                        return config[key].Value<T>();
                    } else {
                        return config[key].ToObject<T>();
                    }
                }
            } catch(Exception e) {
                Debug.LogException(e);
            }
            return defaultValue;
        }

        public object this[string key] {
            get {
                if(config.ContainsKey(key)) {
                   return config[key];
                }
                throw new KeyNotFoundException($"the key {key} was not found in the collection");
            }
        }

        public IEnumerable<string> Keys => config.Properties().Select(prop => prop.Name);

        public IEnumerable<object> Values => config.Values<object>();

        public int Count => config.Count;

        public bool ContainsKey(string key) => config.ContainsKey(key);
       
        
        public bool TryGetValue(string key, out object value) {
            if(ContainsKey(key)) {
                value = config[key];
                return true;
            }
            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
            foreach(var key in Keys) {
                yield return new KeyValuePair<string, object>(key, config[key]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
