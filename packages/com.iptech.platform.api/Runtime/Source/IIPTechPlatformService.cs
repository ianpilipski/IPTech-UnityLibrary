using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.Platform {
    public interface IIPTechPlatformService {
        EServiceState State { get; }
        void Initialize(IIPTechPlatform platform);
    }
}
