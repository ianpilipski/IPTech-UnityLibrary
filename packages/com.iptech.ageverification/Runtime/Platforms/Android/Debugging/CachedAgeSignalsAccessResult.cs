using System;
using UnityEngine;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    [Serializable]
    public class CachedAgeSignalsAccessResult
    {
        public enum ResultType
        {
            AgeSignalsAccessResult,
            Exception
        }

        [SerializeReference] public AgeSignalsAccessResult Result;
        [SerializeReference] public CachedError Error;

        [Obsolete("used for json serialization only", true)]
        public CachedAgeSignalsAccessResult()
        {
            // json serialization only
        }
        public CachedAgeSignalsAccessResult(AgeSignalsAccessResult result) : this(result, null) { }
        public CachedAgeSignalsAccessResult(CachedError error) : this(null, error) { }

        private CachedAgeSignalsAccessResult(AgeSignalsAccessResult result, CachedError error)
        {
            Result = result;
            Error = error;
        }

        public ResultType ResultKind
        {
            get
            {
                return Result != null ? ResultType.AgeSignalsAccessResult : ResultType.Exception;
            }
        }

        public AgeSignalsAccessResult EmulateApiResponseAsIfCalled()
        {
            if(this.ResultKind == CachedAgeSignalsAccessResult.ResultType.AgeSignalsAccessResult) 
            {
                return this.Result;
            }
            else if(this.ResultKind == CachedAgeSignalsAccessResult.ResultType.Exception) 
            {
                throw this.Error.CreateException();
            }
            throw new Exception("Invalid cached result type.");
        }
    }
}