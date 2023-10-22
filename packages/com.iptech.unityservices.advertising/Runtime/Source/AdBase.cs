using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.UnityServices.Internal {
    public abstract class AdBase {
        protected enum LoadState {
            NotLoaded,
            Loaded,
            FailedToLoad
        }

        protected enum ShowState {
            NotShown,
            FinishedShowing
        }

        protected LoadState loadState;
        protected ShowState showState;
        protected ShowAdResult result;

        public AdBase(string placementName) {
            result.PlacementID = placementName;
            HookEvents();
        }

        public async Task<ShowAdResult> Show() {
            if(!await LoadAdInternal()) return result;
            await ShowAd();
            return result;
        }

        private async Task<bool> LoadAdInternal() {
            LoadAd();
            await WaitWhile(() => loadState==LoadState.NotLoaded);
            if(loadState == LoadState.FailedToLoad) {
                result.AdResult = AdResult.FaildToLoad;
            }
            return loadState == LoadState.Loaded;
        }

        protected abstract void LoadAd();
        protected abstract Task ShowAd();
            
        protected async Task WaitWhile(Func<bool> condition) {
            while(condition()) {
                await Task.Yield();
                CheckPlayerState();
            }
        }

        protected void CheckPlayerState() {
            if(Application.isPlaying) return;
            throw new OperationCanceledException("playmode state changed");
        }

#if IPTECH_IRONSOURCE_INSTALLED
        void HookEvents() {
            IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
            IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
            IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
            IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
            IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialOnAdShowSucceededEvent;
            IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
            IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;

            IronSourceRewardedVideoEvents.onAdOpenedEvent += ReardedVideoOnAdOpenedEvent;
            IronSourceRewardedVideoEvents.onAdClosedEvent += ReardedVideoOnAdClosedEvent;
            IronSourceRewardedVideoEvents.onAdAvailableEvent += ReardedVideoOnAdAvailable;
            IronSourceRewardedVideoEvents.onAdUnavailableEvent += ReardedVideoOnAdUnavailable;
            IronSourceRewardedVideoEvents.onAdShowFailedEvent += ReardedVideoOnAdShowFailedEvent;
            IronSourceRewardedVideoEvents.onAdRewardedEvent += ReardedVideoOnAdRewardedEvent;
            IronSourceRewardedVideoEvents.onAdClickedEvent += ReardedVideoOnAdClickedEvent;
        }


        #region AdInfo Reward
        protected virtual void ReardedVideoOnAdOpenedEvent(IronSourceAdInfo info) {}

        protected virtual void ReardedVideoOnAdClosedEvent(IronSourceAdInfo info) {}

        protected virtual void ReardedVideoOnAdAvailable(IronSourceAdInfo info) {}

        protected virtual void ReardedVideoOnAdUnavailable() {}

        protected virtual void ReardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo info) {}

        protected virtual void ReardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo info) {}

        protected virtual void ReardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo info) {}
        #endregion

        #region AdInfo Interstitial
        // Invoked when the interstitial ad was loaded succesfully.
        protected virtual void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo) {}

        // Invoked when the initialization process has failed.
        protected virtual void InterstitialOnAdLoadFailed(IronSourceError ironSourceError) {}

        // Invoked when the Interstitial Ad Unit has opened. This is the impression indication. 
        protected virtual void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo) {}

        // Invoked when end user clicked on the interstitial ad
        protected virtual void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo) {}

        // Invoked before the interstitial ad was opened, and before the InterstitialOnAdOpenedEvent is reported.
        // This callback is not supported by all networks, and we recommend using it only if  
        // it's supported by all networks you included in your build. 
        protected virtual void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo adInfo) {}

        // Invoked when the ad failed to show.
        protected virtual void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo) {}

        // Invoked when the interstitial ad closed and the user went back to the application screen.
        protected virtual void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo) {}
        #endregion
#else
        void HookEvents() {}
#endif
    }
}
