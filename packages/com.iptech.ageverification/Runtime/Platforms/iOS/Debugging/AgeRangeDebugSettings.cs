
using UnityEngine;
using System;

namespace IPTech.AgeVerification.iOS.Debugging
{
    public static class AgeRangeDebugSettings
    {
        private const string ENABLE_MOCK_MODE_KEY = "IPTech_AgeRange_EnableMockMode";
        private const string CACHED_RESULT_KEY = "IPTech_AgeRange_CachedResult";
        private const string CACHED_IS_ELIGIBLE_RESULT_KEY = "IPTech_AgeRange_CachedIsEligibleResult";

        private static CachedResult _cachedResultInstance;
        private static CachedIsEligibleResult _cachedIsEligibleResultInstance;
        
        public static CachedResult LastResult { get; set; }
        public static CachedIsEligibleResult LastIsEligibleForAgeFeaturesResult { get; set; }

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

        public static CachedIsEligibleResult CachedIsEligibleForAgeFeaturesResult
        {
            get {
                if(_cachedIsEligibleResultInstance == null && PlayerPrefs.HasKey(CACHED_IS_ELIGIBLE_RESULT_KEY))
                {
                    var json = PlayerPrefs.GetString(CACHED_IS_ELIGIBLE_RESULT_KEY,null);
                    if(!string.IsNullOrWhiteSpace(json))
                    {
                        try
                        {
                            _cachedIsEligibleResultInstance = JsonUtility.FromJson<CachedIsEligibleResult>(json);
                        }
                        catch(Exception)
                        {
                            // If deserialization fails, clear the invalid cached value
                            PlayerPrefs.DeleteKey(CACHED_IS_ELIGIBLE_RESULT_KEY);
                            _cachedIsEligibleResultInstance = null;
                        }
                    }
                }
                return _cachedIsEligibleResultInstance;
            }
            
            set
            {
                if(_cachedIsEligibleResultInstance == value) return;
                _cachedIsEligibleResultInstance = value;
                if (value == null)
                {
                    PlayerPrefs.DeleteKey(CACHED_IS_ELIGIBLE_RESULT_KEY);
                }
                else
                {
                    PlayerPrefs.SetString(CACHED_IS_ELIGIBLE_RESULT_KEY, JsonUtility.ToJson(value));
                }
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
                            // If deserialization fails, clear the invalid cached value
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