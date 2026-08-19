
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using IPTech.AgeVerification.Android.AgeSignals.Debugging;

namespace IPTech.AgeVerification.Android.AgeSignals
{
    public interface IAgeSignalsApi
    {
        Task<AgeSignalsAccessResult> RequestAgeSignalsAccess(CancellationToken ct);
        Task<AgeSignalsResult> CheckAgeSignals(CancellationToken ct);
    }

    public class AgeSignalsApi : IAgeSignalsApi
    {
        public bool EnableMockMode
        {
            get => AgeSignalsDebugSettings.EnableMockMode;
            set => AgeSignalsDebugSettings.EnableMockMode = value;
        }

        public async Task<AgeSignalsAccessResult> RequestAgeSignalsAccess(CancellationToken ct)
        {
            #if UNITY_ANDROID || UNITY_EDITOR
            if(EnableMockMode)
            {
                return await DebugRecordResult(Debugging.MockAgeSignalsAccessResultPopUp.ShowDialog(ct));
            }

            if(!Application.isEditor)
            {
                var tcs = new TaskCompletionSource<AgeSignalsAccessResult>();
                using var reg = ct.Register(() => tcs.TrySetCanceled());
                var res = await Task.WhenAny(Internal.IPTechAgeSignalsUnityPlugin.RequestAgeSignalsAccess(), tcs.Task);
                if (res != tcs.Task)
                {
                    return await DebugRecordResult(res);
                }
                return await tcs.Task;
            }
            #else
            await Task.CompletedTask;
            #endif
            throw new PlatformNotSupportedException("Age Signals API is not supported on this platform.");
        }

        public async Task<AgeSignalsResult> CheckAgeSignals(CancellationToken ct)
        {
#if UNITY_ANDROID || UNITY_EDITOR
            if (EnableMockMode)
            {
                return await DebugRecordResult(Debugging.MockAgeSignalsResultPopUp.ShowDialog(ct));
            }

            if (!Application.isEditor)
            {
                var tcs = new TaskCompletionSource<AgeSignalsResult>();
                using var reg = ct.Register(() => tcs.TrySetCanceled());
                var res = await Task.WhenAny(Internal.IPTechAgeSignalsUnityPlugin.CheckAgeSignals(), tcs.Task);
                if (res != tcs.Task)
                {
                    return await DebugRecordResult(res);
                } 
                return await tcs.Task;
            }

            
#else
            await Task.CompletedTask; // avoid compiler error
#endif
            throw new PlatformNotSupportedException("Age Signals API is not supported on this platform.");
        }

        private async Task<AgeSignalsResult> DebugRecordResult(Task<AgeSignalsResult> result)
        {
            try
            {
                var res = await result;
                var cachedResult = new Debugging.CachedAgeSignalsResult(res);
                AgeSignalsDebugSettings.LastAgeSignalsResult = cachedResult;
                return res;
            }
            catch (Exception ex)
            {
                var cachedError = new Debugging.CachedError(ex);
                var cachedResult = new Debugging.CachedAgeSignalsResult(cachedError);
                AgeSignalsDebugSettings.LastAgeSignalsResult = cachedResult;
                throw;
            }
        }

        private async Task<AgeSignalsAccessResult> DebugRecordResult(Task<AgeSignalsAccessResult> result)
        {
            try
            {
                var res = await result;
                var cachedResult = new Debugging.CachedAgeSignalsAccessResult(res);
                AgeSignalsDebugSettings.LastAgeSignalsAccessResult = cachedResult;
                return res;
            }
            catch (Exception ex)
            {
                var cachedError = new Debugging.CachedError(ex);
                var cachedResult = new Debugging.CachedAgeSignalsAccessResult(cachedError);
                AgeSignalsDebugSettings.LastAgeSignalsAccessResult = cachedResult;
                throw;
            }
        }
    }
}