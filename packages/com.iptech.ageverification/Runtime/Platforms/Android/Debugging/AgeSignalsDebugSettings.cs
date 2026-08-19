using UnityEngine;
using System;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    public static class AgeSignalsDebugSettings
    {
        private const string ENABLE_MOCK_MODE_KEY = "IPTech_AgeSignals_EnableMockMode";
        private const string CACHED_AGESIGNALSRESULT_KEY = "IPTech_AgeSignals_CachedAgeSignalsResult";
        private const string CACHED_AGESIGNALSACCESSRESULT_KEY = "IPTech_AgeSignals_CachedAgeSignalsAccessResult";

        private static CachedAgeSignalsResult _cachedAgeSignalsResultInstance;
        private static CachedAgeSignalsAccessResult _cachedAgeSignalsAccessResultInstance;
        
        public static CachedAgeSignalsResult LastAgeSignalsResult { get; set; }
        public static CachedAgeSignalsAccessResult LastAgeSignalsAccessResult { get; set; }

        public static event Action ValuesChanged;

        public static bool EnableMockMode
        {
            get
            {
                if (!Debug.isDebugBuild) return false;
                if (Application.isEditor) return true;
                return PlayerPrefs.GetInt(ENABLE_MOCK_MODE_KEY, 0) == 1;
            }

            set
            {
                PlayerPrefs.SetInt(ENABLE_MOCK_MODE_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
                ValuesChanged?.Invoke();
            }
        }

        public static CachedAgeSignalsResult CachedAgeSignalsResult
        {
            get
            {
                if (_cachedAgeSignalsResultInstance == null && PlayerPrefs.HasKey(CACHED_AGESIGNALSRESULT_KEY))
                {
                    var json = PlayerPrefs.GetString(CACHED_AGESIGNALSRESULT_KEY, null);
                    if (string.IsNullOrEmpty(json))
                    {
                        _cachedAgeSignalsResultInstance = null;
                    }
                    else
                    {
                        try
                        {
                            _cachedAgeSignalsResultInstance = JsonUtility.FromJson<CachedAgeSignalsResult>(json);
                        }
                        catch (Exception)
                        {
                            PlayerPrefs.DeleteKey(CACHED_AGESIGNALSRESULT_KEY);
                            _cachedAgeSignalsResultInstance = null;
                        }
                    }
                }
                return _cachedAgeSignalsResultInstance;
            }

            set
            {
                if (_cachedAgeSignalsResultInstance == value) return;
                _cachedAgeSignalsResultInstance = value;
                if (value == null)
                {
                    PlayerPrefs.DeleteKey(CACHED_AGESIGNALSRESULT_KEY);
                }
                else
                {
                    var json = JsonUtility.ToJson(value);
                    PlayerPrefs.SetString(CACHED_AGESIGNALSRESULT_KEY, json);
                }
                PlayerPrefs.Save();
                ValuesChanged?.Invoke();
            }
        }

        public static CachedAgeSignalsAccessResult CachedAgeSignalsAccessResult
        {
            get
            {
                if(_cachedAgeSignalsAccessResultInstance == null && PlayerPrefs.HasKey(CACHED_AGESIGNALSACCESSRESULT_KEY))
                {
                    var json = PlayerPrefs.GetString(CACHED_AGESIGNALSACCESSRESULT_KEY, null);
                    if (string.IsNullOrEmpty(json))
                    {
                        _cachedAgeSignalsAccessResultInstance = null;
                    }
                    else
                    {
                        try
                        {
                            _cachedAgeSignalsAccessResultInstance = JsonUtility.FromJson<CachedAgeSignalsAccessResult>(json);
                        }
                        catch (Exception)
                        {
                            PlayerPrefs.DeleteKey(CACHED_AGESIGNALSACCESSRESULT_KEY);
                            _cachedAgeSignalsAccessResultInstance = null;
                        }
                    }
                }
                return _cachedAgeSignalsAccessResultInstance;
            }

            set
            {
                if (_cachedAgeSignalsAccessResultInstance == value) return;
                _cachedAgeSignalsAccessResultInstance = value;
                if (value == null)
                {
                    PlayerPrefs.DeleteKey(CACHED_AGESIGNALSACCESSRESULT_KEY);
                }
                else
                {
                    var json = JsonUtility.ToJson(value);
                    PlayerPrefs.SetString(CACHED_AGESIGNALSACCESSRESULT_KEY, json);
                }
                PlayerPrefs.Save();
                ValuesChanged?.Invoke();
            }
        }
    }

    
}