
using System;
using UnityEngine.Scripting;

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

        [Preserve]
        public CachedIsEligibleError()
        {
        }

        public CachedIsEligibleError(ErrorType type, string message)
        {
            Type = type;
            Message = message;
        }

        public CachedIsEligibleError(CachedIsEligibleError other)
        {
            Type = other.Type;
            Message = other.Message;
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