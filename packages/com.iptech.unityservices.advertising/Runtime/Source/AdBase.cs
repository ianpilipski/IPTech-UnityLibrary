using System;
using System.Threading.Tasks;
using IPTech.Platform;
using UnityEngine;

namespace IPTech.UnityServices.Internal {
    public abstract class AdBase {
        public enum AdState {
            NotLoaded,
            Loading,
            Loaded,
            FailedToLoad,
            Showing
        }

        private Task<FillAdResult> loadTask;
        private Task<AdResult> showTask;

        protected ShowAdResult result;

        public AdBase(string adUnitId) {
            result.AdUnitId = adUnitId;
        }
        
        public AdState CurrentState
        {
            get
            {
                if (showTask != null) return AdState.Showing;
                if (loadTask == null) return AdState.NotLoaded;
                if (!loadTask.IsCompleted) return AdState.Loading;
                if (loadTask.IsCompletedSuccessfully) return AdState.Loaded;
                return AdState.FailedToLoad;
            }
        }

        public Task<FillAdResult> Load()
        {
            if (loadTask != null)
            {
                if (!loadTask.IsCompleted) return loadTask;
                if (loadTask.IsCompletedSuccessfully)
                {
                    return loadTask;
                }
            }

            loadTask = LoadAd();
            return loadTask;
        }

        public async Task<ShowAdResult> Show(string placementName)
        {
            if (showTask != null) throw new InvalidOperationException("Ad is already showing");
            if (loadTask == null || !loadTask.IsCompletedSuccessfully) throw new InvalidOperationException("Ad must be loaded before showing");

            try
            {
                if (!string.IsNullOrWhiteSpace(placementName))
                {
                    result.PlacementName = placementName;
                }
                showTask = ShowAd(result.PlacementName);
                result.AdResult = await showTask;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error showing ad: {ex.Message}");
                result.AdResult = AdResult.FailedToShow;
            }
            finally
            {
                showTask = null;
                loadTask = null;
            }
            return result;
        }

        protected abstract Task<FillAdResult> LoadAd();
        protected abstract Task<AdResult> ShowAd(string placementName = null);
            
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
    }
}
