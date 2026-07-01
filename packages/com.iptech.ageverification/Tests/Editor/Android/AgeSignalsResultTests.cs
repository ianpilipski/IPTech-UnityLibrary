using System;
using NUnit.Framework;
using IPTech.AgeVerification.Android.AgeSignals;

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
            Assert.IsNotNull(json);
            Assert.That(json, Contains.Substring("\"UserStatus\":\"VERIFIED\""));
            Assert.That(json, Contains.Substring("\"AgeLower\":18"));
            Assert.That(json, Contains.Substring("\"AgeUpper\":25"));
            Assert.That(json, Contains.Substring("\"MostRecentApprovalDate\":\"2023-10-15T14:30:00Z\""));
            Assert.That(json, Contains.Substring("\"InstallId\":\"test-install-id\""));
        }

        [Test]
        public void ToJson_WithPrettyPrint_ReturnsFormattedJson()
        {
            // Arrange
            _ageSignalsResult.UserStatus = AgeSignalsVerificationStatus.SUPERVISED;
            _ageSignalsResult.InstallId = "test-install-id";

            // Act
            var json = _ageSignalsResult.ToJson(prettyPrint: true);

            // Assert
            Assert.IsNotNull(json);
            Assert.That(json, Contains.Substring("\n")); // Should contain newlines for pretty printing
            Assert.That(json, Contains.Substring("\"UserStatus\": \"SUPERVISED\""));
            Assert.That(json, Contains.Substring("\"InstallId\": \"test-install-id\""));
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
            Assert.That(json, Contains.Substring("\"UserStatus\":null"));
            Assert.That(json, Contains.Substring("\"AgeLower\":null"));
            Assert.That(json, Contains.Substring("\"AgeUpper\":null"));
            Assert.That(json, Contains.Substring("\"MostRecentApprovalDate\":null"));
            Assert.That(json, Contains.Substring("\"InstallId\":\"test-install-id\""));
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
            Assert.That(json, Contains.Substring("\"UserStatus\":\"SUPERVISED_APPROVAL_PENDING\""));
            Assert.That(json, Contains.Substring("\"AgeLower\":13"));
            Assert.That(json, Contains.Substring("\"InstallId\":\"partial-test-id\""));
            Assert.That(json, Contains.Substring("\"AgeUpper\":null"));
            Assert.That(json, Contains.Substring("\"MostRecentApprovalDate\":null"));
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

            // Assert
            Assert.IsNotNull(json);
            Assert.That(json, Contains.Substring("\"InstallId\":null"));
            Assert.That(json, Contains.Substring("\"UserStatus\":null"));
            Assert.That(json, Contains.Substring("\"AgeLower\":null"));
            Assert.That(json, Contains.Substring("\"AgeUpper\":null"));
            Assert.That(json, Contains.Substring("\"MostRecentApprovalDate\":null"));
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

                // Assert
                Assert.That(json, Contains.Substring($"\"UserStatus\":\"{expectedValue}\""), 
                    $"Expected UserStatus {status} to serialize as \"{expectedValue}\"");
            }
        }

        [Test]
        public void ToJson_WithDateTimeInDifferentFormats_UsesIso8601Format()
        {
            // Arrange
            var testDate = new DateTime(2023, 12, 25, 10, 30, 45, 123, DateTimeKind.Utc);
            _ageSignalsResult.MostRecentApprovalDate = testDate;
            _ageSignalsResult.InstallId = "date-test-id";

            // Act
            var json = _ageSignalsResult.ToJson();

            // Assert
            Assert.IsNotNull(json);
            Assert.That(json, Contains.Substring("\"MostRecentApprovalDate\":\"2023-12-25T10:30:45.123Z\""));
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
            Assert.IsNotNull(json);
            Assert.That(json, Contains.Substring("\"AgeLower\":-1"));
            Assert.That(json, Contains.Substring("\"AgeUpper\":-5"));
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
            Assert.IsNotNull(json);
            Assert.That(json, Contains.Substring("\"AgeLower\":0"));
            Assert.That(json, Contains.Substring("\"AgeUpper\":0"));
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
            Assert.That(json, Contains.Substring("\"InstallId\":\"\""));
        }

        [Test]
        public void ToJson_WithSpecialCharactersInInstallId_EscapesCorrectly()
        {
            // Arrange
            _ageSignalsResult.InstallId = "test\"id\\with/special\ncharacters\t";

            // Act
            var json = _ageSignalsResult.ToJson();

            // Assert
            Assert.IsNotNull(json);
            // The JSON should escape the special characters
            Assert.That(json, Contains.Substring("\"InstallId\":\"test\\\"id\\\\with/special\\ncharacters\\t\""));
        }
    }
}
