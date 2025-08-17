using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.Platform
{
    public interface IAdsManager
    {
        Task<FillAdResult> FillAd(AdType adType, string adUnitId);
        bool IsAdFilled(AdType adType, string adUnitId);
        Task<ShowAdResult> ShowAd(AdType type, string adUnitId, string placementName = null);
        void ShowDebugger();
    }
}
