using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace IPTech.UnityServices {
    public enum EAdType {
        InterstitialAd,
        RewardAd
    }

    public enum AdResult {
        FaildToLoad,
        FailedToShow,
        Watched,
        Cancelled,
        Unknown
    }

    public struct ShowAdResult {
        public AdResult AdResult;
        public string PlacementID;
        public bool UserClicked;

        public override string ToString()
        {
            var clicked = UserClicked ? "clicked" : "not clicked";
            return $"{AdResult} : {PlacementID} : {clicked}";
        }
    }

#if IPTECH_IRONSOURCE_INSTALLED
    public class AdsManager 
    {
        readonly string appKey;
        readonly IUnityServicesManager unityServicesManager;

        bool initialized;
        bool alreadyCalledInitialized;
        bool isShowingAd;
        bool alreadySetConsent;

        public AdsManager(IUnityServicesManager unityServicesManager) {
            this.unityServicesManager = unityServicesManager;
        
            var developerSettings = Resources.Load<IronSourceMediationSettings>(IronSourceConstants.IRONSOURCE_MEDIATION_SETTING_NAME);
#if UNITY_ANDROID
            this.appKey = developerSettings.AndroidAppKey;
#elif UNITY_IOS
            this.appKey = developerSettings.IOSAppKey;
#elif !UNITY_EDITOR
            throw new Exception("unsupported platform");
#endif

            if (developerSettings.EnableAdapterDebug)
            {
                IronSource.Agent.setAdaptersDebug(true);
            }

            if (developerSettings.EnableIntegrationHelper)
            {
                IronSource.Agent.validateIntegration();
            }

            HookEventsPriorToInitializing();
            Initialize();
        }

        void Initialize() {
            if(unityServicesManager.State == EState.Initializing) {
                unityServicesManager.Initialized += HandleUnityServicesInitialized;
                return;
            }
            HandleUnityServicesInitialized();            
        }

        void HandleUnityServicesInitialized() {
            unityServicesManager.ConsentValueChanged += HandleConsentValueChanged;
            HandleConsentValueChanged(unityServicesManager.Consent);
        }

        void HandleConsentValueChanged(ConsentInfo consentValue) {
            if(consentValue.Consent == EConsentValue.Unknown) return;
            //TODO Consent should be set prior to init https://developers.is.com/ironsource-mobile/unity/regulation-advanced-settings/#step-1
            SetConsent(consentValue);
            InitializeIronSource();
        }

        void SetConsent(ConsentInfo consentValue) {
            alreadySetConsent = true;

            var didConsent = consentValue.Consent == EConsentValue.Accepted;
            var didConsentString = didConsent ? "true" : "false";
            var isUnder13String = consentValue.AgeInfo == EConsentAge.Over13 ? "false" : "true";

            // IRONSOURCE end-user delete request url https://ironsrc.formtitan.com/Data_Subject_Request
            IronSource.Agent.setMetaData("do_not_sell", didConsentString); // true/false
            IronSource.Agent.setConsent(didConsent);
            IronSource.Agent.setMetaData("is_child_directed", isUnder13String);
            
            //android google stuff (mixed audience)
            //<uses-permission android:name="com.google.android.gms.permission.AD_ID"/>
            //implementation 'com.google.android.gms:play-services-appset:16.0.0'
            //implementation 'com.google.android.gms:play-services-ads-identifier:17.0.0'
            // for childen or users of unkown age ...
            if(consentValue.Consent != EConsentValue.Accepted || consentValue.AgeInfo != EConsentAge.Over13) {
                IronSource.Agent.setMetaData("is_deviceid_optout", "true");
                IronSource.Agent.setMetaData("Google_Family_Self_Certified_SDKS","true");
            } else {
                IronSource.Agent.setMetaData("is_deviceid_optout", "false");
                IronSource.Agent.setMetaData("Google_Family_Self_Certified_SDKS","false");
            }
        }

        void InitializeIronSource()
        {
            if(!alreadySetConsent) throw new InvalidOperationException("consent must be set prior to calling this function");
            if(!alreadyCalledInitialized) {
                alreadyCalledInitialized = true;
                
                Debug.Log("unity-script: IronSource.Agent.validateIntegration");
                IronSource.Agent.validateIntegration();

                Debug.Log("unity-script: unity version" + IronSource.unityVersion());

                // offerwall uses user id .. set prior to init
                // IronSource.Agent.setUserId(YOUR_USER_ID);

                // SDK init
                Debug.Log("unity-script: IronSource.Agent.init");
                IronSource.Agent.init(appKey);
            }
        }

        void HookEventsPriorToInitializing()
        {
            unityServicesManager.ApplicationPaused += HandleApplicationPaused;

            //Add Init Event
            IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;
            
            //Add ImpressionSuccess Event
            IronSourceEvents.onImpressionDataReadyEvent += ImpressionDataReadyEvent;
        }

        void HandleApplicationPaused(bool isPaused)
        {
            if(initialized) {
                Debug.Log("unity-script: OnApplicationPause = " + isPaused);
                IronSource.Agent.onApplicationPause(isPaused);
            }
        }

        void SdkInitializationCompletedEvent()
        {
            initialized = true;
            Debug.Log("unity-script: I got SdkInitializationCompletedEvent");
        }
        
        public async Task<ShowAdResult> ShowAd(EAdType adType, string placementName) {
            if(!isShowingAd) {
                isShowingAd = true;
                DebugLog($"ShowAd - {adType}");
                try {
                    await WaitForInitialization();
                    ShowAdResult res = default;
                    switch(adType) {
                        case EAdType.InterstitialAd:
                            res = await new Internal.InterstitialAd(placementName).Show();
                            break;
                        case EAdType.RewardAd:
                            res = await new Internal.RewardAd(placementName).Show();
                            break;
                    }
                    DebugLog($"ShowAd - {adType} - {res}");  
                    return res;  
                } finally {
                    isShowingAd = false;
                }
            } else {
                throw new InvalidOperationException("you are already showing an ad, you can only show 1 ad at a time");
            }
        }

        async Task WaitForInitialization() {
            if(initialized) return;
            
            if(unityServicesManager.State != EState.Initializing) {
                if(unityServicesManager.Consent.Consent != EConsentValue.Unknown) {
                    DateTime timeout = DateTime.Now.AddSeconds(5);
                    while(!initialized) {
                        await Task.Yield();
                        if(DateTime.Now > timeout) throw new Exception("timed out waiting for the sdk to initialize");
                        if(!Application.isPlaying) throw new OperationCanceledException("playmode state changed");
                    }    
                }
                throw new InvalidOperationException("you must set the Consent value on UnityServicesManager before using this function");
            }
            throw new InvalidOperationException("you must call UnityServicesManager initialize before using this function");
        }

        void DebugLog(string msg) {
            Debug.Log($"[IPTech.AdsManager]: {msg}");
        }
    
        #region ImpressionSuccess callback handler

        void ImpressionSuccessEvent(IronSourceImpressionData impressionData)
        {
            Debug.Log("unity - script: I got ImpressionSuccessEvent ToString(): " + impressionData.ToString());
            Debug.Log("unity - script: I got ImpressionSuccessEvent allData: " + impressionData.allData);
        }

        void ImpressionDataReadyEvent(IronSourceImpressionData impressionData)
        {
            Debug.Log("unity - script: I got ImpressionDataReadyEvent ToString(): " + impressionData.ToString());
            Debug.Log("unity - script: I got ImpressionDataReadyEvent allData: " + impressionData.allData);
        }

        #endregion
    }
#else
    public class AdsManager {
        public AdsManager(IUnityServicesManager unityServicesManager) {}
        public Task<ShowAdResult> ShowAd(EAdType adType, string placementName) {
            throw new Exception("you must integrate IronSource for this to work.");
        }
    }
#endif
}