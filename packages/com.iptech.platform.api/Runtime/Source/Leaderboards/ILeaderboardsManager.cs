using System.Collections.Generic;
using System.Threading.Tasks;

namespace IPTech.Platform {
    public interface ILeaderboardsManager {
        Task<ILeaderboardEntry> AddScore(string leaderboardId, double score);
        Task<ILeaderboardScoresPage> GetScores(string leaderboardId);
        Task<List<ILeaderboardEntry>> GetScoresInPlayerRange(string leaderboardId, int rangeLimit);
    }
}
