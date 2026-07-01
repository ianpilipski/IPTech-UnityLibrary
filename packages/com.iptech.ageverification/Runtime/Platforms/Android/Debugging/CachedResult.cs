using System;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    [Serializable]
    public class CachedResult
    {
        public enum ResultType
        {
            AgeSignalsResult,
            Exception
        }

        public AgeSignalsResult Result;
        public CachedError Error;

        [Obsolete("used for json serialization only", true)]
        public CachedResult()
        {
            // json serialization only
        }
        public CachedResult(AgeSignalsResult result) : this(result, null) { }
        public CachedResult(CachedError error) : this(null, error) { }

        private CachedResult(AgeSignalsResult result, CachedError error)
        {
            Result = result;
            Error = error;
        }

        public ResultType ResultKind
        {
            get
            {
                return Result != null ? ResultType.AgeSignalsResult : ResultType.Exception;
            }
        }
    }
}