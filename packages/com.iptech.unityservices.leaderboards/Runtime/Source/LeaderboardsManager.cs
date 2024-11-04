using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IPTech.Platform;
using Unity.Services.Leaderboards;

namespace IPTech.UnityServices.Leaderboards {
    public class LeaderboardsManager : ILeaderboardsManager {
        private readonly IUnityServicesManager unityServicesManager;

        public bool Initialized { get; private set; }

        public LeaderboardsManager(IUnityServicesManager unityServicesManager) {
            this.unityServicesManager = unityServicesManager;
            Initialize();
        }

        public async Task<ILeaderboardEntry> AddScore(string leaderboardId, double score) {
            AssertInitialized();
            return new LeaderboardEntryAdapter(await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score));
        }

        public async Task<ILeaderboardScoresPage> GetScores(string leaderboardId) {
            AssertInitialized();
            return new LeaderboardScoresPageAdapter(await LeaderboardsService.Instance.GetScoresAsync(leaderboardId));
        }

        public async Task<List<ILeaderboardEntry>> GetScoresInPlayerRange(string leaderboardId, int rangeLimit) {
            AssertInitialized();
            var res = await LeaderboardsService.Instance.GetPlayerRangeAsync(leaderboardId, new GetPlayerRangeOptions { RangeLimit = rangeLimit });
            return res.Results.Select(r => (ILeaderboardEntry)new LeaderboardEntryAdapter(r)).ToList();
        }

        void Initialize() {
            if(unityServicesManager.State == EServiceState.Initializing) {
                unityServicesManager.Initialized += HandleInitialized;
                return;
            }
            HandleInitialized();
        }

        void HandleInitialized() {
            Initialized = true;
        }

        void AssertInitialized() {
            if(Initialized) return;
            throw new InvalidOperationException("the platform has not yet been initialized");
        }
    }
}
