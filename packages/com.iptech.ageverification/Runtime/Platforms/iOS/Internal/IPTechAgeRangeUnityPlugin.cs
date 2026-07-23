
#if UNITY_IOS || UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.AgeVerification.iOS.Internal
{

    internal static class IPTechAgeRangeUnityPlugin
    {
        private static int staticCallerId = 0;
        private static readonly object _lockObject = new object();

        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool IPTechAgeRangeUnityPlugin_isEligibleForAgeFeatures(int callerId, IsEligibleForAgeFeaturesCallback callback);
        private delegate void IsEligibleForAgeFeaturesCallback(int callerId, int isEligible, IntPtr errorMsgPtr);
        private static event Action<IsEligibleForAgeFeaturesCallbackResult> IsEligibleForAgeFeaturesCallbackHasBeenCalled;

        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void IPTechAgeRangeUnityPlugin_getAgeRange(int callerId, int requiredMinAge, int additionalMinAge1, int additionalMinAge2, RequestAgeRangeCallback callback);
        private delegate void RequestAgeRangeCallback(int callerId, int callStatus, int lowerBound, int upperBound, int ageStatus, IntPtr errorMsgPtr);
        private static event Action<RequestAgeRangeCallbackResult> AgeRangeCallbackHasBeenCalled;


        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void IPTechAgeRangeUnityPlugin_freeString(IntPtr strPtr);

        
        [AOT.MonoPInvokeCallback(typeof(IsEligibleForAgeFeaturesCallback))]
        private static void HandleIsEligibleForAgeFeaturesCallback(int callerId, int isEligible, IntPtr errorMsgPtr)
        {
            try
            {
                string errorMsg = MarshalPtrToStringAndFree(errorMsgPtr);
                var result = new IsEligibleForAgeFeaturesCallbackResult(callerId, isEligible, errorMsg);
                IsEligibleForAgeFeaturesCallbackHasBeenCalled?.Invoke(result);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in HandleIsEligibleForAgeFeaturesCallback: {ex}");
            }
        }

        [AOT.MonoPInvokeCallback(typeof(RequestAgeRangeCallback))]
        private static void HandleRequestAgeRangeCallback(int callerId, int callStatus, int lowerBound, int upperBound, int ageStatus, IntPtr errorMsgPtr)
        {
            try
            {
                string errorMsg = MarshalPtrToStringAndFree(errorMsgPtr);
                var result = new RequestAgeRangeCallbackResult(
                    callerId, (RequestAgeRangeCallbackResult.CallStatus)callStatus, lowerBound, upperBound, ageStatus, errorMsg);
                AgeRangeCallbackHasBeenCalled?.Invoke(result);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in HandleRequestAgeRangeCallback: {ex}");
            }
        }

        private static string MarshalPtrToStringAndFree(IntPtr strPtr)
        {
            if (strPtr == IntPtr.Zero) return null;

            string result = Marshal.PtrToStringUTF8(strPtr);
            IPTechAgeRangeUnityPlugin_freeString(strPtr);
            return result;
        }

        public static async Task<bool> IsEligibleForAgeFeatures(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            var callerId = 0;
            var tcs = new TaskCompletionSource<bool>();
            lock (_lockObject)
            {
                callerId = staticCallerId++;
                IsEligibleForAgeFeaturesCallbackHasBeenCalled += OnIsEligibleForAgeFeaturesCallback;
            }
            
            try 
            {
                using (ct.Register(() => tcs.TrySetCanceled()))
                {
                    IPTechAgeRangeUnityPlugin_isEligibleForAgeFeatures(callerId, HandleIsEligibleForAgeFeaturesCallback);
                    return await tcs.Task;
                }
            }
            finally
            {
                lock (_lockObject)
                {
                    IsEligibleForAgeFeaturesCallbackHasBeenCalled -= OnIsEligibleForAgeFeaturesCallback;
                }
            }
        
            void OnIsEligibleForAgeFeaturesCallback(IsEligibleForAgeFeaturesCallbackResult result)
            {
                if (result.CallerId != callerId) return;
                try
                {
                    if (string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        tcs.TrySetResult(result.IsEligible);
                    }
                    else
                    {
                        if(result.ErrorMessage == "notAvailable")
                        {
                            tcs.TrySetException(new AgeRangeNotAvailableException());
                        }
                        else if(result.ErrorMessage == "platformNotSupported")
                        {
                            tcs.TrySetException(new PlatformNotSupportedException());
                        }
                        else
                        {
                            tcs.TrySetException(new Exception(result.ErrorMessage));
                        }
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }
        }

        // Public method to request age range
        public static async Task<AgeRangeResult> RequestAgeRange(int requiredMinAge, CancellationToken ct, int additionalMinAge1 = 0, int additionalMinAge2 = 0)
        {
            var callerId = 0;
            var tcs = new TaskCompletionSource<AgeRangeResult>();
            lock (_lockObject)
            {
                callerId = staticCallerId++;
                AgeRangeCallbackHasBeenCalled += OnAgeRangeCallback;
            }
            try
            {
                using (ct.Register(() => tcs.TrySetCanceled()))
                {
                    IPTechAgeRangeUnityPlugin_getAgeRange(callerId, requiredMinAge, additionalMinAge1, additionalMinAge2, HandleRequestAgeRangeCallback);
                    return await tcs.Task;
                }
            }
            finally
            {
                lock (_lockObject)
                {
                    AgeRangeCallbackHasBeenCalled -= OnAgeRangeCallback;
                }
            }

            void OnAgeRangeCallback(RequestAgeRangeCallbackResult result)
            {
                if (result.CallerId != callerId) return;
                try
				{
					if (result.Status == RequestAgeRangeCallbackResult.CallStatus.Success)
					{
						// the ios sdk will return nil for these values, we use -1 to indicate nil in the bridge code
						int? lowerBound = result.LowerBound < 0 ? null : result.LowerBound;
						int? upperbound = result.UpperBound < 0 ? null : result.UpperBound;

						var ageRangeResult = new AgeRangeResult(AgeRangeResultStatus.Success, lowerBound, upperbound, result.AgeDeclaration);
						tcs.TrySetResult(ageRangeResult);
					}
					else if (result.Status == RequestAgeRangeCallbackResult.CallStatus.UserDeclined)
					{
						var ageRangeResult = new AgeRangeResult(AgeRangeResultStatus.UserDeclined, null, null, result.AgeDeclaration);
						tcs.TrySetResult(ageRangeResult);
					}
					else if (result.Status == RequestAgeRangeCallbackResult.CallStatus.UnsupportedPlatformVersion)
					{
						tcs.TrySetResult(new AgeRangeResult(AgeRangeResultStatus.UnsupportedPlatformVersion, null, null, AgeDeclaration.Unknown));
					}
					else if(result.Status == RequestAgeRangeCallbackResult.CallStatus.Error && result.ErrorMessage == "notAvailable")
					{
						tcs.TrySetException(new AgeRangeNotAvailableException());
					}
					else if(result.Status == RequestAgeRangeCallbackResult.CallStatus.Error && result.ErrorMessage == "invalidRequest")
					{
						tcs.TrySetException(new AgeRangeInvalidRequestException());
					}
					else
					{
						tcs.TrySetException(new Exception(result.ErrorMessage));
					}
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }
        }

        private class IsEligibleForAgeFeaturesCallbackResult
        {
            public int CallerId { get; set; }
            public bool IsEligible { get; }
            public string ErrorMessage { get; }

            public IsEligibleForAgeFeaturesCallbackResult(int callerId, int isEligibleCode, string errorMessage)
            {
                CallerId = callerId;
                ErrorMessage = errorMessage;
                if(isEligibleCode < 0)
                {
                    IsEligible = false;
                    if(isEligibleCode == -2)
                    {
                        ErrorMessage = "platformNotSupported";
                    } 
                    else if(string.IsNullOrWhiteSpace(ErrorMessage))
                    {
                        ErrorMessage = "Unknown error";
                    }
                }
                else
                {
                    IsEligible = isEligibleCode == 1;
                }
            }
        }

        private class RequestAgeRangeCallbackResult
        {
            public int CallerId { get; set; }
            public CallStatus Status { get; }

            public int? LowerBound { get; }
            public int? UpperBound { get; }
            public AgeDeclaration AgeDeclaration { get; }
            public string ErrorMessage { get; }


            // These values match what is defined in the IPTechAgeRangeUnityPlugin.swift file
            public enum CallStatus
            {
                Error = 0,
                Success = 1,
                UserDeclined = 2,
                UnsupportedPlatformVersion = 3
            }

            public RequestAgeRangeCallbackResult(int callerId, CallStatus status, int lowerBound, int upperBound, int ageStatus, string errorMessage)
            {
                CallerId = callerId;
                Status = status;
                LowerBound = lowerBound;
                UpperBound = upperBound;
                AgeDeclaration = (AgeDeclaration)ageStatus;
                ErrorMessage = errorMessage;
            }
        }
    }
}
#endif