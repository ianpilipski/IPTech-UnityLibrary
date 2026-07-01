using System.Threading;
using IPTech.AgeVerification.iOS;
using NUnit.Framework;


namespace Tests
{
	[TestFixture]
	public class AgeRangeApiTests
	{
		private AgeRangeApi _api;
		private bool _originalMockMode;

		[SetUp]
		public void SetUp()
		{
			_api = new AgeRangeApi();
			_originalMockMode = AgeRangeApi.EnableMockMode;
			AgeRangeApi.EnableMockMode = false;
		}

		[TearDown]
		public void TearDown()
		{
			AgeRangeApi.EnableMockMode = _originalMockMode;
		}

		[Test]
		public void RequestAgeRange_RequiredMinAgeAbove18_ThrowsInvalidRequestException()
		{
			var ex = Assert.Throws<AgeRangeInvalidRequestException>(
				() => _api.RequestAgeRange(21, CancellationToken.None).GetAwaiter().GetResult());
			Assert.That(ex.Message, Does.Contain("21"));
			Assert.That(ex.Message, Does.Contain("requiredMinAge"));
		}

		[Test]
		public void RequestAgeRange_AdditionalMinAge1Above18_ThrowsInvalidRequestException()
		{
			var ex = Assert.Throws<AgeRangeInvalidRequestException>(
				() => _api.RequestAgeRange(13, CancellationToken.None, additionalMinAge1: 21).GetAwaiter().GetResult());
			Assert.That(ex.Message, Does.Contain("21"));
			Assert.That(ex.Message, Does.Contain("additionalMinAge1"));
		}

		[Test]
		public void RequestAgeRange_AdditionalMinAge2Above18_ThrowsInvalidRequestException()
		{
			var ex = Assert.Throws<AgeRangeInvalidRequestException>(
				() => _api.RequestAgeRange(13, CancellationToken.None, additionalMinAge2: 25).GetAwaiter().GetResult());
			Assert.That(ex.Message, Does.Contain("25"));
			Assert.That(ex.Message, Does.Contain("additionalMinAge2"));
		}

		[Test]
		public void RequestAgeRange_RequiredMinAgeLessThan1_ThrowsInvalidRequestException()
		{
			var ex = Assert.Throws<AgeRangeInvalidRequestException>(
				() => _api.RequestAgeRange(0, CancellationToken.None).GetAwaiter().GetResult());
			Assert.That(ex.Message, Does.Contain("requiredMinAge"));
		}

		[Test]
		public void RequestAgeRange_RequiredMinAgeAt18_DoesNotThrowValidationError()
		{
			// Age 18 is valid — should not throw AgeRangeInvalidRequestException.
			// It may throw PlatformNotSupportedException in editor without iOS target, which is expected.
			try
			{
				_api.RequestAgeRange(18, CancellationToken.None).GetAwaiter().GetResult();
			}
			catch (AgeRangeInvalidRequestException)
			{
				Assert.Fail("Should not throw AgeRangeInvalidRequestException for age gate value of 18.");
			}
			catch
			{
				// Other exceptions (PlatformNotSupportedException, etc.) are expected in test environment
			}
		}

		[Test]
		public void RequestAgeRange_AdditionalMinAgeZero_IsIgnored()
		{
			// additionalMinAge1=0 means "not used" and should not be validated
			try
			{
				_api.RequestAgeRange(13, CancellationToken.None, additionalMinAge1: 0);
			}
			catch (AgeRangeInvalidRequestException)
			{
				Assert.Fail("Should not throw AgeRangeInvalidRequestException for unused age gate (value 0).");
			}
			catch
			{
				// Other exceptions are expected in test environment
			}
		}
	}
}
