
using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments;

namespace IPTech.UnityServices {
    using UEServices = global::Unity.Services.Core.UnityServices;

    public class IPTechUnityServices {
        #if IPTECH_UNITYANALYTICS_INSTALLED
        
        #endif
        

        readonly INetworkDetector networkDetector;
        public readonly Task<bool> initTask;

        public enum EState {
            NotOnline,
            Online
        }

        public string EnvironmentID {
            get {
                if(Debug.isDebugBuild) {
                    return "development";
                }
                return "production";
            }
        }

        public EState State { get; private set; }

        public IPTechUnityServices() : this(new NetworkDetector(10)) {}
        private IPTechUnityServices(INetworkDetector networkDetector) {
            State = EState.NotOnline;
            this.networkDetector = networkDetector;
            initTask = Initialize();
        }

        async Task<bool> Initialize() {
            await WaitUntilOnline();
            State = await InitializeUnityServices();
            return true;
        }

        async Task WaitUntilOnline() {
            var online = networkDetector.State == ENetworkState.Reachable;
            if(online) {
                return;
            }

            networkDetector.NetworkStateChanged += HandleNetworkStateChanged;
            while(!online) {
                await Task.Yield();
                if(!Application.isPlaying) {
                    throw new OperationCanceledException();
                }
            }

            void HandleNetworkStateChanged(ENetworkState newState) {
                online = newState == ENetworkState.Reachable;
            }
        }

        async Task<EState> InitializeUnityServices() {
            if(Application.isPlaying) {
                try {
                    var options = new InitializationOptions();

                    options.SetEnvironmentName(EnvironmentID);
                    await UEServices.InitializeAsync(options);

                    #if UNITY_AUTHSERVICE_INSTALLED
                    // remote config requires authentication for managing environment information
                    if(!AuthenticationService.Instance.IsSignedIn) {
                        await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    }
                    #endif

                    return EState.Online;
                } catch(Exception e) {
                    Debug.LogException(e);
                }
            }
            return EState.NotOnline;
        }

        public Task<bool> IsInitialized() => initTask;
    }
}