using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using IPTech.Platform;
using Unity.Services.LevelPlay;
using UnityEngine;

namespace IPTech.UnityServices.Internal {
    public class InterstitialAd : AdBase
    {
        private readonly LevelPlayInterstitialAd _levelPlayInterstitialAd;
        private TaskCompletionSource<FillAdResult> _loadTaskCompletionSource;
        private TaskCompletionSource<AdResult> _showAdCompletionSource;

        public InterstitialAd(string adUnitId) : base(adUnitId)
        {
            _levelPlayInterstitialAd = new LevelPlayInterstitialAd(adUnitId);
            HookEvents();
        }

        private void HookEvents()
        {
            _levelPlayInterstitialAd.OnAdClicked += HandleAdClicked;
            _levelPlayInterstitialAd.OnAdClosed += HandleAdClosed;
            _levelPlayInterstitialAd.OnAdDisplayed += HandleAdDisplayed;
            _levelPlayInterstitialAd.OnAdDisplayFailed += HandleAdDisplayFailed;
            _levelPlayInterstitialAd.OnAdInfoChanged += HandleAdInfoChanged;
            _levelPlayInterstitialAd.OnAdLoaded += HandleAdLoaded;
            _levelPlayInterstitialAd.OnAdLoadFailed += HandleAdLoadFailed;
        }

        override protected Task<FillAdResult> LoadAd()
        {
            if (_loadTaskCompletionSource != null && !_loadTaskCompletionSource.Task.IsCompleted) throw new InvalidOperationException("Ad is already loading");
            _loadTaskCompletionSource = new TaskCompletionSource<FillAdResult>();

            _levelPlayInterstitialAd.LoadAd();
            return _loadTaskCompletionSource.Task;
        }

        override protected Task<AdResult> ShowAd(string placementName = null)
        {
            if (_showAdCompletionSource != null && !_showAdCompletionSource.Task.IsCompleted) throw new InvalidOperationException("Ad is already showing");
            if (!_levelPlayInterstitialAd.IsAdReady()) throw new InvalidOperationException("Ad must be loaded before showing");
            if (string.IsNullOrWhiteSpace(placementName))
            {
                if (LevelPlayInterstitialAd.IsPlacementCapped(placementName))
                {
                    return Task.FromResult(AdResult.FailedToShow);
                }
                _showAdCompletionSource = new TaskCompletionSource<AdResult>();
                _levelPlayInterstitialAd.ShowAd();
            }
            else
            {
                _showAdCompletionSource = new TaskCompletionSource<AdResult>();
                _levelPlayInterstitialAd.ShowAd(placementName);
            }
            return _showAdCompletionSource.Task;
        }

        private void HandleAdClicked(LevelPlayAdInfo adInfo)
        {
            
        }

        private void HandleAdClosed(LevelPlayAdInfo adInfo)
        {
            if (_showAdCompletionSource == null)
            {
                Debug.LogError("Show task completion source is null, cannot complete show.");
                return;
            }
            _showAdCompletionSource.SetResult(AdResult.Watched);
        }

        private void HandleAdDisplayed(LevelPlayAdInfo adInfo)
        {
            
        }

#pragma warning disable 618
        private void HandleAdDisplayFailed(LevelPlayAdDisplayInfoError errorInfo)
        {
            if (_showAdCompletionSource == null)
            {
                Debug.LogError("Show task completion source is null, cannot complete show.");
                return;
            }
            _showAdCompletionSource.SetResult(AdResult.FailedToShow);
        }
#pragma warning restore 618

        private void HandleAdInfoChanged(LevelPlayAdInfo adInfo)
        {
            // This method can be used to update ad info if needed
        }

        private void HandleAdLoaded(LevelPlayAdInfo adInfo)
        {
            if (_loadTaskCompletionSource == null)
            {
                Debug.LogError("Load task completion source is null, cannot complete load.");
                return;
            }
            _loadTaskCompletionSource.SetResult(FillAdResult.Loaded);
            _loadTaskCompletionSource = null;
        }

        private void HandleAdLoadFailed(LevelPlayAdError errorInfo)
        {
            if (_loadTaskCompletionSource == null)
            {
                Debug.LogError("Load task completion source is null, cannot complete load.");
                return;
            }
            _loadTaskCompletionSource.SetResult(FillAdResult.FailedToLoad);
            _loadTaskCompletionSource = null;
        }
    }
}
