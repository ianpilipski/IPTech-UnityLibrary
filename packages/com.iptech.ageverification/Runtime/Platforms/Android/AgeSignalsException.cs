

using System;

namespace IPTech.AgeVerification.Android.AgeSignals
{
    public class AgeSignalsException : Exception
    {
        public int ErrorCode { get; private set; }

        public AgeSignalsException(int errorCode, string message) : base(CreateErrorMessage(errorCode, message))
        {
            ErrorCode = errorCode;
        }

        private static string CreateErrorMessage(int errorCode, string message)
        {
            if(TryGetKnownErrorCode(errorCode, out var knownError))
            {
                return $"{nameof(Internal.IPTechAgeSignalsUnityPlugin)}: {knownError} ({errorCode}): {message}";
            }
            else
            {
                return $"{nameof(Internal.IPTechAgeSignalsUnityPlugin)}: Unknown Error ({errorCode}): {message}";
            }
        }

        // Define known error codes for reference (MATCHING THE JAVA SDK VALUES)
        public enum KnownErrorCodes
        {
            API_NOT_AVAILABLE = -1,
            PLAY_STORE_NOT_FOUND = -2,
            NETWORK_ERROR = -3,
            PLAY_SERVICES_NOT_FOUND = -4,
            CANNOT_BIND_TO_SERVICE = -5,
            PLAY_STORE_VERSION_OUTDATED = -6,
            PLAY_SERVICES_VERSION_OUTDATED = -7,
            CLIENT_TRANSIENT_ERROR = -8,
            APP_NOT_OWNED = -9,
            SDK_VERSION_OUTDATED = -10,
            INTERNAL_ERROR = -100,
            NO_ERROR = 0,
        }

        public static bool TryGetKnownErrorCode(int errorCode, out KnownErrorCodes knownError)
        {
            if (Enum.IsDefined(typeof(KnownErrorCodes), errorCode))
            {
                knownError = (KnownErrorCodes)errorCode;
                return true;
            }
            knownError = default;
            return false;
        }
    }
}