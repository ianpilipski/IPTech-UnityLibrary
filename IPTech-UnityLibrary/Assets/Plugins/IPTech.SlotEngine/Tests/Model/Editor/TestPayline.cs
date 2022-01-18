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
			IList<IPaylineEntry> paylinesList = new List<IPaylineEntry>();
			Payline payline = new Payline();
			payline.PaylineEntries = paylinesList;
			Assert.AreSame(paylinesList, payline.PaylineEntries);
		}

		[Test]
		public void TestSerializingAndDeserializing_CreatesEquivalentObject() {
			Payline payline = new Payline();
			ObjectSerializer<Payline> paylineSerializer = new ObjectSerializer<Payline>() { TargetObject = payline };
			paylineSerializer.SerializeBinaryObject();
			paylineSerializer.Bytes = paylineSerializer.Bytes;
			paylineSerializer.DeserializeBinaryObject();
			Assert.AreEqual(payline.GetType(), paylineSerializer.TargetObject.GetType());
		}

		[Test]
		public void TestDeserializingOlderVersion_CreatesProperObject() {
			Payline payline = new Payline();
			ObjectSerializer<Payline> paylineSerializer = new ObjectSerializer<Payline>();
			paylineSerializer.HexString = SERIALIZED_HEX_PAYLINE;
			paylineSerializer.DeserializeBinaryObject();
			Assert.AreEqual(payline.GetType(), paylineSerializer.TargetObject.GetType());
		}

		const string SERIALIZED_HEX_PAYLINE = 
			"00-01-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-0C-02-00-00-00-0F-41-73-73-65-6D-62-6C-79-2D-43-53-68-61-" +
			"72-70-05-01-00-00-00-1F-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-50-61-79-6C-" +
			"69-6E-65-01-00-00-00-1F-3C-50-61-79-6C-69-6E-65-45-6E-74-72-69-65-73-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-" +
			"65-6C-64-03-97-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-49-4C-69-" +
			"73-74-60-31-5B-5B-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-50-" +
			"61-79-6C-69-6E-65-45-6E-74-72-79-2C-20-41-73-73-65-6D-62-6C-79-2D-43-53-68-61-72-70-2C-20-56-65-72-73-69-6F-" +
			"6E-3D-30-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-" +
			"79-54-6F-6B-65-6E-3D-6E-75-6C-6C-5D-5D-02-00-00-00-09-03-00-00-00-04-03-00-00-00-96-01-53-79-73-74-65-6D-2E-" +
			"43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-4C-69-73-74-60-31-5B-5B-49-50-54-65-63-68-2E-53-" +
			"6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-50-61-79-6C-69-6E-65-45-6E-74-72-79-2C-20-41-" +
			"73-73-65-6D-62-6C-79-2D-43-53-68-61-72-70-2C-20-56-65-72-73-69-6F-6E-3D-30-2E-30-2E-30-2E-30-2C-20-43-75-6C-" +
			"74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-6E-75-6C-6C-5D-5D-03-" +
			"00-00-00-06-5F-69-74-65-6D-73-05-5F-73-69-7A-65-08-5F-76-65-72-73-69-6F-6E-04-00-00-2B-49-50-54-65-63-68-2E-" +
			"53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-50-61-79-6C-69-6E-65-45-6E-74-72-79-5B-5D-" +
			"02-00-00-00-08-08-09-04-00-00-00-00-00-00-00-00-00-00-00-07-04-00-00-00-00-01-00-00-00-00-00-00-00-04-29-49-" +
			"50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-50-61-79-6C-69-6E-65-45-" + 
			"6E-74-72-79-02-00-00-00-0B";
	}
}
