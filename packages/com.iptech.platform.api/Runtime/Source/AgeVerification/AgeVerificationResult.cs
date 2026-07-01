
using System;

namespace IPTech.AgeVerification
{
    public class AgeVerificationResult
    {
        public AgeVerificationStatus Status;
        public int? LowerBound;
        public int? UpperBound;
        public DateTime? MostRecentApprovalDate;
    }

    public enum AgeVerificationStatus
    {
        Unknown,
        HasAgeRange,
        AgeRangeNotRequired
    }
}