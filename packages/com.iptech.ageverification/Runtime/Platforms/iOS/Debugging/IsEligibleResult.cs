
using System;

namespace IPTech.AgeVerification.iOS.Debugging
{
    [Serializable]
    public class IsEligibleResult
    {
        public bool IsEligible;

        public IsEligibleResult() {}
        public IsEligibleResult(IsEligibleResult other)
        {
            IsEligible = other.IsEligible;
        }
    }
}