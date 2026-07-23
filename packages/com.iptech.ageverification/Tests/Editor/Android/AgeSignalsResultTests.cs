using System;
using NUnit.Framework;
using IPTech.AgeVerification.Android.AgeSignals;
using UnityEngine;

namespace Tests
{
    [TestFixture]
    public class AgeSignalsResultTests
    {
        private AgeSignalsResult _ageSignalsResult;

        [SetUp]
        public void SetUp()
        {
            _ageSignalsResult = new AgeSignalsResult();
        }

        [Test]
        public void UserStatus_CanBeSetAndRetrieved()
        {
            // Arrange
            var expectedStatus = AgeSignalsVerificationStatus.VERIFIED;

            // Act
            _ageSignalsResult.UserStatus = expectedStatus;

            // Assert
            Assert.AreEqual(expectedStatus, _ageSignalsResult.UserStatus);
        }

        [Test]
        public void UserStatus_CanBeNull()
        {
            // Arrange & Act
            _ageSignalsResult.UserStatus = null;

            // Assert
            Assert.IsNull(_ageSignalsResult.UserStatus);
        }

        [Test]
        public void AgeLower_CanBeSetAndRetrieved()
        {
            // Arrange
            var expectedAge = 18;

            // Act
            _ageSignalsResult.AgeLower = expectedAge;

            // Assert
            Assert.AreEqual(expectedAge, _ageSignalsResult.AgeLower);
        }

        [Test]
        public void AgeLower_CanBeNull()
        {
            // Arrange & Act
            _ageSignalsResult.AgeLower = null;

            // Assert
            Assert.IsNull(_ageSignalsResult.AgeLower);
        }

        [Test]
        public void AgeUpper_CanBeSetAndRetrieved()
        {
            // Arrange
            var expectedAge = 25;

            // Act
            _ageSignalsResult.AgeUpper = expectedAge;

            // Assert
            Assert.AreEqual(expectedAge, _ageSignalsResult.AgeUpper);
        }

        [Test]
        public void AgeUpper_CanBeNull()
        {
            // Arrange & Act
            _ageSignalsResult.AgeUpper = null;

            // Assert
            Assert.IsNull(_ageSignalsResult.AgeUpper);
        }

        [Test]
        public void MostRecentApprovalDate_CanBeSetAndRetrieved()
        {
            // Arrange
            var expectedDate = new DateTime(2023, 10, 15, 14, 30, 0);

            // Act
            _ageSignalsResult.MostRecentApprovalDate = expectedDate;

            // Assert
            Assert.AreEqual(expectedDate, _ageSignalsResult.MostRecentApprovalDate);
        }

        [Test]
        public void MostRecentApprovalDate_CanBeNull()
        {
            // Arrange & Act
            _ageSignalsResult.MostRecentApprovalDate = null;

            // Assert
            Assert.IsNull(_ageSignalsResult.MostRecentApprovalDate);
        }

        [Test]
        public void InstallId_CanBeSetAndRetrieved()
        {
            // Arrange
            var expectedInstallId = "test-install-id-12345";

            // Act
            _ageSignalsResult.InstallId = expectedInstallId;

            // Assert
            Assert.AreEqual(expectedInstallId, _ageSignalsResult.InstallId);
        }

        [Test]
        public void InstallId_CanBeNull()
        {
            // Arrange & Act
            _ageSignalsResult.InstallId = null;

            // Assert
            Assert.IsNull(_ageSignalsResult.InstallId);
        }

        [Test]
        public void ToJson_WithAllPropertiesSet_ReturnsValidJson()
        {
            // Arrange
            _ageSignalsResult.UserStatus = AgeSignalsVerificationStatus.VERIFIED;
            _ageSignalsResult.AgeLower = 18;
            _ageSignalsResult.AgeUpper = 25;
            _ageSignalsResult.MostRecentApprovalDate = new DateTime(2023, 10, 15, 14, 30, 0, DateTimeKind.Utc);
            _ageSignalsResult.InstallId = "test-install-id";

            // Act
            var json = _ageSignalsResult.ToJson();

            // Assert
            Assert.AreEqual(CreateExpectedJson(AgeSignalsVerificationStatus.VERIFIED, 18, 25, new DateTime(2023, 10, 15, 14, 30, 0, DateTimeKind.Utc), "test-install-id"), json);
        }

        [Test]
        public void ToJson_WithNullProperties_IncludesNullValues()
        {
            // Arrange
            _ageSignalsResult.UserStatus = null;
            _ageSignalsResult.AgeLower = null;
            _ageSignalsResult.AgeUpper = null;
            _ageSignalsResult.MostRecentApprovalDate = null;
            _ageSignalsResult.InstallId = "test-install-id";

            // Act
            var json = _ageSignalsResult.ToJson();

            // Assert
            Assert.IsNotNull(json);
            Assert.AreEqual(CreateExpectedJson(null, null, null, null, "test-install-id"), json);
        }

        [Test]
        public void ToJson_WithPartialProperties_IncludesAllValues()
        {
            // Arrange
            _ageSignalsResult.UserStatus = AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_PENDING;
            _ageSignalsResult.AgeLower = 13;
            _ageSignalsResult.InstallId = "partial-test-id";
            // AgeUpper and MostRecentApprovalDate are null by default

            // Act
            var json = _ageSignalsResult.ToJson();

            // Assert
            Assert.IsNotNull(json);
            Assert.AreEqual(CreateExpectedJson(AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_PENDING, 13, null, null, "partial-test-id"), json);
        }

        [Test]
        public void ToJson_WithAllNullProperties_ReturnsJsonWithAllNullValues()
        {
            // Arrange
            _ageSignalsResult.UserStatus = null;
            _ageSignalsResult.AgeLower = null;
            _ageSignalsResult.AgeUpper = null;
            _ageSignalsResult.MostRecentApprovalDate = null;
            _ageSignalsResult.InstallId = null;

            // Act
            var json = _ageSignalsResult.ToJson();

            Assert.AreEqual(CreateExpectedJson(null, null, null, null, null), json);
        }

        string CreateExpectedJson(AgeSignalsVerificationStatus? userStatus, int? ageLower, int? ageUpper, DateTime? mostRecentApprovalDate, string installId)
        {
            // {"_userStatus":0,"_userStatusHasValue":false,"_ageLower":0,"_ageLowerHasValue":false,"_ageUpper":0,"_ageUpperHasValue":false,"_mostRecentApprovalDateHasValue":false,"InstallId":""}
            return $"{{\"_userStatus\":{((int)(userStatus ?? 0))},\"_userStatusHasValue\":{(userStatus.HasValue).ToString().ToLower()}," +
                $"\"_ageLower\":{ageLower ?? 0},\"_ageLowerHasValue\":{(ageLower.HasValue).ToString().ToLower()},\"_ageUpper\":{ageUpper ?? 0}," +
                $"\"_ageUpperHasValue\":{(ageUpper.HasValue).ToString().ToLower()}," + 
                $"\"_mostRecentApprovalDateUtcTicks\":{mostRecentApprovalDate?.Ticks ?? 0}," +
                $"\"_mostRecentApprovalDateHasValue\":{mostRecentApprovalDate.HasValue.ToString().ToLower()}," + 
                $"\"InstallId\":\"{installId ?? ""}\"}}";
        }

        [Test]
        public void ToJson_WithDifferentVerificationStatuses_SerializesCorrectly()
        {
            // Test all enum values
            var statusTestCases = new[]
            {
                (AgeSignalsVerificationStatus.VERIFIED, "VERIFIED"),
                (AgeSignalsVerificationStatus.SUPERVISED, "SUPERVISED"),
                (AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_PENDING, "SUPERVISED_APPROVAL_PENDING"),
                (AgeSignalsVerificationStatus.SUPERVISED_APPROVAL_DENIED, "SUPERVISED_APPROVAL_DENIED"),
                (AgeSignalsVerificationStatus.UNKNOWN, "UNKNOWN")
            };

            foreach (var (status, expectedValue) in statusTestCases)
            {
                // Arrange
                _ageSignalsResult.UserStatus = status;
                _ageSignalsResult.InstallId = "test-id";

                // Act
                var json = _ageSignalsResult.ToJson();

                Assert.AreEqual(CreateExpectedJson(status, null, null, null, "test-id"), json);
            }
        }

        [Test]
        public void ToJson_WithNegativeAgeValues_SerializesCorrectly()
        {
            // Arrange
            _ageSignalsResult.AgeLower = -1;
            _ageSignalsResult.AgeUpper = -5;
            _ageSignalsResult.InstallId = "negative-age-test";

            // Act
            var json = _ageSignalsResult.ToJson();

            // Assert
            Assert.AreEqual(CreateExpectedJson(null, -1, -5, null, "negative-age-test"), json);
        }

        [Test]
        public void ToJson_WithZeroAgeValues_SerializesCorrectly()
        {
            // Arrange
            _ageSignalsResult.AgeLower = 0;
            _ageSignalsResult.AgeUpper = 0;
            _ageSignalsResult.InstallId = "zero-age-test";

            // Act
            var json = _ageSignalsResult.ToJson();

            // Assert
            Assert.AreEqual(CreateExpectedJson(null, 0, 0, null, "zero-age-test"), json);
        }

        [Test]
        public void ToJson_WithEmptyStringInstallId_SerializesCorrectly()
        {
            // Arrange
            _ageSignalsResult.InstallId = string.Empty;

            // Act
            var json = _ageSignalsResult.ToJson();

            // Assert
            Assert.IsNotNull(json);
            Assert.AreEqual(CreateExpectedJson(null, null, null, null, ""), json);
        }

        [Test]
        public void ToJson_WithSpecialCharactersInInstallId_EscapesCorrectly()
        {
            // Arrange
            var installId = "test\"id\\with/special\ncharacters\t";
            _ageSignalsResult.InstallId = installId;

            // Act
            var json = _ageSignalsResult.ToJson();
            var roundTrip = JsonUtility.FromJson<AgeSignalsResult>(json);

            Assert.AreEqual(installId, roundTrip.InstallId);
        }
    }
}
