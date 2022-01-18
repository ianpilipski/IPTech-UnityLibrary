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

		public static IList<IPayline>[] PaylineListTestData = new IList<IPayline>[] { null, new List<IPayline>(), new List<IPayline>() };

		[SetUp]
		public void SetUp() {
			this.paylineSet = new PaylineSet();
		}

		[Test, TestCaseSource("PaylineListTestData")]
		public void SettingAndGettingPaylines_ReturnsTheSameReference(IList<IPayline> paylineList) {
			this.paylineSet.PayLines = paylineList;
			Assert.AreSame(paylineList, this.paylineSet.PayLines);
		}

		[Test]
		public void Serializing_CompaitibleWithPreviousVersion() {
			ObjectSerializer<PaylineSet> paylineSetSerializer = new ObjectSerializer<PaylineSet>() { TargetObject = this.paylineSet };
			paylineSetSerializer.SerializeBinaryObject();
			Assert.AreEqual(SERIALIZED_HEX_STRING, paylineSetSerializer.HexString);
		}

		const string SERIALIZED_HEX_STRING =
			"00-01-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-0C-02-00-00-00-0F-41-73-73-65-6D-62-6C-79-" +
			"2D-43-53-68-61-72-70-05-01-00-00-00-22-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-" +
			"4D-6F-64-65-6C-2E-50-61-79-6C-69-6E-65-53-65-74-01-00-00-00-19-3C-50-61-79-4C-69-6E-65-73-3E-" +
			"6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-03-92-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-" +
			"74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-49-4C-69-73-74-60-31-5B-5B-49-50-54-65-63-68-2E-53-" +
			"6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-50-61-79-6C-69-6E-65-2C-20-41-" +
			"73-73-65-6D-62-6C-79-2D-43-53-68-61-72-70-2C-20-56-65-72-73-69-6F-6E-3D-30-2E-30-2E-30-2E-30-" +
			"2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-" +
			"65-6E-3D-6E-75-6C-6C-5D-5D-02-00-00-00-09-03-00-00-00-04-03-00-00-00-91-01-53-79-73-74-65-6D-" +
			"2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-4C-69-73-74-60-31-5B-5B-49-50-" +
			"54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-50-61-79-6C-" +
			"69-6E-65-2C-20-41-73-73-65-6D-62-6C-79-2D-43-53-68-61-72-70-2C-20-56-65-72-73-69-6F-6E-3D-30-" +
			"2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-" +
			"4B-65-79-54-6F-6B-65-6E-3D-6E-75-6C-6C-5D-5D-03-00-00-00-06-5F-69-74-65-6D-73-05-5F-73-69-7A-" +
			"65-08-5F-76-65-72-73-69-6F-6E-04-00-00-26-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-" +
			"2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-50-61-79-6C-69-6E-65-5B-5D-02-00-00-00-08-08-09-04-00-00-" +
			"00-00-00-00-00-00-00-00-00-07-04-00-00-00-00-01-00-00-00-00-00-00-00-04-24-49-50-54-65-63-68-" +
			"2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-50-61-79-6C-69-6E-65-02-" + 
			"00-00-00-0B";
	}
}
