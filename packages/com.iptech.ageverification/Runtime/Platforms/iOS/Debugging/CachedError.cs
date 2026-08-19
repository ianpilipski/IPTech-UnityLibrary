
using System;
using UnityEngine.Scripting;

namespace IPTech.AgeVerification.iOS.Debugging
{
    [Serializable]
    public class CachedError
    {
        public enum ErrorType
        {
            GeneralException,
            PlatformNotSupportedException,
            AgeRangeNotAvailableException,
            AgeRangeInvalidRequestException
        }

        public ErrorType Type = ErrorType.GeneralException;
        public string Message = "[Mock] An error occurred";

        [Preserve]
        public CachedError() {}

        public CachedError(ErrorType type, string message)
        {
            Type = type;
            Message = message;
        } 

        public CachedError(CachedError other)
        {
            Type = other.Type;
            Message = other.Message;
        }

        public CachedError(Exception exception)
        {
            if (exception is PlatformNotSupportedException)
            {
                Type = ErrorType.PlatformNotSupportedException;
            }
            else if (exception is AgeRangeNotAvailableException)
            {
                Type = ErrorType.AgeRangeNotAvailableException;
            }
            else if (exception is AgeRangeInvalidRequestException)
            {
                Type = ErrorType.AgeRangeInvalidRequestException;
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