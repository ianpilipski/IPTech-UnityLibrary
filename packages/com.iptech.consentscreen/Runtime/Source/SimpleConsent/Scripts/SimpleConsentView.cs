using System;
using UnityEngine;

namespace IPTech.ConsentScreen {
    public class SimpleConsentView : MonoBehaviour
    {
        public event Action simpleConsentAccepted;
        
        public string TOSUrl;
        public string PrivacyPolicyUrl;

        public void Accept() {
            simpleConsentAccepted?.Invoke();
        }

        public void OpenTOS() {
            Application.OpenURL(TOSUrl);
        }

        public void OpenPrivacyPolicy() {
            Application.OpenURL(PrivacyPolicyUrl);
        }
    }
}
