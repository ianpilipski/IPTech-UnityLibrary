using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IPTech.Platform;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using UnityEngine;

namespace IPTech.UnityServices.Leaderboards {
    public class LeaderboardsManager : ILeaderboardsManager {
        private readonly IUnityServicesManager unityServicesManager;
        private ILeaderboardsService service;

        public bool Initialized { get; private set; }

        public LeaderboardsManager(IUnityServicesManager unityServicesManager) {
            this.unityServicesManager = unityServicesManager;
            Initialize();
        }

        public async Task<ILeaderboardEntry> AddScore(string leaderboardId, double score) {
            await WaitUntilInitialized();
            return new LeaderboardEntryAdapter(await service.AddPlayerScoreAsync(leaderboardId, score));
        }

        public async Task<ILeaderboardScoresPage> GetScores(string leaderboardId) {
            await WaitUntilInitialized();
            return new LeaderboardScoresPageAdapter(await service.GetScoresAsync(leaderboardId));
        }

        public async Task<List<ILeaderboardEntry>> GetScoresInPlayerRange(string leaderboardId, int rangeLimit) {
            await WaitUntilInitialized();
            try {
                var res = await service.GetPlayerRangeAsync(leaderboardId, new GetPlayerRangeOptions { RangeLimit = rangeLimit });
                return res.Results.Select(r => (ILeaderboardEntry)new LeaderboardEntryAdapter(r)).ToList();
            } catch(LeaderboardsException lex) {
                if(lex.Reason == LeaderboardsExceptionReason.EntryNotFound) {
                    var res = await GetScores(leaderboardId);
                    return res.Entries;
                }
                throw;
            }
        }

        void Initialize() {
            if(unityServicesManager.State == EServiceState.Initializing) {
                unityServicesManager.Initialized += HandleInitialized;
                return;
            }
            HandleInitialized();
        }

        void HandleInitialized() {
            if(unityServicesManager.State == EServiceState.Initialized) {
                service = LeaderboardsService.Instance;
            }
            Initialized = true;
        }

        async Task WaitUntilInitialized() {
            while(!Initialized) {
                await Task.Yield();
                if(!Application.isPlaying) throw new OperationCanceledException("unity editor exited play mode");
            }
            if(unityServicesManager.State == EServiceState.FailedToInitialize) {
                throw new ApiUnavailableException(nameof(LeaderboardsManager));
            }
        }
    }
}
