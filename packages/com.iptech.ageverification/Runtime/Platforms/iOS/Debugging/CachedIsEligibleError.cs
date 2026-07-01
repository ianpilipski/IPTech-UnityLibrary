
using System;

namespace IPTech.AgeVerification.iOS.Debugging
{
    [Serializable]
    public class CachedIsEligibleError
    {
        public enum ErrorType
        {
            AgeRangeNotAvailableException,
            PlatformNotSupportedException,
            GeneralException
        }

        public ErrorType Type = ErrorType.AgeRangeNotAvailableException;
        public string Message = "[Mock] An error occurred";


        [Obsolete("used only for unity json deserialization", true)]
        public CachedIsEligibleError()
        {
            // for deserialization
        }

        public CachedIsEligibleError(ErrorType type, string message)
        {
            Type = type;
            Message = message;
        } 

        public CachedIsEligibleError(Exception exception)
        {
            if (exception is AgeRangeNotAvailableException)
            {
                Type = ErrorType.AgeRangeNotAvailableException;
            }
            else if(exception is PlatformNotSupportedException)
            {
                Type = ErrorType.PlatformNotSupportedException;
            }
            else
            {
                Type = ErrorType.GeneralException;
            }
            Message = exception.Message;
        }
    }
}