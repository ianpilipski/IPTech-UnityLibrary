using NUnit.Framework;

namespace IPTech.Types.Tests
{
	[TestFixture]
	public class TestRName
	{
		const string MY_TEST_STRING_NAME = "MyTestStringName";
		const string MY_OTHER_TEST_STRING_NAME = "MyOtherTestStringName";

		[Test]
		public void WhenConstructedWithString_ToStringReturnsSameCaseString() {
			RName intName = RName.Name(MY_TEST_STRING_NAME);
			Assert.AreEqual(MY_TEST_STRING_NAME, intName.ToString());
		}

		[Test]
		public void WhenConstructedTwice_ReferenceIsSame() {
			RName intName1 = RName.Name(MY_TEST_STRING_NAME);
			RName intName2 = RName.Name(MY_TEST_STRING_NAME);

			Assert.AreSame(intName1, intName2);
		}

		[Test]
		public void WhenConstructedMultipleTimes_TheCacheSizeDoesNotGrow() {
			int originalSize = RName.CacheSize();
			RName.Name(MY_TEST_STRING_NAME);
			RName.Name(MY_TEST_STRING_NAME);

			int cacheGrowth = RName.CacheSize() - originalSize;

			Assert.IsTrue(cacheGrowth <= 1);
		}

		[Test]
		public void WhenComparedCallingEqual_SuccedsWhenTheSame() {
			RName intName1 = RName.Name(MY_TEST_STRING_NAME);
			RName intName2 = RName.Name(MY_TEST_STRING_NAME);

			Assert.IsTrue(intName1.Equals(intName2));
		}

		[Test]
		public void WhenComparedCallingEqual_FailesWhenDifferent() {
			RName intName1 = RName.Name(MY_TEST_STRING_NAME);
			RName intName2 = RName.Name(MY_OTHER_TEST_STRING_NAME);

			Assert.IsFalse(intName1.Equals(intName2));
		}

		[Test]
		public void WhenComparedUsingEqualityOperator_SuccedsWhenTheSame() {
			RName intName1 = RName.Name(MY_TEST_STRING_NAME);
			RName intName2 = RName.Name(MY_TEST_STRING_NAME);

			Assert.IsTrue(intName1 == intName2);
		}

		[Test]
		public void WhenComparedUsingEqualityOperator_FailesWhenDifferent() {
			RName intName1 = RName.Name(MY_TEST_STRING_NAME);
			RName intName2 = RName.Name(MY_OTHER_TEST_STRING_NAME);

			Assert.IsFalse(intName1 == intName2);
		}
	}
}