using UnityEngine;
using System;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif


namespace IPTech.ConsentScreen {
    /// <summary>
    /// This component will trigger the context screen to appear when the scene starts,
    /// if the user hasn't already responded to the iOS tracking dialog.
    /// </summary>
    public class ConsentScreenManager : MonoBehaviour
    {
        /// <summary>
        /// The prefab that will be instantiated by this component.
        /// The prefab has to have an ContextScreenView component on its root GameObject.
        /// </summary>
        [SerializeField] private ContextScreenView contextScreenPrefab;
        [SerializeField] private SimpleConsentView simpleConsentPrefab;
        [SerializeField] private string PrivacyPolicyUrl = "https://www.threepstreet.com/privacy-policy";
        [SerializeField] private string TOSUrl =  "https://www.threepstreet.com/terms-of-service";
        [SerializeField] private ConsentScreenHandler handler;
        [SerializeField] private string NextSceneName;

        void Start() {
            if(handler == null) {
                Debug.LogError("You must assign a consent handler to the consent screen manager.");
                return;
            }
            simpleConsentPrefab.TOSUrl = TOSUrl;
            simpleConsentPrefab.PrivacyPolicyUrl = PrivacyPolicyUrl;

            CheckConsent();    
        }

        void CheckConsent() {
            try {
                var consentInfo = handler.GetCurrentConsentInfo();
                if(consentInfo.Consent != EConsentValue.Accepted) {
                    var optInScreen = Instantiate(simpleConsentPrefab).GetComponent<SimpleConsentView>();

                    optInScreen.simpleConsentAccepted += () => {
                        Destroy(optInScreen.gameObject);
                        //TODO: add age gate to consent popup
                        //TODO: Ironsource says to show their dialog here??? https://developers.is.com/ironsource-mobile/flutter/permission-popup-ios/#step-1
                        CheckATT(new ConsentInfo {
                            Consent = EConsentValue.Accepted,
                            AgeInfo = EConsentAge.Child
                        });
                    };
                } else {
                    CheckATT(consentInfo);
                }
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }

        void CheckATT(ConsentInfo info)
        {
#if UNITY_IOS
            // check with iOS to see if the user has accepted or declined tracking
            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
            info.IOSAppTrackingStatus = Status(status);
            if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                var contextScreen = Instantiate(contextScreenPrefab).GetComponent<ContextScreenView>();

                // after the Continue button is pressed, and the tracking request
                // has been sent, automatically destroy the popup to conserve memory
                contextScreen.sentTrackingAuthorizationRequest += () => {
                    Destroy(contextScreen.gameObject);
                    var newStatus = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                    Debug.Log($"Recevied callback from ATTrackingStatus consent request: {newStatus}");
                    if(Application.isEditor) {
                        Debug.Log($"Setting ATTrackingStatus in editor to RESTRICTED");
                        newStatus = ATTrackingStatusBinding.AuthorizationTrackingStatus.RESTRICTED;
                    }
                    info.IOSAppTrackingStatus = Status(newStatus);
                    LoadNextScene(info);
                    };

                contextScreen.RequestAuthorizationTracking();
            } else {
                // iOS will reject us for not calling the ATT prompt even though we know what the answer is
                if(!Application.isEditor) {
                    ATTrackingStatusBinding.RequestAuthorizationTracking();
                }
                LoadNextScene(info);
            }
#else
            Debug.Log("Unity iOS Support: App Tracking Transparency status not checked, because the platform is not iOS.");
            LoadNextScene(info);
#endif
        }

#if UNITY_IOS
        EIOSAppTrackingStatus Status(ATTrackingStatusBinding.AuthorizationTrackingStatus status) {
            return status switch {
                ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED => EIOSAppTrackingStatus.NotDetermined,
                ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED => EIOSAppTrackingStatus.Authorized,
                ATTrackingStatusBinding.AuthorizationTrackingStatus.DENIED => EIOSAppTrackingStatus.Denied,
                ATTrackingStatusBinding.AuthorizationTrackingStatus.RESTRICTED => EIOSAppTrackingStatus.Restricted,
                _ => throw new ArgumentOutOfRangeException(nameof(status), $"Unexpected status value {status}")
            };
        }

#endif

        void LoadNextScene(ConsentInfo info) {
            handler.SetConsentInfo(info);
            
            if(string.IsNullOrEmpty(NextSceneName)) {
                var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.buildIndex + 1);
            } else {
                UnityEngine.SceneManagement.SceneManager.LoadScene(NextSceneName);
            }
        }
    }   
}
