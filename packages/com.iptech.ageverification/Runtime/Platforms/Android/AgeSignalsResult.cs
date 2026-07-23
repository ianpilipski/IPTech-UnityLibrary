using System;
using UnityEngine;

namespace IPTech.AgeVerification.Android.AgeSignals
{
    [Serializable]
    public sealed class AgeSignalsResult
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
            get => _mostRecentApprovalDateHasValue ? new DateTime(_mostRecentApprovalDateUtcTicks, DateTimeKind.Utc) : null;
            set
            {
                _mostRecentApprovalDateHasValue = value.HasValue;
                _mostRecentApprovalDateUtcTicks = value?.Ticks ?? 0;
            }
        }

        public string InstallId;

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }
    }
}