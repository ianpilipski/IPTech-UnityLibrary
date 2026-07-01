
using System;
using System.Text.Json.Serialization;
using UnityEngine;

namespace IPTech.AgeVerification.iOS
{
    // These values match what is defined in the IPAgeRangeUnityPlugin.swift file
    public enum AgeRangeResultStatus
	{
		Error,
        Success,
        UserDeclined,
        UnsupportedPlatformVersion
    }

    // These values match what is defined in the IPAgeRangeUnityPlugin.swift file
    public enum AgeDeclaration
    {
        Unknown = 0,
        SelfDeclared = 1,
        GuardianDeclared = 2
    }

    [Serializable]
    public class AgeRangeResult
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

        [Obsolete("used only for unity json deserialization", true)]
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

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this);
        }

        public override string ToString()
        {
            return $"AgeRangeResult(Status={Status}, LowerBound={LowerBound}, UpperBound={UpperBound}, AgeDeclaration={AgeDeclaration})";
        }
    }
}