
using System;

namespace IPTech.AgeVerification.iOS.Debugging
{
    [Serializable]
    public class CachedResult
    {
        public enum ResultType
        {
            AgeRangeResult,
            Exception
        }

        public AgeRangeResult Result;
        public CachedError Error;

        [Obsolete("used only for unity json deserialization", true)]
        public CachedResult()
        {
            // for json deserialization
        }
        public CachedResult(AgeRangeResult result) : this(result, null) {}
        public CachedResult(CachedError error) : this(null, error) {}
        
        private CachedResult(AgeRangeResult result, CachedError error)
        {
            Result = result;
            Error = error; 
        }
        
        public ResultType ResultKind
        {
            get
            {
                return Result != null ? ResultType.AgeRangeResult : ResultType.Exception;
            }
        }
    }
}