
using System;
using System.Threading;
using System.Threading.Tasks;
using IPTech.AgeVerification.Android.AgeSignals;
using IPTech.AgeVerification.iOS;
using UnityEngine;

namespace IPTech.AgeVerification
{
    public class AgeVerificationManager : IAgeVerification
    {
        public async Task<AgeVerificationResult> VerifyAge(int requiredMinAge, CancellationToken ct, int additionalMinAge1 = 0, int additionalMinAge2 = 0)
        {
            if (IsIOS())
            {
                var api = new AgeRangeApi();
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
                    return new AgeVerificationResult
                    {
                        Status = AgeVerificationStatus.Unknown
                    };
                }
                if (res.Status == AgeRangeResultStatus.UnsupportedPlatformVersion)
                {
                    Debug.LogWarning("Age Verification is not supported on this iOS version.");
                    return new AgeVerificationResult
                    {
                        Status = AgeVerificationStatus.AgeRangeNotRequired
                    };
                }
                if (res.Status == AgeRangeResultStatus.Error)
                {
                    Debug.LogError("Age Verification failed with an error.");
                }
                // Error or unknown status
                return new AgeVerificationResult
                {
                    Status = AgeVerificationStatus.Unknown
                };
            }
            else if (IsAndroid())
            {
                var api = new AgeSignalsApi();
                var res = await api.RequestAgeSignals(ct);
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