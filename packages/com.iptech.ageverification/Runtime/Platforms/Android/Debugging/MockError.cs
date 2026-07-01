using System;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    public class MockError
    {
        public CachedError.ErrorType Type = CachedError.ErrorType.GeneralException;
        public string Message = "[Mock] An error occurred";
        public int ErrorCode = 0;

        public MockError()
        {
        }

        public MockError(CachedError error)
        {
            Type = error.Type;
            Message = error.Message;
            ErrorCode = error.ErrorCode;
        }

        public Exception CreateException()
        {
            switch (Type)
            {
                case CachedError.ErrorType.PlatformNotSupportedException:
                    return new PlatformNotSupportedException(Message);
                case CachedError.ErrorType.InvalidOperationException:
                    return new InvalidOperationException(Message);
                case CachedError.ErrorType.AgeSignalsException:
                    return new AgeSignalsException(ErrorCode, Message);
                case CachedError.ErrorType.GeneralException:
                default:
                    return new Exception(Message);
            }
        }
    }
}