using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.Platform {
    using Internal;

    public class MaxSdkAdsManager : IAdsManager {
        readonly IIPTechPlatform platform;
        readonly string sdkKey;
        readonly string userId;
        readonly RewardAdManager rewardAdManager;

        EInitValue initValue;

        enum EInitValue {
            None,
            InitializeCalled,
            Initialized
        }

        public MaxSdkAdsManager(IIPTechPlatform platform, string sdkKey, string rewardAdUnitId, string userId = null) {
            this.platform = platform;
            this.sdkKey = sdkKey;
            this.userId = userId;
            this.rewardAdManager = new RewardAdManager(rewardAdUnitId);

            Initialize();
        }

        void Initialize() {
            if(platform.State == EServiceState.Initializing) {
                Log("waiting for platform initialization ... STARTED");
                platform.Initialized += PlatformInitialized;
                return;
            }

            PlatformInitialized();
        }

        void PlatformInitialized() {
            Log("waiting for platform initialization .. COMPLETE");
            if(platform.Network.State != ENetworkState.Reachable) {
                //NO network???
            }

            platform.ConsentValueChanged += ConsentValueChanged;
            ConsentValueChanged(platform.Consent);
        }

        private void ConsentValueChanged(ConsentInfo obj) {
            Log($"handling consent value changed : {obj.Consent}");
            if(obj.Consent == EConsentValue.Unknown) return;

            MaxSdk.SetHasUserConsent(obj.Consent == EConsentValue.Accepted);
            MaxSdk.SetDoNotSell(obj.Consent == EConsentValue.Accepted);
            MaxSdk.SetIsAgeRestrictedUser(obj.AgeInfo != EConsentAge.Adult);

            InitializeMaxSdk();
        }

        void InitializeMaxSdk() {
            if(initValue == EInitValue.None) {
                Log("calling MaxSdk.InitializeSdk");
                initValue = EInitValue.InitializeCalled;

                MaxSdkCallbacks.OnSdkInitializedEvent += HandleSdkInitialized;

                MaxSdk.SetSdkKey(sdkKey);

                if(!string.IsNullOrEmpty(userId)) {
                    MaxSdk.SetUserId(userId);
                }
                MaxSdk.InitializeSdk();
            }
        }

        private void HandleSdkInitialized(MaxSdkBase.SdkConfiguration obj) {
            Log("maxsdk initialized and callback handled");
            initValue = EInitValue.Initialized;

            //TODO: max sdk docs say to wait 3 to 5 seconds before loading ads to allow networks to init
            
            // max sdk inited start loading ads..
            rewardAdManager.Initialize();
        }

        public void ShowDebugger() {
            MaxSdk.ShowMediationDebugger();
        }

        public Task<ShowAdResult> ShowAd(AdType type, string placementName) {
            if(initValue == EInitValue.Initialized) {
                return ShowAdInternal(type, placementName);
            }

            if(platform.Consent.Consent == EConsentValue.Unknown) {
                throw new Exception("you must set the consent value before calling this function");
            }

            throw new Exception("the maxsdk has not initialized");
        }

        private Task<ShowAdResult> ShowAdInternal(AdType type, string placementName) {
            switch(type) {
                case AdType.Reward:
                    return rewardAdManager.ShowAd(placementName);
            }
            throw new Exception("Ad type not implemented yet");
        }

        void Log(string msg) {
            Debug.Log($"[MaxSdkAdsManager]: {msg}");
        }
    }
}