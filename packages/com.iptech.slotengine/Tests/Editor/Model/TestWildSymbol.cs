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
		
		public static string[] idData = new string[] {
			"abc",
			"123",
			"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ`1234567890-=+_)(*&^%$#@!~[]\\|}{;:'\",./<>?",
			"  helo  goodby  ",
		};

		[SetUp]
		public void SetUp() {
			this.wildSymbol = new WildSymbol();
		}

		[Test, TestCaseSource("idData")]
		public void SettingAndGettingTheID_ReturnsTheSameValue(string value) {
			this.wildSymbol.ID = value;
			Assert.AreEqual(value, this.wildSymbol.ID);
		}

		[Test, TestCaseSource("idData")]
		public void SerializingAndDeserializing_ReturnsTheSameValues(string value) {
			this.wildSymbol.ID = value;
			var json = JsonUtility.ToJson(this.wildSymbol);
			var deserializedWildSymbol = JsonUtility.FromJson<WildSymbol>(json);
			Assert.AreEqual(value, deserializedWildSymbol.ID);
		}
	}
}
