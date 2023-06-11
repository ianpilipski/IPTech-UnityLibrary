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
			ObjectSerializer<PaylineEntry> paylineEntrySerializer = new ObjectSerializer<PaylineEntry>();
			paylineEntrySerializer.TargetObject = this.paylineEntry;
			paylineEntrySerializer.SerializeBinaryObject();
			paylineEntrySerializer.Bytes = paylineEntrySerializer.Bytes;
			paylineEntrySerializer.DeserializeBinaryObject();
			Assert.AreEqual(typeof(PaylineEntry), paylineEntrySerializer.TargetObject.GetType());
		}

		[Test, Ignore("TODO: Fix test")]
		public void DeserializingOlderVersions_ReturnsProperObjects() {
			ObjectSerializer<PaylineEntry> paylineEntrySerializer = new ObjectSerializer<PaylineEntry>();
			paylineEntrySerializer.HexString = SERIALIZED_HEX_STRING;
			paylineEntrySerializer.DeserializeBinaryObject();
			Assert.AreEqual(typeof(PaylineEntry), paylineEntrySerializer.TargetObject.GetType());
		}

		[Test, Ignore("TODO: Fix test")]
		public void SerializingIsCompatibleWithPreviouslySavedFormat() {
			this.paylineEntry.ReelColumn = int.MaxValue;
			this.paylineEntry.ReelRow = int.MinValue;
			ObjectSerializer<PaylineEntry> paylineEntrySerializer = new ObjectSerializer<PaylineEntry>();
			paylineEntrySerializer.TargetObject = this.paylineEntry;
			paylineEntrySerializer.SerializeBinaryObject();
			Assert.AreEqual(SERIALIZED_HEX_STRING, paylineEntrySerializer.HexString);
		}

		const string SERIALIZED_HEX_STRING = 
			"00-01-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-0C-02-00-00-00-0F-41-73-73-65-6D-62-6C-79-2D-43-" +
			"53-68-61-72-70-05-01-00-00-00-24-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-" +
			"6C-2E-50-61-79-6C-69-6E-65-45-6E-74-72-79-02-00-00-00-1B-3C-52-65-65-6C-43-6F-6C-75-6D-6E-3E-6B-5F-" +
			"5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-18-3C-52-65-65-6C-52-6F-77-3E-6B-5F-5F-42-61-63-6B-69-6E-67-" + 
			"46-69-65-6C-64-00-00-08-08-02-00-00-00-FF-FF-FF-7F-00-00-00-80-0B";
	}
}
