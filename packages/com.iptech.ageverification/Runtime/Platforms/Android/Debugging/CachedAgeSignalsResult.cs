using System;
using UnityEngine;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    [Serializable]
    public class CachedAgeSignalsResult
    {
        public enum ResultType
        {
            AgeSignalsResult,
            Exception
        }

        [SerializeReference] public AgeSignalsResult Result;
        [SerializeReference] public CachedError Error;

        [Obsolete("used for json serialization only", true)]
        public CachedAgeSignalsResult()
        {
            // json serialization only
        }
        public CachedAgeSignalsResult(AgeSignalsResult result) : this(result, null) { }
        public CachedAgeSignalsResult(CachedError error) : this(null, error) { }

        private CachedAgeSignalsResult(AgeSignalsResult result, CachedError error)
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

        public AgeSignalsResult EmulateApiResponseAsIfCalled()
        {
            if(this.ResultKind == CachedAgeSignalsResult.ResultType.AgeSignalsResult) 
            {
                return this.Result;
            }
            else if(this.ResultKind == CachedAgeSignalsResult.ResultType.Exception) 
            {
                throw this.Error.CreateException();
            }
            throw new Exception("Invalid cached result type.");
        }
    }
}