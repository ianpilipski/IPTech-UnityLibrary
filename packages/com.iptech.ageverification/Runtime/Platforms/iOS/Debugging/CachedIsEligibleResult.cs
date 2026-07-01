
using System;

namespace IPTech.AgeVerification.iOS.Debugging
{
    [Serializable]
    public class CachedIsEligibleResult
    {
        public enum ResultType
        {
            IsEligibleResult,
            Exception
        }

        public bool Result;
        public CachedIsEligibleError Error;

        public CachedIsEligibleResult() 
        {
             // for deserialization 
        }
        public CachedIsEligibleResult(bool result) : this(result, null) {}
        public CachedIsEligibleResult(CachedIsEligibleError error) : this(false, error) {}
        private CachedIsEligibleResult(bool result, CachedIsEligibleError error)
        {
            Result = result;
            Error = error; 
        }
        
        public ResultType ResultKind
        {
            get
            {
                return Error == null ? ResultType.IsEligibleResult : ResultType.Exception;
            }
        }
    }
}