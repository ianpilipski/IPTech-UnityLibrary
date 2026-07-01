#if UNITY_ANDROID || UNITY_EDITOR
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace IPTech.AgeVerification.Android.AgeSignals.Internal
{
    public static class IPTechAgeSignalsUnityPlugin
    {
        private const string JAVA_CLASS_NAME = "com.IPTech.AgeVerification.Android.agesignals.IPTechAgeSignalsUnityPlugin";
        private const string CALLBACK_INTERFACE_NAME = "com.IPTech.AgeVerification.Android.agesignals.IPTechAgeSignalsUnityPlugin$IRequestAgeSignalsCallback";

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

        public static Task<AgeSignalsResult> RequestAgeSignals()
        {
            try
            {
                AssertAndroidRuntimePlatform();
                AssertJavaClassInitialized();

                var callback = new RequestAgeSignalsCallbackProxy();
                _javaClass.CallStatic("requestAgeSignals", callback);
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

        private class RequestAgeSignalsCallbackProxy : AndroidJavaProxy 
        {
            private readonly TaskCompletionSource<AgeSignalsResult> _tcs = new TaskCompletionSource<AgeSignalsResult>();

            public RequestAgeSignalsCallbackProxy() : base(CALLBACK_INTERFACE_NAME)
            {
            }

            public void onSuccess(AndroidJavaObject result)
            {
                try
                {
                    var ageSignalsResult = AgeSignalResultFactory.CreateFromJavaObject(result);
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