using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace IPTech.AgeVerification.Android.AgeSignals
{
    public interface IAgeSignalsAccessResultBackingFields
    {
        public bool _hasAgeSignalsAccessStatus { get; set; }
        public AgeSignalsAccessStatus _ageSignalsAccessStatus { get; set;}    
    }

    [Serializable]
    public sealed class AgeSignalsAccessResult : IAgeSignalsAccessResultBackingFields
    {
        [SerializeField] private bool _hasAgeSignalsAccessStatus;
        [SerializeField] private AgeSignalsAccessStatus _ageSignalsAccessStatus;

        [Preserve]
        public AgeSignalsAccessResult() {}

        public AgeSignalsAccessResult(AgeSignalsAccessResult other)
        {
            _hasAgeSignalsAccessStatus = other._hasAgeSignalsAccessStatus;
            _ageSignalsAccessStatus = other._ageSignalsAccessStatus;
        }

        public AgeSignalsAccessStatus? AgeSignalsAccessStatus 
        { 
            get
            {
                return _hasAgeSignalsAccessStatus ? _ageSignalsAccessStatus : null;
            }
            set
            {
                _hasAgeSignalsAccessStatus = value.HasValue;
                _ageSignalsAccessStatus = value ?? _ageSignalsAccessStatus;
            }
        }

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        #region IAgeSignalsAccessResultBackingFields
        bool IAgeSignalsAccessResultBackingFields._hasAgeSignalsAccessStatus { get => _hasAgeSignalsAccessStatus; set => _hasAgeSignalsAccessStatus = value; }
        AgeSignalsAccessStatus IAgeSignalsAccessResultBackingFields._ageSignalsAccessStatus { get => _ageSignalsAccessStatus; set => _ageSignalsAccessStatus = value; }
        #endregion
    }
}