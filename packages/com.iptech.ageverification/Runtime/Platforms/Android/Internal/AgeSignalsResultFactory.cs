
#if UNITY_ANDROID || UNITY_EDITOR
using System;
using UnityEngine;

namespace IPTech.AgeVerification.Android.AgeSignals.Internal
{
    internal static class AgeSignalsResultFactory
    {
        internal static AgeSignalsResult CreateAgeSignalResultFromJavaObject(AndroidJavaObject javaResult)
        {
            var res = new AgeSignalsResult();
            if (javaResult != null)
            {
                try
                {
                    res.UserStatus = (AgeSignalsVerificationStatus?)GetNullableJavaProperty<int>(javaResult, "getUserStatus");
                    res.AgeLower = GetNullableJavaProperty<int>(javaResult, "getAgeLower");
                    res.AgeUpper = GetNullableJavaProperty<int>(javaResult, "getAgeUpper");
                    var dateObj = javaResult.Call<AndroidJavaObject>("getMostRecentApprovalDate");
                    if (!IsJavaObjectNull(dateObj))
                    {
                        long approvalDateMillis = dateObj.Call<long>("getTime");
                        res.MostRecentApprovalDate = DateTimeOffset.FromUnixTimeMilliseconds(approvalDateMillis).DateTime;
                    }
                    res.InstallId = javaResult.Call<string>("getInstallId");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing AgeSignalsResult: {e.Message}");
                }
            }
            return res;
        }

        internal static AgeSignalsAccessResult CreateAgeSignalsAccessResultFromJavaObject(AndroidJavaObject javaResult)
        {
            var res = new AgeSignalsAccessResult();
            if (javaResult != null)
            {
                try
                {
                    res.AgeSignalsAccessStatus = (AgeSignalsAccessStatus?)GetNullableJavaProperty<int>(javaResult, "getAgeSignalsStatus");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing AgeSignalsAccessResult: {e.Message}");
                }
            }
            return res;
        }

        

        private static T? GetNullableJavaProperty<T>(AndroidJavaObject javaResult, string methodName) where T : struct
        {
            try
            {
                var objRes = javaResult.Call<AndroidJavaObject>(methodName);

                if (!IsJavaObjectNull(objRes))
                {
                    string primitiveMethod = typeof(T) switch
                    {
                        Type t when t == typeof(int) => "intValue",
                        Type t when t == typeof(long) => "longValue",
                        Type t when t == typeof(bool) => "booleanValue",
                        Type t when t == typeof(float) => "floatValue",
                        Type t when t == typeof(double) => "doubleValue",
                        _ => null
                    };
                    if(primitiveMethod != null)
                    {
                        return objRes.Call<T>(primitiveMethod);
                    }
                    return objRes.Call<T>("valueOf");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading nullable Java property '{methodName}': {e.Message}");
            }
            return null;
        }
        
        private static bool IsJavaObjectNull(AndroidJavaObject obj)
        {
            return obj == null || obj.GetRawObject() == IntPtr.Zero;
        }
    }
}
#endif