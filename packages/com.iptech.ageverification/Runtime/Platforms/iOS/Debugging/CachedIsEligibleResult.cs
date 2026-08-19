
using System;
using UnityEngine;

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

        [SerializeReference] public IsEligibleResult Result;
        [SerializeReference] public CachedIsEligibleError Error;

        public CachedIsEligibleResult() {}
        public CachedIsEligibleResult(bool result) : this(new IsEligibleResult { IsEligible = result }, null) {}
        public CachedIsEligibleResult(IsEligibleResult isEligibleResult) : this(isEligibleResult, null) {}
        public CachedIsEligibleResult(CachedIsEligibleError error) : this(null, error) {}

        private CachedIsEligibleResult(IsEligibleResult result, CachedIsEligibleError error)
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

        public bool EmulateApiResponseAsIfCalled()
        {
            if(ResultKind == CachedIsEligibleResult.ResultType.IsEligibleResult) 
            {
                return Result.IsEligible;
            }
            else if(ResultKind == CachedIsEligibleResult.ResultType.Exception) 
            {
                throw Error.CreateException();
            }
            throw new Exception("Invalid cached result type.");
        }
    }
}