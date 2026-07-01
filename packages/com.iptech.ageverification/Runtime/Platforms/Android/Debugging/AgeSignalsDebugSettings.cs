using UnityEngine;
using System;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    public static class AgeSignalsDebugSettings
    {
        private const string ENABLE_MOCK_MODE_KEY = "IPTech_AgeSignals_EnableMockMode";
        private const string CACHED_RESULT_KEY = "IPTech_AgeSignals_CachedResult";
        private static CachedResult _cachedResultInstance;
        
        public static CachedResult LastResult { get; set; }

        public static bool EnableMockMode
        {
            get
            {
                if (!Debug.isDebugBuild) return false;
                if (Application.isEditor)
                {
                    return true;
                }
                return PlayerPrefs.GetInt(ENABLE_MOCK_MODE_KEY, 0) == 1;
            }

            set
            {
                PlayerPrefs.SetInt(ENABLE_MOCK_MODE_KEY, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static CachedResult CachedResult
        {
            get
            {
                if (_cachedResultInstance == null && PlayerPrefs.HasKey(CACHED_RESULT_KEY))
                {
                    var json = PlayerPrefs.GetString(CACHED_RESULT_KEY, null);
                    if (string.IsNullOrEmpty(json))
                    {
                        _cachedResultInstance = null;
                    }
                    else
                    {
                        try
                        {
                            _cachedResultInstance = JsonUtility.FromJson<CachedResult>(json);
                        }
                        catch (Exception)
                        {
                            PlayerPrefs.DeleteKey(CACHED_RESULT_KEY);
                            _cachedResultInstance = null;
                        }
                    }
                }
                return _cachedResultInstance;
            }

            set
            {
                if (_cachedResultInstance == value) return;
                _cachedResultInstance = value;
                if (value == null)
                {
                    PlayerPrefs.DeleteKey(CACHED_RESULT_KEY);
                }
                else
                {
                    var json = JsonUtility.ToJson(value);
                    PlayerPrefs.SetString(CACHED_RESULT_KEY, json);
                }
                PlayerPrefs.Save();
            }
        }
    }
}