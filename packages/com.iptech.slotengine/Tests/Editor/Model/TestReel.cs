using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using IPTech.SlotEngine.Model;
using IPTech.SlotEngine.Model.Api;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using IPTech.Utils;

namespace IPTech.SlotEngine.Tests.Model
{
	[TestFixture]
	class TestReel
	{
		const string TESTID = "My Test ID: abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()`~[]\\{}|;':\",./<>?";

		private static ISymbol[] TEST_SYMBOLS = new ISymbol[] {
			new Symbol(),
			new Symbol(),
			new Symbol(),
		};

		private Reel reel;

		[SetUp]
		public void Setup() {
			this.reel = new Reel() { ID = TESTID };
			foreach (ISymbol symbol in TEST_SYMBOLS) {
				this.reel.Add(symbol);
			}
		}

		[Test]
		public void SettingAndGettingPropertyIDIsReturningTheValueSet() {
			Assert.AreEqual(TESTID, this.reel.ID);
		}

		[Test]
		public void AddingSymbolsToTheReelIsReturningTheSymbolsAddedInTheCorrectOrder() {
			int i = 0;
			foreach(ISymbol symbol in TEST_SYMBOLS) {
				Assert.AreSame(symbol, this.reel[i++]);
			}
		}

		[Test]
		public void SerializingAndDeserializingProduceTheSameValues() {
			var json = JsonUtility.ToJson(this.reel);
			Reel newReel = JsonUtility.FromJson<Reel>(json);
			Assert.AreEqual(this.reel.Count, newReel.Count);
			Assert.AreEqual(this.reel.ID, newReel.ID);
		}
	}
}
