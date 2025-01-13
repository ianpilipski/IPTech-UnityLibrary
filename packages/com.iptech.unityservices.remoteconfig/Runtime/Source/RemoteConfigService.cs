using System;
using System.Threading;
using System.Threading.Tasks;
using IPTech.Platform;
using Unity.Services.RemoteConfig;
using UnityEngine;

namespace IPTech.UnityServices.RemoteConfig {
    public class RemoteConfigManager : IRemoteConfigManager {
        private readonly IUnityServicesManager unityServicesManager;
        private RemoteConfigService service;
        private EServiceState initialized;

        public RemoteConfigManager(IUnityServicesManager ipTechUnityServices) {
            initialized = EServiceState.Initializing;
            this.unityServicesManager = ipTechUnityServices;
            HookInitialize();
        }

        void HookInitialize() {
            if(this.unityServicesManager.State != EServiceState.Initializing) {
                HandleInitialized(this.unityServicesManager.State);
            } else {
                this.unityServicesManager.Initialized += HandleInitialized;
            }

            void HandleInitialized(EServiceState state) {
                initialized = state;
                if(state != EServiceState.Initialized) return;
                service = RemoteConfigService.Instance;
                Debug.Log($"[IPTech.UnityServices.RemoteConfig] initialized");
                // no need to call RemoteConfigService.Instance.SetEnvironmentID("an-env-id");
                // as it was set in the unity service initialization through SetEnvironment(..)
            }
        }

        public EOnlineState OnlineState => this.unityServicesManager.Authentication.IsSignedIn ? EOnlineState.Online : EOnlineState.Offline;

        public Task<IRuntimeConfig> GetConfigs(int onlineTimeoutMilliseconds) {
            return GetConfigs(onlineTimeoutMilliseconds, new { }, new { }, new { });
        }

        public async Task<IRuntimeConfig> GetConfigs<TUserAttributes, TAppAttributes, TFilterAttributes>(
            int onlineTimeoutMilliseconds = 0,
            TUserAttributes userAttributes = null,
            TAppAttributes appAttributes = null,
            TFilterAttributes filterAttributes = null)
            where TUserAttributes : class
            where TAppAttributes : class
            where TFilterAttributes : class
            {
            Debug.Log($"[IPTech.UnityServices.RemoteConfig] get configs started");
            await WaitUntilInitialized();
            IPTechExceptions.AssertServiceInitialized(initialized, nameof(RemoteConfigManager));

            // unity remote config needs to have unity services initialized, but it should work without sign in
            // we give the user the option to wait a certain amount of time to come online
            if(onlineTimeoutMilliseconds > 0 && OnlineState == EOnlineState.Offline
                && unityServicesManager.Network.State == ENetworkState.Reachable) {
                await WaitForOnline();
            }
            
            var res = await service.FetchConfigsAsync(userAttributes, appAttributes, filterAttributes);

            Debug.Log($"[IPTech.UnityServices.RemoteConfig] get configs finished, returning result ({res.origin})");
            return new RuntimeConfigAdapter(res);

            async Task WaitForOnline() {
                Debug.Log($"[IPTech.UnityServices.RemoteConfig] waiting for service to connect online...");
                bool timedOut = false;
                var startTime = DateTime.UtcNow;
                while(OnlineState == EOnlineState.Offline && !timedOut) {
                    await Task.Yield();
                    timedOut = (DateTime.UtcNow - startTime).TotalMilliseconds > onlineTimeoutMilliseconds;
                }
                Debug.Log($"[IPTech.UnityServices.RemoteConfig] service " +
                    (timedOut ? "did not connect " : "connected ") +
                    $"after {(DateTime.UtcNow - startTime).TotalMilliseconds} milliseconds");
            }
        }

        private async Task WaitUntilInitialized() {
            while(initialized == EServiceState.Initializing) {
                await Task.Yield();
                if(!Application.isPlaying) throw new OperationCanceledException("unity playmode state changed");
            }
        }
    }
}
