
using System;

namespace IPTech.AgeVerification.iOS.Debugging
{
    public class MockError
    {
        public CachedError.ErrorType Type = CachedError.ErrorType.GeneralException;
        public string Message = "[Mock] An error occurred";

        public MockError()
        {
        }

        public MockError(CachedError error)
        {
            Type = error.Type;
            Message = error.Message;
        }

        public Exception CreateException()
        {
            switch (Type)
            {
                case CachedError.ErrorType.PlatformNotSupportedException:
                    return new PlatformNotSupportedException(Message);
                case CachedError.ErrorType.AgeRangeNotAvailableException:
                    return new AgeRangeNotAvailableException(Message);
                case CachedError.ErrorType.AgeRangeInvalidRequestException:
                    return new AgeRangeInvalidRequestException(Message);
                case CachedError.ErrorType.GeneralException:
                default:
                    return new Exception(Message);
            }
        }
    }
}