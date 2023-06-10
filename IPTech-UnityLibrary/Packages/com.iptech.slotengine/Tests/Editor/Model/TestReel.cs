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

		const string SERIALIZED_BINARY_REEL_V1 =
			"00-01-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-0C-02-00-00-00-0F-41-73-73-65-6D-62-6C-79-2D-43-53-68-61-" +
			"72-70-05-01-00-00-00-1C-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-52-65-65-6C-" +
			"02-00-00-00-13-3C-49-44-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-18-3C-53-79-6D-62-6F-6C-73-3E-6B-5F-" +
			"5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-01-03-90-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-" +
			"47-65-6E-65-72-69-63-2E-4C-69-73-74-60-31-5B-5B-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-" +
			"64-65-6C-2E-41-70-69-2E-49-53-79-6D-62-6F-6C-2C-20-41-73-73-65-6D-62-6C-79-2D-43-53-68-61-72-70-2C-20-56-65-" +
			"72-73-69-6F-6E-3D-30-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-" +
			"69-63-4B-65-79-54-6F-6B-65-6E-3D-6E-75-6C-6C-5D-5D-02-00-00-00-06-03-00-00-00-66-4D-79-20-54-65-73-74-20-49-" +
			"44-3A-20-61-62-63-64-65-66-67-68-69-6A-6B-6C-6D-6E-6F-70-71-72-73-74-75-76-77-78-79-7A-41-42-43-44-45-46-47-" +
			"48-49-4A-4B-4C-4D-4E-4F-50-51-52-53-54-55-56-57-58-59-5A-31-32-33-34-35-36-37-38-39-30-21-40-23-24-25-5E-26-" +
			"2A-28-29-60-7E-5B-5D-5C-7B-7D-7C-3B-27-3A-22-2C-2E-2F-3C-3E-3F-09-04-00-00-00-04-04-00-00-00-90-01-53-79-73-" +
			"74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-4C-69-73-74-60-31-5B-5B-49-50-54-65-" +
			"63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-53-79-6D-62-6F-6C-2C-20-41-73-73-" +
			"65-6D-62-6C-79-2D-43-53-68-61-72-70-2C-20-56-65-72-73-69-6F-6E-3D-30-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-" +
			"72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-6E-75-6C-6C-5D-5D-03-00-00-" +
			"00-06-5F-69-74-65-6D-73-05-5F-73-69-7A-65-08-5F-76-65-72-73-69-6F-6E-04-00-00-25-49-50-54-65-63-68-2E-53-6C-" +
			"6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-53-79-6D-62-6F-6C-5B-5D-02-00-00-00-08-08-09-05-" +
			"00-00-00-03-00-00-00-03-00-00-00-07-05-00-00-00-00-01-00-00-00-04-00-00-00-04-23-49-50-54-65-63-68-2E-53-6C-" +
			"6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-53-79-6D-62-6F-6C-02-00-00-00-09-06-00-00-00-09-" +
			"07-00-00-00-09-08-00-00-00-0A-05-06-00-00-00-1E-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-" +
			"64-65-6C-2E-53-79-6D-62-6F-6C-02-00-00-00-13-3C-49-44-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-17-3C-" +
			"57-65-69-67-68-74-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-01-00-0B-02-00-00-00-0A-00-00-80-3F-01-07-" +
			"00-00-00-06-00-00-00-0A-00-00-80-3F-01-08-00-00-00-06-00-00-00-0A-00-00-80-3F-0B";

		public static string[] OLDER_SERIALIZED_BINARY_VERSIONS = new string[] {
			SERIALIZED_BINARY_REEL_V1,
		};

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
			ObjectSerializer<Reel> serializer = new ObjectSerializer<Reel>() { TargetObject=this.reel };
			serializer.SerializeBinaryObject();
			byte[] bytes = serializer.Bytes;
			ObjectSerializer<Reel> deserializer = new ObjectSerializer<Reel>() { Bytes = bytes };
			deserializer.DeserializeBinaryObject();
			Reel newReel = deserializer.TargetObject;
			Assert.AreEqual(this.reel.Count, newReel.Count);
			Assert.AreEqual(this.reel.ID, newReel.ID);
		}

		[Test, TestCaseSource("OLDER_SERIALIZED_BINARY_VERSIONS"), Ignore("TODO: Fix test")]
		public void BinaryDeserializationOfOlderSavedObjectsIsWorking(string olderVersion) {
			ObjectSerializer<Reel> serializer = new ObjectSerializer<Reel>() { HexString = olderVersion };
			serializer.DeserializeBinaryObject();
			Reel deserializedReel = serializer.TargetObject;
			Assert.AreEqual(this.reel.ID, deserializedReel.ID);
			Assert.AreEqual(this.reel.Count, deserializedReel.Count);
		}



	}
}
