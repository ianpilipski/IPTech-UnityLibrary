using System;
using UnityEngine;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
{
    public class MockResult
    {
        public bool HasUserStatus = true;
        public AgeSignalsVerificationStatus UserStatus = AgeSignalsVerificationStatus.VERIFIED;
        public bool HasAgeLower = true;
        public int AgeLower = 13;
        public bool HasAgeUpper = true;
        public int AgeUpper = 17;
        public bool HasMostRecentApprovalDate = false;
        public string MostRecentApprovalDateString = "";
        public string InstallId = "";

        public AgeSignalsResult CreateResult()
        {
            var userStatus = HasUserStatus ? (AgeSignalsVerificationStatus?)UserStatus : null;
            var ageLower = HasAgeLower ? (int?)AgeLower : null;
            var ageUpper = HasAgeUpper ? (int?)AgeUpper : null;
            
            DateTime? mostRecentApprovalDate = null;
            if (HasMostRecentApprovalDate && !string.IsNullOrEmpty(MostRecentApprovalDateString))
            {
                if (DateTime.TryParse(MostRecentApprovalDateString, out var parsedDate))
                {
                    mostRecentApprovalDate = parsedDate;
                }
            }

            return new AgeSignalsResult
            {
                UserStatus = userStatus,
                AgeLower = ageLower,
                AgeUpper = ageUpper,
                MostRecentApprovalDate = mostRecentApprovalDate,
                InstallId = InstallId
            };
        }

        public void PopulateFromResult(AgeSignalsResult result)
        {
            HasUserStatus = result.UserStatus.HasValue;
            UserStatus = result.UserStatus ?? AgeSignalsVerificationStatus.UNKNOWN;
            HasAgeLower = result.AgeLower.HasValue;
            AgeLower = result.AgeLower ?? 0;
            HasAgeUpper = result.AgeUpper.HasValue;
            AgeUpper = result.AgeUpper ?? 0;
            HasMostRecentApprovalDate = result.MostRecentApprovalDate.HasValue;
            MostRecentApprovalDateString = result.MostRecentApprovalDate?.ToString("o") ?? "";
            InstallId = result.InstallId ?? "";
        }
    }
}
