using System.Collections.Generic;
using System.Linq;
using IPTech.Platform;
using Unity.Services.Leaderboards.Models;

namespace IPTech.UnityServices.Leaderboards {
    public class LeaderboardScoresPageAdapter : ILeaderboardScoresPage {
        private readonly LeaderboardScoresPage page;
        private readonly List<ILeaderboardEntry> entries;

        public LeaderboardScoresPageAdapter(LeaderboardScoresPage page) {
            this.page = page;
            if(page.Results != null) {
                entries = page.Results.Select(r => (ILeaderboardEntry)new LeaderboardEntryAdapter(r)).ToList();
            } else {
                entries = new List<ILeaderboardEntry>();
            }
        }

        public int Offset => page.Offset;

        public int Limit => page.Limit;

        public int Total => page.Total;

        public IList<ILeaderboardEntry> Entries => entries;
    }
}