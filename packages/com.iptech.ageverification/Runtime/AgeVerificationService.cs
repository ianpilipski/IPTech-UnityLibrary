
using System;
using System.Threading;
using System.Threading.Tasks;
using IPTech.AgeVerification.Android.AgeSignals;
using IPTech.AgeVerification.iOS;
using UnityEngine;

namespace IPTech.AgeVerification
{
    public class AgeVerificationService : IAgeVerificationService
    {
        public async Task<AgeVerificationResult> RequestAgeData(int requiredMinAge, CancellationToken ct, int additionalMinAge1 = 0, int additionalMinAge2 = 0)
        {
            if (IsIOS())
            {
                var api = new AgeRangeApi();
                var isEligible = await api.IsEligibleForAgeFeatures(ct);
                if(!isEligible)
                {
                    return new AgeVerificationResult
                    {
                        Status = AgeVerificationStatus.AgeRangeNotRequired
                    };
                }
                
                var res = await api.RequestAgeRange(requiredMinAge, ct, additionalMinAge1, additionalMinAge2);

                if (res.Status == AgeRangeResultStatus.Success)
                {
                    return new AgeVerificationResult
                    {
                        Status = AgeVerificationStatus.HasAgeRange,
                        LowerBound = res.LowerBound,
                        UpperBound = res.UpperBound,
                        MostRecentApprovalDate = DateTime.Now
                    };
                }

                if (res.Status == AgeRangeResultStatus.UserDeclined)
                {
                    Debug.Log("User declined age range request.");
                }
                if (res.Status == AgeRangeResultStatus.UnsupportedPlatformVersion)
                {
                    Debug.LogWarning("Age Verification is not supported on this iOS version.");
                }
                if (res.Status == AgeRangeResultStatus.Error)
                {
                    Debug.LogError("Age Verification failed with an error.");
                }
                // AgeRangeUnavailable Error or unknown status
                return new AgeVerificationResult
                {
                    Status = AgeVerificationStatus.Unknown
                };
            }
            else if (IsAndroid())
            {
                var api = new AgeSignalsApi();
                var accessRes = await api.RequestAgeSignalsAccess(ct);

                if(!IsAgeVerificationRequired(accessRes))
                {
                    Debug.Log("Age verification is not required.");
                    return new AgeVerificationResult
                    {
                        Status = AgeVerificationStatus.AgeRangeNotRequired
                    };
                }

                var res = await api.CheckAgeSignals(ct);
                if (res.UserStatus.HasValue)
                {
                    var status = res.UserStatus.Value;
                    switch (status)
                    {
                        case AgeSignalsVerificationStatus.VERIFIED:
                        case AgeSignalsVerificationStatus.DECLARED:
                            return new AgeVerificationResult
                            {
                                Status = AgeVerificationStatus.HasAgeRange,
                                LowerBound = res.AgeLower,
                                UpperBound = res.AgeUpper,
                                MostRecentApprovalDate = DateTime.Now
                            };
                        case AgeSignalsVerificationStatus.SUPERVISED:
                        case AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_PENDING:
                        case AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_DENIED:
                            return new AgeVerificationResult
                            {
                                Status = AgeVerificationStatus.HasAgeRange,
                                LowerBound = res.AgeLower,
                                UpperBound = res.AgeUpper,
                                MostRecentApprovalDate = res.MostRecentApprovalDate
                            };
                        case AgeSignalsVerificationStatus.UNKNOWN:
                            return new AgeVerificationResult
                            {
                                Status = AgeVerificationStatus.Unknown
                            };
                        default:
                            Debug.LogError($"AgeSignal status was not in defined values status={status}");
                            return new AgeVerificationResult
                            {
                                Status = AgeVerificationStatus.Unknown
                            };
                    }
                }
                // user is not in region that requires age
                Debug.Log("Age verification is not required (no result from check).");
                return new AgeVerificationResult
                {
                    Status = AgeVerificationStatus.AgeRangeNotRequired
                };
            }

            // default to no age range required
            return new AgeVerificationResult
            {
                Status = AgeVerificationStatus.AgeRangeNotRequired
            };
        }

        private bool IsAgeVerificationRequired(AgeSignalsAccessResult accessResult)
        {
            return accessResult.AgeSignalsAccessStatus == AgeSignalsAccessStatus.SHARED
                || accessResult.AgeSignalsAccessStatus == AgeSignalsAccessStatus.VERIFICATION_REQUIRED;
        }

        private bool IsIOS()
        {
#if UNITY_IOS
            return true;
#else
            return false;
#endif
        }

        private bool IsAndroid()
        {
#if UNITY_ANDROID
            return true;
#else
            return false;
#endif
        }
    }


}