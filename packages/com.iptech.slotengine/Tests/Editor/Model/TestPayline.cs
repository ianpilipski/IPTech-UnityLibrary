using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using NUnit.Framework;
using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Model;
using IPTech.Utils;
using UnityEngine;

namespace IPTech.SlotEngine.Tests.Model
{
	[TestFixture]
	class TestPayline
	{
		[Test]
		public void TestThatSettingAndGettingPaylineEntries_ReturnsSameReference() {
			List<IPaylineEntry> paylinesList = new List<IPaylineEntry>();
			Payline payline = new Payline();
			payline.PaylineEntries = paylinesList;
			Assert.AreSame(paylinesList, payline.PaylineEntries);
		}

		[Test]
		public void TestSerializingAndDeserializing_CreatesEquivalentObject() {
			Payline payline = new Payline()
			{
				PaylineEntries = new List<IPaylineEntry>()
				{
					new PaylineEntry() { ReelColumn = 0, ReelRow = 0 },
					new PaylineEntry() { ReelColumn = 1, ReelRow = 1 }
				}
			};
			var json = JsonUtility.ToJson(payline);
			Payline deserializedPayline = JsonUtility.FromJson<Payline>(json);
			Assert.AreEqual(payline.PaylineEntries.Count, deserializedPayline.PaylineEntries.Count);
			for(int i = 0; i < payline.PaylineEntries.Count; i++) {
				Assert.AreEqual(payline.PaylineEntries[i].ReelColumn, deserializedPayline.PaylineEntries[i].ReelColumn);
				Assert.AreEqual(payline.PaylineEntries[i].ReelRow, deserializedPayline.PaylineEntries[i].ReelRow);
			}
		}
	}
}
