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
	class TestReelSet
	{
		const string TESTID = "My Test ID: abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()`~[]\\{}|;':\",./<>?";

		static Reel[] TESTREELS = new Reel[] {
			new Reel() { ID = "reel1" },
			new Reel() { ID = "reel2" },
			new Reel() { ID = "reel3" },
			new Reel() { ID = "reel4" },
			new Reel() { ID = "reel5" },
			new Reel() { ID = "reel6" }
		};

		private ReelSet TESTREELSET;

		[SetUp]
		public void Setup() {
			this.TESTREELSET = new ReelSet() { ID = TESTID };
			foreach(Reel reel in TESTREELS) {
				this.TESTREELSET.Add(reel);
			}
		}

		[Test]
		public void SettingAndGettingPropertyIDIsReturningTheValueSet() {
			Assert.AreEqual(TESTID, this.TESTREELSET.ID);
		}

		[Test]
		public void AddingReelsToCollectionProducesCollectionInSameOrderAsAddedWhenIteratedWithEnumerator() {
			int i = 0;
			foreach(Reel reel in this.TESTREELSET) {
				Assert.AreSame(TESTREELS[i++], reel);
			}
		}

		[Test]
		public void AddingReelsToCollectionProducesCollectionInSameOrderAsAddedWhenIteratedWithIndexer() {
			for(int i=0; i<TESTREELS.Length;i++) {
				Assert.AreSame(TESTREELS[i], this.TESTREELSET[i]);
			}
		}

		[Test]
		public void SerializationAndDeserializationProduceAnEquivalentObject() {
			var json = JsonUtility.ToJson(this.TESTREELSET);
			ReelSet deserializedReelSet = JsonUtility.FromJson<ReelSet>(json);

			Assert.AreEqual(this.TESTREELSET.ID, deserializedReelSet.ID);
			Assert.AreEqual(this.TESTREELSET.Count, deserializedReelSet.Count);
			for(int i = 0; i < this.TESTREELSET.Count; i++) {
				Assert.AreEqual(this.TESTREELSET[i].ID, deserializedReelSet[i].ID);
			}
		}
	}
}
