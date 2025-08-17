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
        readonly string appKey;
        readonly IIPTechPlatform platform;
        readonly Dictionary<string, Internal.InterstitialAd> interstitialAds = new ();
        readonly Dictionary<string, Internal.RewardAd> rewardAds = new ();

        bool initialized;
        bool alreadyCalledInitialized;
        bool isShowingAd;
        bool alreadySetConsent;
        LevelPlayConfiguration configuration;

        public event Action<ShowAdResult> AdShown;

        public AdsManager(IIPTechPlatform platform)
        {
            this.platform = platform;

            this.appKey = "<your app key here>";
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
            var didConsentString = didConsent ? "true" : "false";
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
            // LevelPlay.SetMetaData("is_test_suite", "enable");

            Debug.Log("unity-script: LevelPlay.Init");
            LevelPlay.OnInitSuccess += HandleLevelPlayInitSuccess;
            LevelPlay.OnInitFailed += HandleLevelPlayInitFailed;
            LevelPlay.Init(appKey);

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

        public async Task<FillAdResult> FillAd(AdType adType, string adUnitId)
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
                        if(!rewardAds.TryGetValue(adUnitId, out var rewardAd))
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

        public bool IsAdFilled(AdType adType, string adUnitId)
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

        public async Task<ShowAdResult> ShowAd(AdType adType, string adUnitId, string placementName = null)
        {
            if (!isShowingAd)
            {
                isShowingAd = true;
                DebugLog($"ShowAd - {adType}");
                try
                {
                    await WaitForInitialization();
                    
                    ShowAdResult res = default;
                    if (!Application.isEditor)
                    {
                        var fillRes = await FillAd(adType, adUnitId);
                        if(fillRes == FillAdResult.FailedToLoad) 
                        {
                            DebugLog($"ShowAd - {adType} - Failed to fill ad");
                            return new ShowAdResult { AdResult = AdResult.FailedToLoad, AdUnitId = adUnitId, PlacementID = placementName };
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
                        res = new ShowAdResult { AdResult = AdResult.FailedToLoad, AdUnitId = adUnitId, PlacementID = placementName };
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
                AdShown.Invoke(res);
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
            LevelPlay.LaunchTestSuite();
        }
    }

}