using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace IPTech.AgeVerification.Android.AgeSignals
{
    public interface IAgeSignalsResultBackingFields 
    {
        public bool _userStatusHasValue { get; set; }
        public AgeSignalsVerificationStatus _userStatus { get; set; }
        public bool _ageLowerHasValue { get; set; }
        public int _ageLower { get; set; }
        public bool _ageUppderHasValue { get; set; }
        public int _ageUpper { get; set; }
        public bool _mostRecentApprovalDateHasValue { get; set; }
        public DateTime MostRecentApprovalDate { get; set; }
        public string InstallId { get; set; }
    }

    [Serializable]
    public sealed class AgeSignalsResult : IAgeSignalsResultBackingFields
    {
        [SerializeField] private AgeSignalsVerificationStatus _userStatus;
        [SerializeField] private bool _userStatusHasValue;

        public AgeSignalsVerificationStatus? UserStatus
        {
            get => _userStatusHasValue ? _userStatus : null;
            set
            {
                _userStatusHasValue = value.HasValue;
                _userStatus = value ?? _userStatus;
            }
        }

        [SerializeField] private int _ageLower;
        [SerializeField] private bool _ageLowerHasValue;
        public int? AgeLower
        {
            get => _ageLowerHasValue ? _ageLower : null;
            set
            {
                _ageLowerHasValue = value.HasValue;
                _ageLower = value ?? _ageLower;
            }
        }

        [SerializeField] private int _ageUpper;
        [SerializeField] private bool _ageUpperHasValue;

        public int? AgeUpper
        {
            get => _ageUpperHasValue ? _ageUpper : null;
            set
            {
                _ageUpperHasValue = value.HasValue;
                _ageUpper = value ?? _ageUpper;
            }
        }

        [SerializeField] private long _mostRecentApprovalDateUtcTicks;
        [SerializeField] private bool _mostRecentApprovalDateHasValue;

        public DateTime? MostRecentApprovalDate
        {
            get => _mostRecentApprovalDateHasValue ? GetMostRecentApprovalDateFromTicks() : null;
            set
            {
                _mostRecentApprovalDateHasValue = value.HasValue;
                _mostRecentApprovalDateUtcTicks = value?.Ticks ?? 0;
            }
        }

        public string InstallId;


        [Preserve]
        public AgeSignalsResult() {}
        public AgeSignalsResult(AgeSignalsResult other)
        {
            _userStatus = other._userStatus;
            _userStatusHasValue = other._userStatusHasValue;
            _ageLower = other._ageLower;
            _ageLowerHasValue = other._ageLowerHasValue;
            _ageUpper = other._ageUpper;
            _ageUpperHasValue = other._ageUpperHasValue;
            _mostRecentApprovalDateUtcTicks = other._mostRecentApprovalDateUtcTicks;
            _mostRecentApprovalDateHasValue = other._mostRecentApprovalDateHasValue;
            InstallId = other.InstallId;
        }

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        private DateTime GetMostRecentApprovalDateFromTicks()
        {
            return new DateTime(_mostRecentApprovalDateUtcTicks, DateTimeKind.Utc);
        }

        private void SetTicksMostRecentApprovalDate(DateTime value)
        {
            _mostRecentApprovalDateUtcTicks = value.Ticks;
        }

        

        #region IAgeSignalsResultUIAccess
        bool IAgeSignalsResultBackingFields._userStatusHasValue { get => _userStatusHasValue; set => _userStatusHasValue = value; }
        AgeSignalsVerificationStatus IAgeSignalsResultBackingFields._userStatus { get => _userStatus; set => _userStatus = value; }
        bool IAgeSignalsResultBackingFields._ageLowerHasValue { get => _ageLowerHasValue; set => _ageLowerHasValue = value; }
        int IAgeSignalsResultBackingFields._ageLower { get => _ageLower; set => _ageLower = value; }
        bool IAgeSignalsResultBackingFields._ageUppderHasValue { get => _ageUpperHasValue; set => _ageUpperHasValue = value; }
        int IAgeSignalsResultBackingFields._ageUpper { get => _ageUpper; set => _ageUpper = value; }
        bool IAgeSignalsResultBackingFields._mostRecentApprovalDateHasValue { get => _mostRecentApprovalDateHasValue; set => _mostRecentApprovalDateHasValue = value; }
        DateTime IAgeSignalsResultBackingFields.MostRecentApprovalDate { get => GetMostRecentApprovalDateFromTicks(); set => SetTicksMostRecentApprovalDate(value); }
        string IAgeSignalsResultBackingFields.InstallId { get => InstallId; set => InstallId = value; }
        #endregion
    }
}