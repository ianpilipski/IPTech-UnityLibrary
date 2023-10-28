using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.Platform.Internal {
    internal class RewardAdManager  {
        readonly string adUnitId;

        int retryAttempt;

        AdResult internalResult;

        public RewardAdManager(string adUnitId) {
            this.adUnitId = adUnitId;
        }

        public async Task<ShowAdResult> ShowAd(string placementName) {
            if(MaxSdk.IsRewardedAdReady(adUnitId)) {
                internalResult = AdResult.Unknown;

                MaxSdk.ShowRewardedAd(adUnitId, placementName);
                await WaitForInternalResult();
                return new ShowAdResult { AdResult = internalResult };
            }

            return new ShowAdResult { AdResult = AdResult.FailedToLoad };
        }

        async Task WaitForInternalResult() {
            while(internalResult == AdResult.Unknown) {
                await Task.Yield();
                
            }
        }

        public void Initialize() {

            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;

            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

            // Load the first rewarded ad
            LoadRewardedAd();
        }

        private void LoadRewardedAd() {
            MaxSdk.LoadRewardedAd(adUnitId);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.
            // Reset retry attempt
            retryAttempt = 0;
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) {
            // Rewarded ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).
            retryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));

            CallAfterDelay(retryDelay, LoadRewardedAd);
        }

        async void CallAfterDelay(double delaySeconds, Action action) {
            await Task.Delay((int)(delaySeconds * 1000));
            action();
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {

        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            // Ad revenue paid. Use this callback to track user revenue.
        }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            Debug.Log("OnRewardedAdHiddenEvent");
            if(internalResult == AdResult.Unknown) {
                internalResult = AdResult.Cancelled;
            }

            // Rewarded ad is hidden. Pre-load the next ad
            LoadRewardedAd();
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            Debug.Log("OnRewardedAdDisplayedEvent");
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) {
            Debug.Log("OnRewardedAdFailedToDisplayEvent");
            internalResult = AdResult.FailedToShow;

            // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
            LoadRewardedAd();
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo) {
            Debug.Log("OnRewardedAdReceivedRewardEvent");
            // The rewarded ad displayed and the user should receive the reward.
            internalResult = AdResult.Watched;
        }
    }
}
