using System;

namespace IPTech.UnityServices.Leaderboards {
    using IPTech.Platform;
    using Unity.Services.Leaderboards.Models;

    public class LeaderboardEntryAdapter : ILeaderboardEntry {
        private readonly LeaderboardEntry entry;

        public LeaderboardEntryAdapter(LeaderboardEntry entry) {
            this.entry = entry;
        }

        public string PlayerId => entry.PlayerId;

        public string PlayerName => entry.PlayerName;

        public int Rank => entry.Rank;

        public double Score => entry.Score;

        public string Tier => entry.Tier;

        public DateTime LastUpdated => entry.UpdatedTime;
    }
}
