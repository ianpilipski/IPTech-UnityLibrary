using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using IPTech.SlotEngine.Model;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

namespace IPTech.SlotEngine.Tests.Model
{
	[TestFixture]
	class TestSymbol
	{
		private const string TESTID = "My Test ID: abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()`~[]\\{}|;':\",./<>?";
		private const float TESTWEIGHT = 0.75f;

		[Test]
		public void SettingAndGettingPropertyIDIsReturningTheValueSet() {
			Symbol symbol = new Symbol();
			symbol.ID = TESTID;
			Assert.AreEqual(TESTID, symbol.ID);
		}

		public static float[] TESTWEIGHTS = new float[] {
			float.MaxValue, 0.0f, 1.0f, 0.123456789f, 0.987654321f, 123456789.987654321f
		};

		[Test, TestCaseSource("TESTWEIGHTS")]
		public void SettingAndGettingPropertyWeightIsReturningTheValueSet(float weight) {
			Symbol symbol = new Symbol();
			symbol.Weight = weight;
			Assert.AreEqual(weight, symbol.Weight);
		}

		[Test]
		public void SettingTheWeightToANegativeValueThrowsArgumentOutOfRangeException() {
			Symbol symbol = new Symbol();
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                symbol.Weight = float.MinValue;
            });
		}

		[Test]
		public void SerializingAndDeserializingProduceTheSameValues() {
			Symbol symbol = new Symbol() { ID = TESTID, Weight = TESTWEIGHT };
			var json = JsonUtility.ToJson(symbol);
			Symbol newSymbol = JsonUtility.FromJson<Symbol>(json);
			Assert.AreEqual(symbol.ID, newSymbol.ID);
			Assert.AreEqual(symbol.Weight, newSymbol.Weight);
		}
	}
}
