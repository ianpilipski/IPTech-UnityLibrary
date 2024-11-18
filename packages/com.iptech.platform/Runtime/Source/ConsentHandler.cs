using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.Platform.Internal {
    public interface IConsentHandler {
        event Action<ConsentInfo> ConsentValueChanged;
        ConsentInfo Consent { get; set; }
    }

    public class ConsentHandler : IConsentHandler {
        const string CONSENT_KEY = "_iptech_platform_consentvalue";

        public event Action<ConsentInfo> ConsentValueChanged;

        public ConsentInfo Consent {
            get {
                var s = PlayerPrefs.GetString(CONSENT_KEY, "");
                if(!string.IsNullOrWhiteSpace(s)) {
                    try {
                        return JsonUtility.FromJson<ConsentInfo>(s);
                    } catch(Exception e) {
                        Debug.LogException(e);
                    }
                }
                return new ConsentInfo();
            }

            set {
                if(!value.Equals(Consent)) {
                    PlayerPrefs.SetString(CONSENT_KEY, JsonUtility.ToJson(value));
                    PlayerPrefs.Save();
                    ConsentValueChanged?.Invoke(value);
                }
            }
        }
    }
} 
