using System.Threading;
using System.Threading.Tasks;

namespace IPTech.Platform {
    public interface IRemoteConfigManager {
        EOnlineState OnlineState { get; }
        Task<IRuntimeConfig> GetConfigs(int onlineTimeoutMilliseconds);           
    }
}
