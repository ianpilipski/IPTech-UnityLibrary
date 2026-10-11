using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using IPTech.SlotEngine.Model;
using IPTech.Utils;
using IPTech.SlotEngine.Model.Api;
using UnityEngine;

namespace IPTech.SlotEngine.Tests.Model
{
	[TestFixture]
	class TestPayoutTable
	{
		private PayoutTable payoutTable;
		
		public static string[] payoutTableIDList = new string[] { null, "abc", "qwerty",
			"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-=+_)(*&^%$#@![]\\{}|`~,./<>?;:'\"" };

		public static List<IPayoutTableEntry>[] payoutTableEntryLists = new List<IPayoutTableEntry>[] {
			null, new List<IPayoutTableEntry>(), new List<IPayoutTableEntry>()
		};

		[SetUp]
		public void SetUp() {
			this.payoutTable = new PayoutTable();
		}

		[Test, TestCaseSource("payoutTableIDList")]
		public void SettingAndGettingID_ReturnsSameValue(string ID) {
			this.payoutTable.ID = ID;
			Assert.AreEqual(ID, this.payoutTable.ID);
		}

		[Test, TestCaseSource("payoutTableEntryLists")]
		public void SettingAndGettingPayoutTableEntries_ReturnsTheSameReference(List<IPayoutTableEntry> list) {
			this.payoutTable.PayoutTableEntries = list;
			Assert.AreSame(list, this.payoutTable.PayoutTableEntries);
		}

		[Test]
		public void EnsureDefaultPayoutTableEntries_IsNotNull() {
			Assert.NotNull(this.payoutTable.PayoutTableEntries);
		}

		[Test]
		public void SerializingAndDeserializing_ReturnsEquivalentObject() {
			this.payoutTable.PayoutTableEntries = new List<IPayoutTableEntry>()
			{
				new PayoutTableEntry() { ID = "entry1", PayoutMultiplier = 100 },
				new PayoutTableEntry() { ID = "entry2", PayoutMultiplier = 200 }
			};
			var json = JsonUtility.ToJson(this.payoutTable);
			PayoutTable deserializedPayoutTable = JsonUtility.FromJson<PayoutTable>(json);
			Assert.AreEqual(this.payoutTable.PayoutTableEntries.Count, deserializedPayoutTable.PayoutTableEntries.Count);
			for(int i = 0; i < this.payoutTable.PayoutTableEntries.Count; i++) {
				Assert.AreEqual(this.payoutTable.PayoutTableEntries[i].ID, deserializedPayoutTable.PayoutTableEntries[i].ID);
				Assert.AreEqual(this.payoutTable.PayoutTableEntries[i].PayoutMultiplier, deserializedPayoutTable.PayoutTableEntries[i].PayoutMultiplier);
			}
		}
	}
}
