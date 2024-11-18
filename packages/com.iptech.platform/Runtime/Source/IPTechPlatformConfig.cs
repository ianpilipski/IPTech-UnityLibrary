using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Platform {
    public class IPTechPlatformConfig : IIPTechPlatformConfig {
        private List<IIPTechServiceFactory> factories = new();

        public IEnumerable<IIPTechServiceFactory> Factories => factories;

        public void RegisterFactory(IIPTechServiceFactory factory) {
            factories.Add(factory);
        }

        public void RegisterFactory<T>(IIPTechServiceFactory<T> factory) {
            factories.Add(factory);
        }
    }
}
