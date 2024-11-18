using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Platform {
    public interface IIPTechPlatformConfig  {
        IEnumerable<IIPTechServiceFactory> Factories { get; }
        void RegisterFactory(IIPTechServiceFactory factory);
        void RegisterFactory<T>(IIPTechServiceFactory<T> factory);
    }
}
