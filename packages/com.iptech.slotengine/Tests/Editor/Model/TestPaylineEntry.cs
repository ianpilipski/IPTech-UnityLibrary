using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using IPTech.SlotEngine.Model;
using IPTech.Utils;
using UnityEngine;

namespace IPTech.SlotEngine.Tests.Model
{
	[TestFixture]
	class TestPaylineEntry
	{
		private PaylineEntry paylineEntry;

		public static int[] ValidIntegerList = new int[] { int.MinValue, -666, 0, 666, int.MaxValue };
		
		[SetUp]
		public void SetUp() {
			this.paylineEntry = new PaylineEntry();
		}

		[Test, TestCaseSource("ValidIntegerList")]
		public void TestSettingAndGettingReelColumn_ReturnSameValue(int value) {
			this.paylineEntry.ReelColumn = value;
			Assert.AreEqual(value, this.paylineEntry.ReelColumn);
			Assert.AreEqual(0, this.paylineEntry.ReelRow);
		}

		[Test, TestCaseSource("ValidIntegerList")]
		public void TestSettingAndGettingReelRow_ReturnsSameValue(int value) {
			this.paylineEntry.ReelRow = value;
			Assert.AreEqual(value, this.paylineEntry.ReelRow);
			Assert.AreEqual(0, this.paylineEntry.ReelColumn);
		}

		[Test]
		public void SerializingAndDeserializing_ReturnsEquivalentObject() {
			this.paylineEntry.ReelColumn = int.MaxValue;
			this.paylineEntry.ReelRow = int.MinValue;
			var json = JsonUtility.ToJson(this.paylineEntry);
			PaylineEntry deserializedPaylineEntry = JsonUtility.FromJson<PaylineEntry>(json);
			Assert.AreEqual(this.paylineEntry.ReelColumn, deserializedPaylineEntry.ReelColumn);
			Assert.AreEqual(this.paylineEntry.ReelRow, deserializedPaylineEntry.ReelRow);
		}
	}
}
