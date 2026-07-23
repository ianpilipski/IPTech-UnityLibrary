using IPTech.AgeVerification.iOS;
using NUnit.Framework;


namespace Tests
{
    [TestFixture]
    public class AgeRangeResultTests
    {
        [Test]
        public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            var status = AgeRangeResultStatus.Success;
            int? lowerBound = 13;
            int? upperBound = 17;
            var ageDeclaration = AgeDeclaration.SelfDeclared;

            // Act
            var result = new AgeRangeResult(status, lowerBound, upperBound, ageDeclaration);

            // Assert
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(lowerBound, result.LowerBound);
            Assert.AreEqual(upperBound, result.UpperBound);
            Assert.AreEqual(ageDeclaration, result.AgeDeclaration);
        }

        [Test]
        public void Constructor_WithUserDeclinedStatus_SetsPropertiesCorrectly()
        {
            // Arrange
            var status = AgeRangeResultStatus.UserDeclined;
            int? lowerBound = 0;
            int? upperBound = 0;
            var ageDeclaration = AgeDeclaration.Unknown;

            // Act
            var result = new AgeRangeResult(status, lowerBound, upperBound, ageDeclaration);

            // Assert
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(lowerBound, result.LowerBound);
            Assert.AreEqual(upperBound, result.UpperBound);
            Assert.AreEqual(ageDeclaration, result.AgeDeclaration);
        }

        [Test]
        public void Constructor_WithUnsupportedPlatformStatus_SetsPropertiesCorrectly()
        {
            // Arrange
            var status = AgeRangeResultStatus.UnsupportedPlatformVersion;
            int? lowerBound = 0;
            int? upperBound = 0;
            var ageDeclaration = AgeDeclaration.Unknown;

            // Act
            var result = new AgeRangeResult(status, lowerBound, upperBound, ageDeclaration);

            // Assert
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(lowerBound, result.LowerBound);
            Assert.AreEqual(upperBound, result.UpperBound);
            Assert.AreEqual(ageDeclaration, result.AgeDeclaration);
        }

        [Test]
        public void Constructor_WithGuardianDeclaredAgeDeclaration_SetsPropertiesCorrectly()
        {
            // Arrange
            var status = AgeRangeResultStatus.Success;
            int? lowerBound = 5;
            int? upperBound = 12;
            var ageDeclaration = AgeDeclaration.GuardianDeclared;

            // Act
            var result = new AgeRangeResult(status, lowerBound, upperBound, ageDeclaration);

            // Assert
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(lowerBound, result.LowerBound);
            Assert.AreEqual(upperBound, result.UpperBound);
            Assert.AreEqual(ageDeclaration, result.AgeDeclaration);
        }

        [Test]
        public void Constructor_WithNullBounds_SetsPropertiesCorrectly()
        {
            // Arrange
            var status = AgeRangeResultStatus.UserDeclined;
            int? lowerBound = null;
            int? upperBound = null;
            var ageDeclaration = AgeDeclaration.Unknown;

            // Act
            var result = new AgeRangeResult(status, lowerBound, upperBound, ageDeclaration);

            // Assert
            Assert.AreEqual(status, result.Status);
            Assert.AreEqual(lowerBound, result.LowerBound);
            Assert.AreEqual(upperBound, result.UpperBound);
            Assert.AreEqual(ageDeclaration, result.AgeDeclaration);
        }

        [Test]
        public void ToJson_WithNullBounds_ReturnsJsonWithNullValues()
        {
            // Arrange
            var result = new AgeRangeResult(AgeRangeResultStatus.UserDeclined, null, null, AgeDeclaration.Unknown);

            // Act
            var json = result.ToJson(prettyPrint: false);

            // Assert
            var expectedJson = CreateExpectedJson(AgeRangeResultStatus.UserDeclined, null, null, AgeDeclaration.Unknown);
            Assert.AreEqual(expectedJson, json);
        }

        [Test]
        public void ToJson_WithPrettyPrintFalse_ReturnsCompactJson()
        {
            // Arrange
            var result = new AgeRangeResult(AgeRangeResultStatus.Success, 13, 17, AgeDeclaration.SelfDeclared);

            // Act
            var json = result.ToJson(prettyPrint: false);

            // Assert
            var expectedJson = CreateExpectedJson(AgeRangeResultStatus.Success, 13, 17, AgeDeclaration.SelfDeclared);
            Assert.AreEqual(expectedJson, json);
        }

        [Test]
        public void ToJson_WithUserDeclinedStatus_ReturnsCorrectJson()
        {
            // Arrange
            var result = new AgeRangeResult(AgeRangeResultStatus.UserDeclined, 0, 0, AgeDeclaration.Unknown);

            // Act
            var json = result.ToJson(prettyPrint: false);

            // Assert
            var expectedJson = CreateExpectedJson(AgeRangeResultStatus.UserDeclined, 0, 0, AgeDeclaration.Unknown);
            Assert.AreEqual(expectedJson, json);
        }

        [Test]
        public void ToJson_WithUnsupportedPlatformStatus_ReturnsCorrectJson()
        {
            // Arrange
            var result = new AgeRangeResult(AgeRangeResultStatus.UnsupportedPlatformVersion, 0, 0, AgeDeclaration.Unknown);

            // Act
            var json = result.ToJson(prettyPrint: false);

            // Assert
            var expectedJson = CreateExpectedJson(AgeRangeResultStatus.UnsupportedPlatformVersion, 0, 0, AgeDeclaration.Unknown);
            Assert.AreEqual(expectedJson, json);
        }

        [Test]
        public void ToJson_WithGuardianDeclared_ReturnsCorrectJson()
        {
            // Arrange
            var result = new AgeRangeResult(AgeRangeResultStatus.Success, 5, 12, AgeDeclaration.GuardianDeclared);

            // Act
            var json = result.ToJson(prettyPrint: false);

            // Assert
            var expectedJson = CreateExpectedJson(AgeRangeResultStatus.Success, 5, 12, AgeDeclaration.GuardianDeclared);
            Assert.AreEqual(expectedJson, json);
        }

        private string CreateExpectedJson(AgeRangeResultStatus status, int? lowerBound, int? upperBound, AgeDeclaration ageDeclaration)
        {
            return $"{{\"Status\":{((int)status)},\"_lowerBound\":{lowerBound ?? 0},\"_lowerBoundHasValue\":{(lowerBound != null).ToString().ToLower()},\"_upperBound\":{upperBound ?? 0},\"_upperBoundHasValue\":{(upperBound != null).ToString().ToLower()},\"AgeDeclaration\":{((int)ageDeclaration)}}}";
        }

        [Test]
        public void ToString_WithSuccessStatus_ReturnsCorrectString()
        {
            // Arrange
            var result = new AgeRangeResult(AgeRangeResultStatus.Success, 13, 17, AgeDeclaration.SelfDeclared);

            // Act
            var stringResult = result.ToString();

            // Assert
            var expected = "AgeRangeResult(Status=Success, LowerBound=13, UpperBound=17, AgeDeclaration=SelfDeclared)";
            Assert.AreEqual(expected, stringResult);
        }

        [Test]
        public void ToString_WithUserDeclinedStatus_ReturnsCorrectString()
        {
            // Arrange
            var result = new AgeRangeResult(AgeRangeResultStatus.UserDeclined, 0, 0, AgeDeclaration.Unknown);

            // Act
            var stringResult = result.ToString();

            // Assert
            var expected = "AgeRangeResult(Status=UserDeclined, LowerBound=0, UpperBound=0, AgeDeclaration=Unknown)";
            Assert.AreEqual(expected, stringResult);
        }

        [Test]
        public void ToString_WithNullBounds_ReturnsCorrectString()
        {
            // Arrange
            var result = new AgeRangeResult(AgeRangeResultStatus.UserDeclined, null, null, AgeDeclaration.Unknown);

            // Act
            var stringResult = result.ToString();

            // Assert
            var expected = "AgeRangeResult(Status=UserDeclined, LowerBound=, UpperBound=, AgeDeclaration=Unknown)";
            Assert.AreEqual(expected, stringResult);
        }

        [Test]
        public void ToString_WithUnsupportedPlatformStatus_ReturnsCorrectString()
        {
            // Arrange
            var result = new AgeRangeResult(AgeRangeResultStatus.UnsupportedPlatformVersion, 0, 0, AgeDeclaration.Unknown);

            // Act
            var stringResult = result.ToString();

            // Assert
            var expected = "AgeRangeResult(Status=UnsupportedPlatformVersion, LowerBound=0, UpperBound=0, AgeDeclaration=Unknown)";
            Assert.AreEqual(expected, stringResult);
        }

        [Test]
        public void ToString_WithGuardianDeclared_ReturnsCorrectString()
        {
            // Arrange
            var result = new AgeRangeResult(AgeRangeResultStatus.Success, 5, 12, AgeDeclaration.GuardianDeclared);

            // Act
            var stringResult = result.ToString();

            // Assert
            var expected = "AgeRangeResult(Status=Success, LowerBound=5, UpperBound=12, AgeDeclaration=GuardianDeclared)";
            Assert.AreEqual(expected, stringResult);
        }

        [Test]
        public void Properties_AreReadOnly()
        {
            // Arrange
            var result = new AgeRangeResult(AgeRangeResultStatus.Success, 13, 17, AgeDeclaration.SelfDeclared);

            // Act & Assert - These should compile without setters
            Assert.IsNotNull(result.Status);
            Assert.IsNotNull(result.LowerBound);
            Assert.IsNotNull(result.UpperBound);
            Assert.IsNotNull(result.AgeDeclaration);
        }

        [Test]
        public void Constructor_WithVariousParameterCombinations_AcceptsAllValues()
        {
            // Test case 1: Success with zero bounds
            var result1 = new AgeRangeResult(AgeRangeResultStatus.Success, 0, 0, AgeDeclaration.Unknown);
            Assert.AreEqual(AgeRangeResultStatus.Success, result1.Status);
            Assert.AreEqual(0, result1.LowerBound);
            Assert.AreEqual(0, result1.UpperBound);
            Assert.AreEqual(AgeDeclaration.Unknown, result1.AgeDeclaration);

            // Test case 2: Success with negative bounds
            var result2 = new AgeRangeResult(AgeRangeResultStatus.Success, -1, -1, AgeDeclaration.SelfDeclared);
            Assert.AreEqual(AgeRangeResultStatus.Success, result2.Status);
            Assert.AreEqual(-1, result2.LowerBound);
            Assert.AreEqual(-1, result2.UpperBound);
            Assert.AreEqual(AgeDeclaration.SelfDeclared, result2.AgeDeclaration);

            // Test case 3: UserDeclined with large bounds
            var result3 = new AgeRangeResult(AgeRangeResultStatus.UserDeclined, 100, 150, AgeDeclaration.GuardianDeclared);
            Assert.AreEqual(AgeRangeResultStatus.UserDeclined, result3.Status);
            Assert.AreEqual(100, result3.LowerBound);
            Assert.AreEqual(150, result3.UpperBound);
            Assert.AreEqual(AgeDeclaration.GuardianDeclared, result3.AgeDeclaration);

            // Test case 4: UnsupportedPlatformVersion with wide bounds
            var result4 = new AgeRangeResult(AgeRangeResultStatus.UnsupportedPlatformVersion, 1, 999, AgeDeclaration.Unknown);
            Assert.AreEqual(AgeRangeResultStatus.UnsupportedPlatformVersion, result4.Status);
            Assert.AreEqual(1, result4.LowerBound);
            Assert.AreEqual(999, result4.UpperBound);
            Assert.AreEqual(AgeDeclaration.Unknown, result4.AgeDeclaration);

            // Test case 5: Success with null bounds
            var result5 = new AgeRangeResult(AgeRangeResultStatus.Success, null, null, AgeDeclaration.SelfDeclared);
            Assert.AreEqual(AgeRangeResultStatus.Success, result5.Status);
            Assert.AreEqual(null, result5.LowerBound);
            Assert.AreEqual(null, result5.UpperBound);
            Assert.AreEqual(AgeDeclaration.SelfDeclared, result5.AgeDeclaration);
        }

        [Test]
        public void AgeRangeResultStatus_HasExpectedValues()
        {
            // Assert - Verify enum values exist
            Assert.IsTrue(System.Enum.IsDefined(typeof(AgeRangeResultStatus), AgeRangeResultStatus.Success));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AgeRangeResultStatus), AgeRangeResultStatus.UserDeclined));
            Assert.IsTrue(System.Enum.IsDefined(typeof(AgeRangeResultStatus), AgeRangeResultStatus.UnsupportedPlatformVersion));
        }

        [Test]
        public void AgeDeclaration_HasExpectedValues()
        {
            // Assert - Verify enum values exist and have correct underlying values
            Assert.AreEqual(0, (int)AgeDeclaration.Unknown);
            Assert.AreEqual(1, (int)AgeDeclaration.SelfDeclared);
            Assert.AreEqual(2, (int)AgeDeclaration.GuardianDeclared);
        }
    }
}
