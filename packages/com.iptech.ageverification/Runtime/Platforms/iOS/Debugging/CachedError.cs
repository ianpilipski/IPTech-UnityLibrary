
using System;

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

        public ErrorType Type {get;} = ErrorType.GeneralException;
        public string Message {get;} = "[Mock] An error occurred";


        public CachedError(ErrorType type, string message)
        {
            Type = type;
            Message = message;
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
    }
}