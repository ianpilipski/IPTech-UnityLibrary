
using System;
using System.Threading;
using System.Threading.Tasks;
using IPTech.AgeVerification.iOS.Debugging;
using UnityEngine;

namespace IPTech.AgeVerification.iOS
{
    public interface IAgeRangeApi
    {
        Task<bool> IsEligibleForAgeFeatures(CancellationToken ct);
        Task<AgeRangeResult> RequestAgeRange(int requiredMinAge,  CancellationToken ct, int additionalMinAge1 = 0, int additionalMinAge2 = 0);
    }

    public class AgeRangeApi : IAgeRangeApi
    {
        public static bool EnableMockMode
        {
            get => AgeRangeDebugSettings.EnableMockMode;
            set => AgeRangeDebugSettings.EnableMockMode = value;
        }

        public async Task<bool> IsEligibleForAgeFeatures(CancellationToken ct)
        {
#if UNITY_IOS || UNITY_EDITOR
            if (EnableMockMode)
            {
                return await DebugRecordResult(MockIsEligiblePopUp.ShowDialog(ct));
            }

            if (!Application.isEditor)
            {
                return await DebugRecordResult(Internal.IPTechAgeRangeUnityPlugin.IsEligibleForAgeFeatures(ct));
            }

            async Task<bool> DebugRecordResult(Task<bool> result)
            {
                try
                {
                    var res =  await result;
                    AgeRangeDebugSettings.LastIsEligibleForAgeFeaturesResult = new CachedIsEligibleResult(res);
                    return res;
                }
                catch (Exception ex)
                {
                    var cachedError = new CachedIsEligibleError(ex);
                    AgeRangeDebugSettings.LastIsEligibleForAgeFeaturesResult = new CachedIsEligibleResult(cachedError);
                    throw;
                }
            }
#else
            await Task.CompletedTask; // avoid compiler warnings
#endif
            throw new PlatformNotSupportedException("Age Range API is only supported on iOS devices.");
        }

        public async Task<AgeRangeResult> RequestAgeRange(int requiredMinAge, CancellationToken ct, int additionalMinAge1 = 0, int additionalMinAge2 = 0)
        {
#if UNITY_IOS || UNITY_EDITOR
            ValidateAgeGates(requiredMinAge, additionalMinAge1, additionalMinAge2);
            if (EnableMockMode)
            {
                return await DebugRecordResult(MockResultPopUp.ShowDialog(requiredMinAge, ct, additionalMinAge1, additionalMinAge2));
            }

            if (!Application.isEditor)
            {
                return await DebugRecordResult(Internal.IPTechAgeRangeUnityPlugin.RequestAgeRange(requiredMinAge, ct, additionalMinAge1, additionalMinAge2));
            }

            async Task<AgeRangeResult> DebugRecordResult(Task<AgeRangeResult> result)
            {
                try
                {
                    var res = await result;
                    var cachedResult = new CachedResult(res);
                    AgeRangeDebugSettings.LastResult = cachedResult;
                    return res;
                }
                catch (Exception ex)
                {
                    var cachedError = new CachedError(ex);
                    var cachedResult = new CachedResult(cachedError);
                    AgeRangeDebugSettings.LastResult = cachedResult;
                    throw;
                }
            }
#else
            await Task.CompletedTask; // avoid compiler warnings
#endif
            throw new PlatformNotSupportedException("Age Range API is only supported on iOS devices.");
        }

        private static void ValidateAgeGates(int requiredMinAge, int additionalMinAge1, int additionalMinAge2)
        {
            const int maxAgeGate = 18;
            ValidateSingleAgeGate(requiredMinAge, nameof(requiredMinAge));
            if (additionalMinAge1 > 0) ValidateSingleAgeGate(additionalMinAge1, nameof(additionalMinAge1));
            if (additionalMinAge2 > 0) ValidateSingleAgeGate(additionalMinAge2, nameof(additionalMinAge2));

            static void ValidateSingleAgeGate(int value, string paramName)
            {
                if (value < 1 || value > maxAgeGate)
                    throw new AgeRangeInvalidRequestException(
                        $"iOS Declarative Age Range only supports age gates between 1 and {maxAgeGate}. " +
                        $"Received {paramName}={value}. For unrestricted content thresholds above {maxAgeGate}, " +
                        $"handle this in your game's callback logic after receiving the age range result.");
            }
        }
    }
}