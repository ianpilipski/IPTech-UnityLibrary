

using System;

namespace IPTech.Platform {
    public interface ILeaderboardEntry {
        string PlayerId { get; }
        string PlayerName { get; }
        int Rank { get; }
        double Score { get; }
        string Tier { get; }
        DateTime LastUpdated { get; }
    }
}
