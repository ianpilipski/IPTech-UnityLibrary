using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.Platform
{
    public interface IAdsManager
    {
        Task<ShowAdResult> ShowAd(AdType type, string placementName = null);
        void ShowDebugger();
    }
}
