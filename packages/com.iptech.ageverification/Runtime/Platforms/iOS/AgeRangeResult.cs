
using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace IPTech.AgeVerification.iOS
{
    // These values match what is defined in the IPTechAgeRangeUnityPlugin.swift file
    public enum AgeRangeResultStatus
	{
		Error,
        Success,
        UserDeclined,
        UnsupportedPlatformVersion
    }

    // These values match what is defined in the IPTechAgeRangeUnityPlugin.swift file
    public enum AgeDeclaration
    {
        Unknown = 0,
        SelfDeclared = 1,
        GuardianDeclared = 2
    }

    public interface IAgeRangeResultBackingFields 
    {
        public AgeRangeResultStatus Status { get; set; }
        public int _lowerBound { get; set; }
        public bool _lowerBoundHasValue { get; set; }
        public int _upperBound { get; set; }
        public bool _upperBoundHasValue { get; set; }
        public AgeDeclaration AgeDeclaration { get; set;}
    }

    [Serializable]
    public class AgeRangeResult : IAgeRangeResultBackingFields
    {
        public AgeRangeResultStatus Status;

        [SerializeField] private int _lowerBound;
        [SerializeField] private bool _lowerBoundHasValue;

        public int? LowerBound
        {
            get => _lowerBoundHasValue ? _lowerBound : null;
            set
            {
                _lowerBoundHasValue = value.HasValue;
                _lowerBound = value ?? _lowerBound;
            }
        }

        [SerializeField] private int _upperBound;
        [SerializeField] private bool _upperBoundHasValue;

        public int? UpperBound
        {
            get => _upperBoundHasValue ? _upperBound : null;
            set
            {
                _upperBoundHasValue = value.HasValue;
                _upperBound = value ?? _upperBound;
            }
        }
        
        public AgeDeclaration AgeDeclaration;

        [Preserve]
        public AgeRangeResult()
        {
        }

        public AgeRangeResult(AgeRangeResultStatus status, int? lowerBound, int? upperBound, AgeDeclaration ageDeclaration)
        {
            Status = status;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            AgeDeclaration = ageDeclaration;
        }

        public AgeRangeResult(AgeRangeResult other)
        {
            Status = other.Status;
            LowerBound = other.LowerBound;
            UpperBound = other.UpperBound;
            AgeDeclaration = other.AgeDeclaration;
        }

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public override string ToString()
        {
            return $"AgeRangeResult(Status={Status}, LowerBound={LowerBound}, UpperBound={UpperBound}, AgeDeclaration={AgeDeclaration})";
        }


        #region IAgeRangeResultBackingFields
        AgeRangeResultStatus IAgeRangeResultBackingFields.Status { get => Status; set => Status = value; }
        int IAgeRangeResultBackingFields._lowerBound { get => _lowerBound; set => _lowerBound = value; }
        bool IAgeRangeResultBackingFields._lowerBoundHasValue { get => _lowerBoundHasValue; set => _lowerBoundHasValue = value; }
        int IAgeRangeResultBackingFields._upperBound { get => _upperBound; set => _upperBound = value; }
        bool IAgeRangeResultBackingFields._upperBoundHasValue { get => _upperBoundHasValue; set => _upperBoundHasValue = value; }
        AgeDeclaration IAgeRangeResultBackingFields.AgeDeclaration { get => AgeDeclaration; set => AgeDeclaration = value; }
        #endregion
    }
}