using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.LevelPlay;
using IPTech.Platform;
using UnityEngine;

namespace IPTech.UnityServices.Internal {
    public class RewardAd : AdBase
    {
        bool rewardWasCalled;
        private TaskCompletionSource<FillAdResult> _loadTaskCompletionSource;
        private TaskCompletionSource<AdResult> _showAdCompletionSource;
        private readonly LevelPlayRewardedAd _rewardAd;

        public RewardAd(string adUnitId) : base(adUnitId)
        {
            _rewardAd = new LevelPlayRewardedAd(adUnitId);
            HookEvents();
        }

        private void HookEvents()
        {
            _rewardAd.OnAdClicked += HandleAdClicked;
            _rewardAd.OnAdClosed += HandleAdClosed;
            _rewardAd.OnAdDisplayed += HandleAdDisplayed;
            _rewardAd.OnAdDisplayFailed += HandleAdDisplayFailed;
            _rewardAd.OnAdInfoChanged += HandleAdInfoChanged;
            _rewardAd.OnAdLoaded += HandleAdLoaded;
            _rewardAd.OnAdLoadFailed += HandleAdLoadFailed;
            _rewardAd.OnAdRewarded += HandleAdRewarded;
        }

        override protected Task<FillAdResult> LoadAd()
        {
            if(_loadTaskCompletionSource != null && !_loadTaskCompletionSource.Task.IsCompleted) throw new InvalidOperationException("Ad is already loading");
            _loadTaskCompletionSource = new TaskCompletionSource<FillAdResult>();
            _rewardAd.LoadAd();
            return _loadTaskCompletionSource.Task;
        }

        override protected Task<AdResult> ShowAd(string placementName = null)
        {
            if (_showAdCompletionSource != null && !_showAdCompletionSource.Task.IsCompleted) throw new InvalidOperationException("Ad is already showing");
            if (!_rewardAd.IsAdReady()) throw new InvalidOperationException("Ad must be loaded before showing");
            if (string.IsNullOrWhiteSpace(placementName))
            {
                if (LevelPlayRewardedAd.IsPlacementCapped(placementName))
                {
                    return Task.FromResult(AdResult.FailedToShow);
                }
                _showAdCompletionSource = new TaskCompletionSource<AdResult>();
                _rewardAd.ShowAd();
            }
            else
            {
                _showAdCompletionSource = new TaskCompletionSource<AdResult>();
                _rewardAd.ShowAd(placementName);
            }
            return _showAdCompletionSource.Task;
        }

        private void HandleAdClicked(LevelPlayAdInfo adInfo)
        {

        }

        private void HandleAdClosed(LevelPlayAdInfo adInfo)
        {
            if(_showAdCompletionSource == null)
            {
                Debug.LogError("Show task completion source is null when ad is closed.");
                return;
            }
            WaitForReward();

            async void WaitForReward()
            {
                var timeout = Task.Delay(5000); // Wait for 5 seconds for the reward to be called
                await WaitWhile(() => !rewardWasCalled || !timeout.IsCompleted);
                if (rewardWasCalled)
                {
                    _showAdCompletionSource.SetResult(AdResult.Watched);
                }
                _showAdCompletionSource.SetResult(AdResult.Cancelled);
            }
        }

        private void HandleAdDisplayed(LevelPlayAdInfo adInfo)
        {

        }

#pragma warning disable 618
        private void HandleAdDisplayFailed(LevelPlayAdDisplayInfoError errorInfo)
        {
            if (_showAdCompletionSource == null)
            {
                Debug.LogError("Show task completion source is null when ad fails to display.");
                return;
            }
            _showAdCompletionSource.SetResult(AdResult.FailedToShow);
        }
#pragma warning restore 618

        private void HandleAdInfoChanged(LevelPlayAdInfo adInfo)
        {
            // Handle ad info changes if needed
        }

        private void HandleAdLoaded(LevelPlayAdInfo adInfo)
        {
            if (_loadTaskCompletionSource == null)
            {
                Debug.LogError("Load task completion source is null when ad is loaded.");
                return;
            }
            _loadTaskCompletionSource.SetResult(FillAdResult.Loaded);
        }

        private void HandleAdLoadFailed(LevelPlayAdError errorInfo)
        {
            if (_loadTaskCompletionSource == null)
            {
                Debug.LogError("Load task completion source is null when ad fails to load.");
                return;
            }
            _loadTaskCompletionSource.SetResult(FillAdResult.FailedToLoad);
        }   
        
        private void HandleAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
        {
            if (rewardWasCalled) return; // Prevent multiple calls
            rewardWasCalled = true;

            // Handle the reward logic here, e.g., grant in-game currency or items
            Debug.Log($"Reward granted for ad: {adInfo.AdUnitId}");
        }

    }
}
