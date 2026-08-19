using System;
using UnityEngine.Scripting;

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

        [Preserve]
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

        public CachedError(CachedError other)
        {
            Type = other.Type;
            ErrorCode = other.ErrorCode;
            Message = other.Message;
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