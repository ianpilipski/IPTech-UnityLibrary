using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace IPTech.UnityServices {
    using Unity.Services.LevelPlay;
    using Platform;
    using IPTech.UnityServices.Internal;

    public class AdsManager : IAdsManager
    {
        readonly IUnityServicesManager unityServicesManager;
        private IIPTechPlatform platform => unityServicesManager.Platform;
        readonly Dictionary<string, Internal.InterstitialAd> interstitialAds = new();
        readonly Dictionary<string, Internal.RewardAd> rewardAds = new();
        readonly Config config;

        bool initialized;
        bool alreadyCalledInitialized;
        bool isShowingAd;
        bool alreadySetConsent;
        LevelPlayConfiguration configuration;

        public event Action<ShowAdResult> AdShown;

        public AdsManager(IUnityServicesManager unityServicesManager, Config config)
        {
            this.unityServicesManager = unityServicesManager;
            this.config = config;
            Initialize();
        }

        void Initialize()
        {
            if (platform.State == EServiceState.Initializing)
            {
                platform.Initialized += HandleUnityServicesInitialized;
                return;
            }
            HandleUnityServicesInitialized();
        }

        void HandleUnityServicesInitialized()
        {
            platform.ConsentValueChanged += HandleConsentValueChanged;
            HandleConsentValueChanged(platform.Consent);
        }

        void HandleConsentValueChanged(ConsentInfo consentValue)
        {
            if (consentValue.Consent == EConsentValue.Unknown) return;
            //TODO Consent should be set prior to init https://developers.is.com/ironsource-mobile/unity/regulation-advanced-settings/#step-1
            SetConsent(consentValue);
            InitializeLevelPlay();
        }

        void SetConsent(ConsentInfo consentValue)
        {
            alreadySetConsent = true;

            var didConsent = consentValue.Consent == EConsentValue.Accepted;
            var didConsentString = consentValue.OkToSell ? "true" : "false";
            var isUnder13String = consentValue.AgeInfo == EConsentAge.Adult ? "false" : "true";

            LevelPlay.SetConsent(didConsent);
            LevelPlay.SetMetaData("do_not_sell", didConsentString); // true/false
            LevelPlay.SetMetaData("is_child_directed", isUnder13String); // true/false            
            // for childen or users of unkown age ...
            if (consentValue.Consent != EConsentValue.Accepted || consentValue.AgeInfo != EConsentAge.Adult)
            {
                LevelPlay.SetMetaData("is_deviceid_optout", "true");
                LevelPlay.SetMetaData("Google_Family_Self_Certified_SDKS", "true");
                LevelPlay.SetMetaData("UnityAds_coppa", "true");
            }
            else
            {
                LevelPlay.SetMetaData("is_deviceid_optout", "false");
                LevelPlay.SetMetaData("Google_Family_Self_Certified_SDKS", "false");
                LevelPlay.SetMetaData("UnityAds_coppa", "false");
            }
        }

        void InitializeLevelPlay()
        {
            if (!alreadySetConsent) throw new InvalidOperationException("consent must be set prior to calling this function");
            if (alreadyCalledInitialized) return;
            alreadyCalledInitialized = true;

            // SDK init

            // to enable testsuite must call before init
            if (config.EnableTestSuite || Application.isEditor)
                LevelPlay.SetMetaData("is_test_suite", "enable");
      
            Debug.Log("unity-script: LevelPlay.Init");
            LevelPlay.OnInitSuccess += HandleLevelPlayInitSuccess;
            LevelPlay.OnInitFailed += HandleLevelPlayInitFailed;
            LevelPlay.Init(config.AppKey);

            void HandleLevelPlayInitFailed(LevelPlayInitError error)
            {
                Debug.LogError($"unity-script: LevelPlay.Init failed with error: {error}");
                initialized = true;
            }

            void HandleLevelPlayInitSuccess(LevelPlayConfiguration configuration)
            {
                Debug.Log("unity-script: LevelPlay.Init success");
                this.configuration = configuration;
                initialized = true;
            }
        }

        void HandleApplicationPaused(bool isPaused)
        {
            if (initialized)
            {
                Debug.Log("unity-script: LevelPlay.SetPauseGame " + isPaused);
                LevelPlay.SetPauseGame(isPaused);
            }
        }

        private async Task<FillAdResult> FillAd(AdType adType, string adUnitId)
        {
            DebugLog($"FillAd - {adType}");
            try
            {
                await WaitForInitialization();

                switch (adType)
                {
                    case AdType.Interstitial:
                        if (!interstitialAds.TryGetValue(adUnitId, out var ad))
                        {
                            ad = new Internal.InterstitialAd(adUnitId);
                            interstitialAds[adUnitId] = ad;
                        }
                        return await ad.Load();
                    case AdType.Reward:
                        if (!rewardAds.TryGetValue(adUnitId, out var rewardAd))
                        {
                            rewardAd = new Internal.RewardAd(adUnitId);
                            rewardAds[adUnitId] = rewardAd;
                        }
                        return await rewardAd.Load();
                    default:
                        throw new NotSupportedException($"Unsupported ad type: {adType}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error filling ad: {e.Message}");
                return FillAdResult.FailedToLoad;
            }
        }

        private bool IsAdFilled(AdType adType, string adUnitId)
        {
            switch (adType)
            {
                case AdType.Interstitial:
                    return interstitialAds.TryGetValue(adUnitId, out var ad) && ad.CurrentState == AdBase.AdState.Loaded;
                case AdType.Reward:
                    return rewardAds.TryGetValue(adUnitId, out var rewardAd) && rewardAd.CurrentState == AdBase.AdState.Loaded;
                default:
                    throw new NotSupportedException($"Unsupported ad type: {adType}");
            }
        }

        public async Task<ShowAdResult> ShowAd(AdType adType, string placementName)
        {
            if (!isShowingAd)
            {
                isShowingAd = true;
                DebugLog($"ShowAd - {adType}");
                var adUnitId = config.AdUnitIds[adType];
                try
                {
                    await WaitForInitialization();

                    ShowAdResult res = default;
                    if (!Application.isEditor)
                    {
                        var fillRes = await FillAd(adType, adUnitId);
                        if (fillRes == FillAdResult.FailedToLoad)
                        {
                            DebugLog($"ShowAd - {adType} - Failed to fill ad");
                            return new ShowAdResult { AdResult = AdResult.FailedToLoad, AdUnitId = adUnitId, PlacementName = placementName };
                        }

                        switch (adType)
                        {
                            case AdType.Interstitial:
                                res = await interstitialAds[adUnitId].Show(placementName);
                                break;
                            case AdType.Reward:
                                res = await rewardAds[adUnitId].Show(placementName);
                                break;
                            default:
                                throw new NotSupportedException($"Unsupported ad type: {adType}");
                        }
                    }
                    else
                    {
                        res = new ShowAdResult { AdResult = AdResult.FailedToLoad, AdUnitId = adUnitId, PlacementName = placementName };
                    }
                    DebugLog($"ShowAd - {adType} - {res}");
                    OnAdShown(res);
                    return res;
                }
                finally
                {
                    isShowingAd = false;
                }
            }
            else
            {
                throw new InvalidOperationException("you are already showing an ad, you can only show 1 ad at a time");
            }
        }

        void OnAdShown(ShowAdResult res)
        {
            try
            {
                AdShown?.Invoke(res);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        async Task WaitForInitialization()
        {
            if (initialized)
            {
                if (configuration != null) return;
                throw new InvalidOperationException("the SDK failed to initialize, check the logs for more details");
            }

            if (platform.State != EServiceState.Initializing)
            {
                if (platform.Consent.Consent != EConsentValue.Unknown)
                {
                    DateTime timeout = DateTime.Now.AddSeconds(5);
                    while (!initialized)
                    {
                        await Task.Yield();
                        if (DateTime.Now > timeout) throw new Exception("timed out waiting for the sdk to initialize");
                        if (!Application.isPlaying) throw new OperationCanceledException("playmode state changed");
                    }
                }
                throw new InvalidOperationException("you must set the Consent value on UnityServicesManager before using this function");
            }
            throw new InvalidOperationException("you must call UnityServicesManager initialize before using this function");
        }

        void DebugLog(string msg)
        {
            Debug.Log($"[IPTech.AdsManager]: {msg}");
        }

        public void ShowDebugger()
        {
            if (!Application.isEditor && !config.EnableTestSuite)
            {
                Debug.LogError("you must configure the ads manager with EnableTestSuite = true to use this function");
                return;
            }
            LevelPlay.LaunchTestSuite();
        }


        public class Config
        {
            public string AppKey;
            public Dictionary<AdType, string> AdUnitIds;
            public bool EnableTestSuite;

            public Config(string appKey, Dictionary<AdType, string> adUnitIds)
            {
                AppKey = appKey ?? throw new ArgumentNullException(nameof(appKey), "AppKey cannot be null");
                AdUnitIds = adUnitIds ?? throw new ArgumentNullException(nameof(adUnitIds), "AdUnitIds cannot be null");
            }
        }
    }
}