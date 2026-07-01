
namespace IPTech.AgeVerification.iOS.Debugging
{
    public class MockIsEligible
    {
        public bool IsEligible;

        public void PopulateFromResult(CachedIsEligibleResult cachedResult)
        {
            if (cachedResult.ResultKind == CachedIsEligibleResult.ResultType.IsEligibleResult)
            {
                IsEligible = cachedResult.Result;
            }
        }
    }
}