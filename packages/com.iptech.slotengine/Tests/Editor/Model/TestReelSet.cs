using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using IPTech.SlotEngine.Model;
using IPTech.Utils;

namespace IPTech.SlotEngine.Tests.Model
{
	[TestFixture]
	class TestReelSet
	{
		const string TESTID = "My Test ID: abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()`~[]\\{}|;':\",./<>?";

		static Reel[] TESTREELS = new Reel[] {
			new Reel(),
			new Reel(),
			new Reel(),
			new Reel(),
			new Reel(),
			new Reel()
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
			ObjectSerializer<ReelSet> serializer = new ObjectSerializer<ReelSet>() { TargetObject = this.TESTREELSET };
			serializer.SerializeBinaryObject();

			ObjectSerializer<ReelSet> secondarySerializer = new ObjectSerializer<ReelSet>() { Bytes = serializer.Bytes };
			secondarySerializer.DeserializeBinaryObject();

			Assert.AreEqual(this.TESTREELSET.ID, secondarySerializer.TargetObject.ID);
			Assert.AreEqual(this.TESTREELSET.Count, secondarySerializer.TargetObject.Count);
		}

		[Test, Ignore("TODO: Fix test")]
		public void SerializationOfOlderObjectsProducesCorrectResult() {
			ObjectSerializer<ReelSet> serializer = new ObjectSerializer<ReelSet>() { HexString = SERIALIZED_BINARY_TESTREELSET };
			serializer.DeserializeBinaryObject();
			Assert.AreEqual(this.TESTREELSET.ID, serializer.TargetObject.ID);
			Assert.AreEqual(this.TESTREELSET.Count, serializer.TargetObject.Count);
		}

		//[Test]
		public void OuputHexStringOfTestObject() {
			ObjectSerializer<ReelSet> serializer = new ObjectSerializer<ReelSet>() { TargetObject = this.TESTREELSET };
			serializer.SerializeBinaryObject();
			Console.WriteLine(serializer.HexString);
		}

		const string SERIALIZED_BINARY_TESTREELSET =
			"00-01-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-0C-02-00-00-00-0F-41-73-73-65-6D-62-6C-79-2D-43-53-" +
			"68-61-72-70-05-01-00-00-00-1F-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-" +
			"52-65-65-6C-53-65-74-02-00-00-00-16-3C-52-65-65-6C-73-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-" +
			"13-3C-49-44-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-03-01-8F-01-53-79-73-74-65-6D-2E-43-6F-6C-" +
			"6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-49-4C-69-73-74-60-31-5B-5B-49-50-54-65-63-68-2E-53-" +
			"6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-52-65-65-6C-2C-20-41-73-73-65-6D-62-6C-" +
			"79-2D-43-53-68-61-72-70-2C-20-56-65-72-73-69-6F-6E-3D-30-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-" +
			"3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-6E-75-6C-6C-5D-5D-02-00-00-" +
			"00-09-03-00-00-00-06-04-00-00-00-66-4D-79-20-54-65-73-74-20-49-44-3A-20-61-62-63-64-65-66-67-68-69-6A-" +
			"6B-6C-6D-6E-6F-70-71-72-73-74-75-76-77-78-79-7A-41-42-43-44-45-46-47-48-49-4A-4B-4C-4D-4E-4F-50-51-52-" +
			"53-54-55-56-57-58-59-5A-31-32-33-34-35-36-37-38-39-30-21-40-23-24-25-5E-26-2A-28-29-60-7E-5B-5D-5C-7B-" +
			"7D-7C-3B-27-3A-22-2C-2E-2F-3C-3E-3F-04-03-00-00-00-8E-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-" +
			"6F-6E-73-2E-47-65-6E-65-72-69-63-2E-4C-69-73-74-60-31-5B-5B-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-" +
			"69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-52-65-65-6C-2C-20-41-73-73-65-6D-62-6C-79-2D-43-53-68-61-" +
			"72-70-2C-20-56-65-72-73-69-6F-6E-3D-30-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-" +
			"61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-6E-75-6C-6C-5D-5D-03-00-00-00-06-5F-69-74-65-" +
			"6D-73-05-5F-73-69-7A-65-08-5F-76-65-72-73-69-6F-6E-04-00-00-23-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-" +
			"67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-52-65-65-6C-5B-5D-02-00-00-00-08-08-09-05-00-00-00-06-" +
			"00-00-00-06-00-00-00-07-05-00-00-00-00-01-00-00-00-08-00-00-00-04-21-49-50-54-65-63-68-2E-53-6C-6F-74-" +
			"45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-52-65-65-6C-02-00-00-00-09-06-00-00-00-09-07-00-" +
			"00-00-09-08-00-00-00-09-09-00-00-00-09-0A-00-00-00-09-0B-00-00-00-0A-0A-05-06-00-00-00-1C-49-50-54-65-" +
			"63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-52-65-65-6C-02-00-00-00-13-3C-49-44-3E-6B-" +
			"5F-5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-18-3C-53-79-6D-62-6F-6C-73-3E-6B-5F-5F-42-61-63-6B-69-6E-67-" +
			"46-69-65-6C-64-01-03-90-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-" +
			"63-2E-4C-69-73-74-60-31-5B-5B-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-" +
			"41-70-69-2E-49-53-79-6D-62-6F-6C-2C-20-41-73-73-65-6D-62-6C-79-2D-43-53-68-61-72-70-2C-20-56-65-72-73-" +
			"69-6F-6E-3D-30-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-" +
			"69-63-4B-65-79-54-6F-6B-65-6E-3D-6E-75-6C-6C-5D-5D-02-00-00-00-0A-09-0C-00-00-00-01-07-00-00-00-06-00-" +
			"00-00-0A-09-0D-00-00-00-01-08-00-00-00-06-00-00-00-0A-09-0E-00-00-00-01-09-00-00-00-06-00-00-00-0A-09-" +
			"0F-00-00-00-01-0A-00-00-00-06-00-00-00-0A-09-10-00-00-00-01-0B-00-00-00-06-00-00-00-0A-09-11-00-00-00-" +
			"04-0C-00-00-00-90-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-" +
			"4C-69-73-74-60-31-5B-5B-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-" +
			"69-2E-49-53-79-6D-62-6F-6C-2C-20-41-73-73-65-6D-62-6C-79-2D-43-53-68-61-72-70-2C-20-56-65-72-73-69-6F-" +
			"6E-3D-30-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-" +
			"4B-65-79-54-6F-6B-65-6E-3D-6E-75-6C-6C-5D-5D-03-00-00-00-06-5F-69-74-65-6D-73-05-5F-73-69-7A-65-08-5F-" +
			"76-65-72-73-69-6F-6E-04-00-00-25-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-" +
			"2E-41-70-69-2E-49-53-79-6D-62-6F-6C-5B-5D-02-00-00-00-08-08-09-12-00-00-00-00-00-00-00-00-00-00-00-01-" +
			"0D-00-00-00-0C-00-00-00-09-12-00-00-00-00-00-00-00-00-00-00-00-01-0E-00-00-00-0C-00-00-00-09-12-00-00-" +
			"00-00-00-00-00-00-00-00-00-01-0F-00-00-00-0C-00-00-00-09-12-00-00-00-00-00-00-00-00-00-00-00-01-10-00-" +
			"00-00-0C-00-00-00-09-12-00-00-00-00-00-00-00-00-00-00-00-01-11-00-00-00-0C-00-00-00-09-12-00-00-00-00-" +
			"00-00-00-00-00-00-00-07-12-00-00-00-00-01-00-00-00-00-00-00-00-04-23-49-50-54-65-63-68-2E-53-6C-6F-74-" + 
			"45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-53-79-6D-62-6F-6C-02-00-00-00-0B";
	}
}
