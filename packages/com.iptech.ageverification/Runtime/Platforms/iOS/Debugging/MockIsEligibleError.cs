using System;

namespace IPTech.AgeVerification.iOS.Debugging
{
    public class MockIsEligibleError
    {
        public CachedIsEligibleError.ErrorType Type = CachedIsEligibleError.ErrorType.AgeRangeNotAvailableException;
        public string Message = "[Mock] An error occurred checking age features eligibility";

        public MockIsEligibleError()
        {
        }

        public MockIsEligibleError(CachedIsEligibleError error)
        {
            Type = error.Type;
            Message = error.Message;
        }

        public Exception CreateException()
        {
            switch (Type)
            {
                case CachedIsEligibleError.ErrorType.AgeRangeNotAvailableException:
                    return new AgeRangeNotAvailableException(Message);
                case CachedIsEligibleError.ErrorType.PlatformNotSupportedException:
                    return new PlatformNotSupportedException(Message);
                case CachedIsEligibleError.ErrorType.GeneralException:
                default:
                    return new Exception(Message);
            }
        }
    }
}