using System;
using UnityEngine;

namespace IPTech.ConsentScreen {
    public class PlayerPrefsConsentScreenHandler : ConsentScreenHandler {
        const string Key = "iptech.consentscreen.consentinfo";

        public override ConsentInfo GetCurrentConsentInfo() {
            return ConsentInfo;
        }

        public override void SetConsentInfo(ConsentInfo info) {
            ConsentInfo = info;
        }

        public static ConsentInfo ConsentInfo {
            get {
                return new ConsentInfo() {
                    Consent = PlayerPrefsConsentValue,
                    AgeInfo = PlayerPrefsConsentAge,
                    IOSAppTrackingStatus = PlayerPrefsAppTrackingStatus
                };
            }
            set {
                PlayerPrefsConsentAge = value.AgeInfo;
                PlayerPrefsAppTrackingStatus = value.IOSAppTrackingStatus;
                PlayerPrefsConsentValue = value.Consent;
            }
        }

        static EConsentValue PlayerPrefsConsentValue {
            get => GetEnumFromPlayerPrefs<EConsentValue>($"{Key}.consent", EConsentValue.Unknown);
            set => PlayerPrefs.SetString($"{Key}.consent", value.ToString());
        }

        static EConsentAge PlayerPrefsConsentAge {
            get => GetEnumFromPlayerPrefs<EConsentAge>($"{Key}.age", EConsentAge.Unknown);
            set => PlayerPrefs.SetString($"{Key}.age", value.ToString());
        }

        static EIOSAppTrackingStatus PlayerPrefsAppTrackingStatus {
            get => GetEnumFromPlayerPrefs<EIOSAppTrackingStatus>($"{Key}.iosapptrackingstatus", EIOSAppTrackingStatus.NotDetermined);
            set => PlayerPrefs.SetString($"{Key}.iosapptrackingstatus", value.ToString());
        }

        static T GetEnumFromPlayerPrefs<T>(string key, T defaultValue) where T : Enum {
            var s = PlayerPrefs.GetString(key, "");
            if(!string.IsNullOrWhiteSpace(s)) {
                if(Enum.TryParse(typeof(T), s, out object value)) {
                    return (T)value;
                }
            }
            return defaultValue;
        }
    }
}
