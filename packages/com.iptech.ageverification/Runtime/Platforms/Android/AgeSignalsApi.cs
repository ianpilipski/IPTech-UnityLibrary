
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using IPTech.AgeVerification.Android.AgeSignals.Debugging;

namespace IPTech.AgeVerification.Android.AgeSignals
{
    public interface IAgeSignalsApi
    {
        Task<AgeSignalsResult> RequestAgeSignals(CancellationToken ct);
    }

    public class AgeSignalsApi : IAgeSignalsApi
    {
        public bool EnableMockMode
        {
            get => AgeSignalsDebugSettings.EnableMockMode;
            set => AgeSignalsDebugSettings.EnableMockMode = value;
        }

        public async Task<AgeSignalsResult> RequestAgeSignals(CancellationToken ct)
        {
#if UNITY_ANDROID || UNITY_EDITOR
            if (EnableMockMode)
            {
                return await DebugRecordResult(Debugging.MockResultPopUp.ShowDialog(ct));
            }

            if (!Application.isEditor)
            {
                var tcs = new TaskCompletionSource<AgeSignalsResult>();
                using var reg = ct.Register(() => tcs.TrySetCanceled());
                var res = await Task.WhenAny(Internal.IPTechAgeSignalsUnityPlugin.RequestAgeSignals(), tcs.Task);
                if (res != tcs.Task)
                {
                    return await DebugRecordResult(res);
                } 
                return await tcs.Task;
            }

            async Task<AgeSignalsResult> DebugRecordResult(Task<AgeSignalsResult> result)
            {
                try
                {
                    var res = await result;
                    var cachedResult = new Debugging.CachedResult(res);
                    AgeSignalsDebugSettings.LastResult = cachedResult;
                    return res;
                }
                catch (Exception ex)
                {
                    var cachedError = new Debugging.CachedError(ex);
                    var cachedResult = new Debugging.CachedResult(cachedError);
                    AgeSignalsDebugSettings.LastResult = cachedResult;
                    throw;
                }
            }
#else
            await Task.CompletedTask; // avoid compiler error
#endif
            throw new PlatformNotSupportedException("Age Signals API is not supported on this platform.");
        }
    }
}