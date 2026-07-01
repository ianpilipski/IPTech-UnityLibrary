using System;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    [Serializable]
    public class CachedError
    {
        public enum ErrorType
        {
            GeneralException,
            PlatformNotSupportedException,
            InvalidOperationException,
            AgeSignalsException
        }

        public int ErrorCode;
        public ErrorType Type = ErrorType.GeneralException;
        public string Message = "[Mock] An error occurred";

        [Obsolete("used for json serialization only", true)]
        public CachedError()
        {
            // json serialization only
        }

        public CachedError(ErrorType type, int errorCode, string message)
        {
            Type = type;
            ErrorCode = errorCode;
            Message = message;
        }

        public CachedError(Exception exception)
        {
            if (exception is PlatformNotSupportedException)
            {
                Type = ErrorType.PlatformNotSupportedException;
            }
            else if (exception is InvalidOperationException)
            {
                Type = ErrorType.InvalidOperationException;
            }
            else if (exception is AgeSignalsException ageSignalsException)
            {
                Type = ErrorType.AgeSignalsException;
                ErrorCode = ageSignalsException.ErrorCode;
            }
            else
            {
                Type = ErrorType.GeneralException;
            }
            Message = exception.Message;
        }
    }
}