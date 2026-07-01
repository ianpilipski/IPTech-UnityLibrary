using UnityEngine;

namespace IPTech.AgeVerification.iOS.Debugging
{
    public class MockResult
    {
        public AgeRangeResultStatus Status = AgeRangeResultStatus.Success;
        public bool HasLowerBound = true;
        public int LowerBound = 13;
        public bool HasUpperBound = true;
        public int UpperBound = 17;
        public AgeDeclaration AgeDeclaration = AgeDeclaration.SelfDeclared;

        public AgeRangeResult CreateResult()
        {
            var lowerBound = HasLowerBound ? (int?)LowerBound : null;
            var upperBound = HasUpperBound ? (int?)UpperBound : null;
            return new AgeRangeResult(Status, lowerBound, upperBound, AgeDeclaration);
        }

        public void PopulateFromResult(AgeRangeResult result)
        {
            Status = result.Status;
            HasLowerBound = result.LowerBound.HasValue;
            LowerBound = result.LowerBound ?? 0;
            HasUpperBound = result.UpperBound.HasValue;
            UpperBound = result.UpperBound ?? 0;
            AgeDeclaration = result.AgeDeclaration;
        }
    }
}
