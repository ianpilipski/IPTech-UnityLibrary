using System;
using UnityEngine;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif

namespace IPTech.ConsentScreen {
    /// <summary>
    /// This component controls an iOS App Tracking Transparency context screen.
    /// You should only have one of these in your app.
    /// </summary>
    public sealed class ContextScreenView : MonoBehaviour
    {
        /// <summary>
        /// This event will be invoked after the ContinueButton is clicked
        /// and after the tracking authorization request has been sent.
        /// It's a good idea to subscribe to this event so you can destroy
        /// this GameObject to free up memory after it's no longer needed.
        /// Once the tracking authorization request has been sent, there's no
        /// need for this popup again until the app is uninstalled and reinstalled.
        /// </summary>
        public event Action sentTrackingAuthorizationRequest;

        public async void RequestAuthorizationTracking()
        {
            gameObject.SetActive(false);
            try {
#if UNITY_IOS
                Debug.Log("Unity iOS Support: Requesting iOS App Tracking Transparency native dialog.");

                bool firstTime = true;
                
                float nextCheck = 0;
                while(Application.isPlaying) {
                    if(Time.time > nextCheck) {
                        if(Application.isEditor) {
                            break;
                        } else {
                            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                            Debug.Log($"Checking ATTrackingStatus {status}");
                            if(status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED) {
                                if(firstTime) {
                                    ATTrackingStatusBinding.RequestAuthorizationTracking();
                                    firstTime = false;
                                }
                            } else {
                                break;
                            }
                        }
                        nextCheck = Time.time + 1F;
                    }
                    await System.Threading.Tasks.Task.Yield();
                }

                sentTrackingAuthorizationRequest?.Invoke();
#else
                await System.Threading.Tasks.Task.Yield();
                Debug.LogWarning("Unity iOS Support: Tried to request iOS App Tracking Transparency native dialog, " +
                                 "but the current platform is not iOS.");
                sentTrackingAuthorizationRequest?.Invoke();
#endif
            } catch(Exception e) {
                Debug.LogException(e);
            }
        }
    }
}
