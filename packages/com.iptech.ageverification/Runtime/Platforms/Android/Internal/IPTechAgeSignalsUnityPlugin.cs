#if UNITY_ANDROID || UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.AgeVerification.Android.AgeSignals.Internal
{
    public static class IPTechAgeSignalsUnityPlugin
    {
        private const string JAVA_CLASS_NAME = "com.IPTech.AgeVerification.Android.agesignals.IPTechAgeSignalsUnityPlugin";
        private const string CHECKAGESIGNALS_CALLBACK_INTERFACE_NAME = "com.IPTech.AgeVerification.Android.agesignals.IPTechAgeSignalsUnityPlugin$ICheckAgeSignalsCallback";
        private const string REQUESTAGESIGNALSACCESS_CALLBACK_INTERFACE_NAME = "com.IPTech.AgeVerification.Android.agesignals.IPTechAgeSignalsUnityPlugin$IRequestAgeSignalsAccessCallback";

        private static AndroidJavaClass _javaClass;

        static IPTechAgeSignalsUnityPlugin()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                try
                {
                    _javaClass = new AndroidJavaClass(JAVA_CLASS_NAME);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{nameof(IPTechAgeSignalsUnityPlugin)}: Failed to initialize IPAgeSignalsUnityPlugin Java class: {e.Message}");
                }
            }
        }

        public static Task<AgeSignalsAccessResult> RequestAgeSignalsAccess()
        {
            try
            {
                AssertAndroidRuntimePlatform();
                AssertJavaClassInitialized();

                var callback = new RequestAgeSignalsAccessCallbackProxy();
                _javaClass.CallStatic("requestAgeSignalsAccess", callback);
                return callback.Task;
            }
            catch (Exception e)
            {
                return Task.FromException<AgeSignalsAccessResult>(e);
            }
        }

        public static Task<AgeSignalsResult> CheckAgeSignals()
        {
            try
            {
                AssertAndroidRuntimePlatform();
                AssertJavaClassInitialized();

                var callback = new CheckAgeSignalsCallbackProxy();
                _javaClass.CallStatic("checkAgeSignals", callback);
                return callback.Task;
            }
            catch (Exception e)
            {
                return Task.FromException<AgeSignalsResult>(e);
            }
        }

        private static void AssertAndroidRuntimePlatform()
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                throw new PlatformNotSupportedException($"{nameof(IPTechAgeSignalsUnityPlugin)} is only supported on Android platform.");
            }
        }

        private static void AssertJavaClassInitialized()
        {
            if (_javaClass == null)
            {
                throw new InvalidOperationException($"{nameof(IPTechAgeSignalsUnityPlugin)} Java class is not initialized.");
            }
        }

        private class RequestAgeSignalsAccessCallbackProxy : AndroidJavaProxy
        {
            private readonly TaskCompletionSource<AgeSignalsAccessResult> _tcs = new TaskCompletionSource<AgeSignalsAccessResult>();

            public RequestAgeSignalsAccessCallbackProxy() : base(REQUESTAGESIGNALSACCESS_CALLBACK_INTERFACE_NAME)
            {
            }

            public void onSuccess(AndroidJavaObject result)
            {
                var ageSignalsAccessResult = AgeSignalsResultFactory.CreateAgeSignalsAccessResultFromJavaObject(result);
                _tcs.TrySetResult(ageSignalsAccessResult);
            }

            public void onFailure(int errorCode, string errorMsg) => _tcs.TrySetException(new AgeSignalsException(errorCode, errorMsg));
            public void onCancel() => _tcs.TrySetCanceled();

            public Task<AgeSignalsAccessResult> Task => _tcs.Task;
        }

        private class CheckAgeSignalsCallbackProxy : AndroidJavaProxy 
        {
            private readonly TaskCompletionSource<AgeSignalsResult> _tcs = new TaskCompletionSource<AgeSignalsResult>();

            public CheckAgeSignalsCallbackProxy() : base(CHECKAGESIGNALS_CALLBACK_INTERFACE_NAME)
            {
            }

            public void onSuccess(AndroidJavaObject result)
            {
                try
                {
                    var ageSignalsResult = AgeSignalsResultFactory.CreateAgeSignalResultFromJavaObject(result);
                    _tcs.TrySetResult(ageSignalsResult);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{nameof(IPTechAgeSignalsUnityPlugin)}: Error in onSuccess callback: {e.Message}");
                    onFailure(0, $"Error processing success result: {e.Message}");
                }
            }

            public void onFailure(int errorCode, string errorMsg) => _tcs.TrySetException(new AgeSignalsException(errorCode, errorMsg));
            public void onCancel() => _tcs.TrySetCanceled();

            public Task<AgeSignalsResult> Task => _tcs.Task;
        }
    }
}
#endif