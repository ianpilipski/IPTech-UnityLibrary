using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.UnityServices.Internal {
    public class InterstitialAd : AdBase
    {
        public InterstitialAd(string placementName) : base(placementName) {
        }

        override protected void LoadAd() {
            #if IPTECH_IRONSOURCE_INSTALLED
            IronSource.Agent.loadInterstitial();
            #endif
        }

        override protected async Task ShowAd() {
            #if IPTECH_IRONSOURCE_INSTALLED
            if (IronSource.Agent.isInterstitialReady()) {
                IronSource.Agent.showInterstitial(result.PlacementID);
                await WaitWhile(() => showState != ShowState.FinishedShowing );
            } else {
                result.AdResult = AdResult.FaildToLoad;
            }
            #endif
        }

#if IPTECH_IRONSOURCE_INSTALLED
        #region AdInfo Interstitial
        // Invoked when the interstitial ad was loaded succesfully.
        override protected void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo) {
            result.Info = adInfo;
            loadState = LoadState.Loaded;
        }

        // Invoked when the initialization process has failed.
        override protected void InterstitialOnAdLoadFailed(IronSourceError ironSourceError) {
            loadState = LoadState.FailedToLoad;
            result.AdResult = AdResult.FaildToLoad;
        }

        // Invoked when the Interstitial Ad Unit has opened. This is the impression indication. 
        override protected void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo) {
            result.Info = adInfo;
        }

        // Invoked when end user clicked on the interstitial ad
        override protected void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo) {
            result.Info = adInfo;
            result.UserClicked = true;
        }

        // Invoked before the interstitial ad was opened, and before the InterstitialOnAdOpenedEvent is reported.
        // This callback is not supported by all networks, and we recommend using it only if  
        // it's supported by all networks you included in your build. 
        override protected void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo adInfo) {
            result.Info = adInfo;
        }

        // Invoked when the ad failed to show.
        override protected void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo) {
            result.Info = adInfo;
            result.AdResult = AdResult.FailedToShow;
            showState = ShowState.FinishedShowing;
        }

        // Invoked when the interstitial ad closed and the user went back to the application screen.
        override protected void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo) {
            result.Info = adInfo;
            result.AdResult = AdResult.Watched;
            showState = ShowState.FinishedShowing;
        }
        #endregion
#endif
    }
}
