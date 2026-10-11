using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using IPTech.SlotEngine.Model;
using IPTech.SlotEngine.Model.Api;
using IPTech.Utils;

namespace IPTech.SlotEngine.Tests.Model
{
	[TestFixture]
	class TestPaylineSet
	{
		private PaylineSet paylineSet;

		public static List<IPayline>[] PaylineListTestData = new List<IPayline>[] { null, new List<IPayline>(), new List<IPayline>() };

		[SetUp]
		public void SetUp() {
			this.paylineSet = new PaylineSet();
		}

		[Test, TestCaseSource("PaylineListTestData")]
		public void SettingAndGettingPaylines_ReturnsTheSameReference(List<IPayline> paylineList) {
			this.paylineSet.PayLines = paylineList;
			Assert.AreSame(paylineList, this.paylineSet.PayLines);
		}

		[Test]
		public void SerializingAndDeserializing_ReturnsEquivalentObject()
		{
			this.paylineSet.PayLines = new List<IPayline>()
			{
				new Payline()
				{
					PaylineEntries = new List<IPaylineEntry>()
					{
						new PaylineEntry() { ReelColumn = 0, ReelRow = 0 },
						new PaylineEntry() { ReelColumn = 1, ReelRow = 1 }
					}
				}
			};
			var json = JsonUtility.ToJson(this.paylineSet);
			PaylineSet deserializedPaylineSet = JsonUtility.FromJson<PaylineSet>(json);
			Assert.AreEqual(this.paylineSet.PayLines.Count, deserializedPaylineSet.PayLines.Count);
			for(int i = 0; i < this.paylineSet.PayLines.Count; i++) {
				Assert.AreEqual(this.paylineSet.PayLines[i].PaylineEntries.Count, deserializedPaylineSet.PayLines[i].PaylineEntries.Count);
				for(int j = 0; j < this.paylineSet.PayLines[i].PaylineEntries.Count; j++) {
					Assert.AreEqual(this.paylineSet.PayLines[i].PaylineEntries[j].ReelColumn, deserializedPaylineSet.PayLines[i].PaylineEntries[j].ReelColumn);
					Assert.AreEqual(this.paylineSet.PayLines[i].PaylineEntries[j].ReelRow, deserializedPaylineSet.PayLines[i].PaylineEntries[j].ReelRow);
				}
			}
		}
	}
}
