using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using IPTech.Utils;
using UnityEngine;
using IPTech.SlotEngine.Model;

namespace IPTech.SlotEngine.Tests.Model
{
	[TestFixture]
	class TestWildSymbol
	{
		private WildSymbol wildSymbol;
		private ObjectSerializer<WildSymbol> wildSymbolSerializer;

		public static string[] idData = new string[] {
			null,
			"abc",
			"123",
			"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ`1234567890-=+_)(*&^%$#@!~[]\\|}{;:'\",./<>?",
			"  helo  goodby  ",
		};

		[SetUp]
		public void SetUp() {
			this.wildSymbol = new WildSymbol();
			this.wildSymbolSerializer = new ObjectSerializer<WildSymbol>() { TargetObject = this.wildSymbol };
		}

		[Test, TestCaseSource("idData")]
		public void SettingAndGettingTheID_ReturnsTheSameValue(string value) {
			this.wildSymbol.ID = value;
			Assert.AreEqual(value, this.wildSymbol.ID);
		}

		[Test, TestCaseSource("idData")]
		public void SerializingAndDeserializing_ReturnsTheSameValues(string value) {
			this.wildSymbol.ID = value;
			this.wildSymbolSerializer.SerializeBinaryObject();
			this.wildSymbolSerializer.Bytes = this.wildSymbolSerializer.Bytes;
			this.wildSymbolSerializer.DeserializeBinaryObject();
			Assert.AreEqual(value, this.wildSymbolSerializer.TargetObject.ID);
		}

		[Test]
		public void SerializationWithOlderVersion_IsCompatible() {
			this.wildSymbolSerializer.SerializeBinaryObject();
			Assert.AreEqual(SERIALIZED_HEX_STRING, this.wildSymbolSerializer.HexString);
		}

		const string SERIALIZED_HEX_STRING = 
			"00-01-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-0C-02-00-00-00-0F-41-73-73-65-6D-62-6C-79-" +
			"2D-43-53-68-61-72-70-05-01-00-00-00-22-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-" +
			"4D-6F-64-65-6C-2E-57-69-6C-64-53-79-6D-62-6F-6C-01-00-00-00-13-3C-49-44-3E-6B-5F-5F-42-61-63-" + 
			"6B-69-6E-67-46-69-65-6C-64-01-02-00-00-00-0A-0B";
	}
}
