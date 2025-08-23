using IPTech.Platform;
using UnityEngine;

namespace IPTech.UnityServices
{
    using Leaderboards;
    public static class UnityServicesConfigExtensionMethodsLeaderboards
    {
        public static UnityServicesConfig ConfigureLeaderboards(this UnityServicesConfig config)
        {
            config.RegisterFactory<ILeaderboardsManager>((usm) => new LeaderboardsManager(usm));
            return config;
        }
    }
}
