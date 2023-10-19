using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using IPTech.SlotEngine.Model;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace IPTech.SlotEngine.Tests.Model
{
	[TestFixture]
	class TestSymbol
	{
		private const string TESTID = "My Test ID: abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()`~[]\\{}|;':\",./<>?";
		private const float TESTWEIGHT = 0.75f;

		private const string SERIALIZED_BINARY_SYMBOL_V1 = 
			"00-01-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-0C-02-00-00-00-0F-41-73-" +
			"73-65-6D-62-6C-79-2D-43-53-68-61-72-70-05-01-00-00-00-1E-49-50-54-65-63-68-" +
			"2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-53-79-6D-62-6F-6C-02-" +
			"00-00-00-13-3C-49-44-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-17-3C-" +
			"57-65-69-67-68-74-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-01-00-0B-" + 
			"02-00-00-00-06-03-00-00-00-66-4D-79-20-54-65-73-74-20-49-44-3A-20-61-62-63-" +
			"64-65-66-67-68-69-6A-6B-6C-6D-6E-6F-70-71-72-73-74-75-76-77-78-79-7A-41-42-" +
			"43-44-45-46-47-48-49-4A-4B-4C-4D-4E-4F-50-51-52-53-54-55-56-57-58-59-5A-31-" +
			"32-33-34-35-36-37-38-39-30-21-40-23-24-25-5E-26-2A-28-29-60-7E-5B-5D-5C-7B-" + 
			"7D-7C-3B-27-3A-22-2C-2E-2F-3C-3E-3F-00-00-40-3F-0B";

		public static string[] OLDER_SERIALIZED_BINARY_VERSIONS = new string[] {
			SERIALIZED_BINARY_SYMBOL_V1,
		};

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
		public void BinarySerializationAndDeserializationIsProducingTheSameObject() {
			Symbol symbol = new Symbol() { ID = TESTID, Weight = TESTWEIGHT };
			byte[] serializedBytes = BinarySerializeObject(symbol);
			string hexString = BitConverter.ToString(serializedBytes);
			Console.WriteLine(hexString);
			Symbol newSymbol = BinaryDeserializeSymbol(serializedBytes);
			Assert.AreEqual(symbol.ID, newSymbol.ID);
			Assert.AreEqual(symbol.Weight, newSymbol.Weight);
		}

		[Test, TestCaseSource("OLDER_SERIALIZED_BINARY_VERSIONS"), Ignore("TODO: Fix test")]
		public void BinaryDeserializationOfOlderSavedObjectsIsWorking(string olderVersion) {
			byte[] bytes = StringToByteArray(olderVersion);
			Symbol symbol = BinaryDeserializeSymbol(bytes);
			Assert.AreEqual(TESTID, symbol.ID);
			Assert.AreEqual(TESTWEIGHT, symbol.Weight);
		}

		private byte[] BinarySerializeObject(Symbol symbol) {
			BinaryFormatter serializer = new BinaryFormatter();
			using(MemoryStream ms = new MemoryStream()) {
				serializer.Serialize(ms, symbol);
				return ms.ToArray();
			}
		}

		private Symbol BinaryDeserializeSymbol(byte[] bytes) {
			BinaryFormatter serializer = new BinaryFormatter();
			using (MemoryStream ms = new MemoryStream(bytes)) {
				return (Symbol)serializer.Deserialize(ms);
			}
		}

		public static byte[] StringToByteArray(String hex) {
			string[] hexBytes = hex.Split('-');
			byte[] bytes = new byte[hexBytes.Length];
			for(int i=0;i<hexBytes.Length;i++) {
				bytes[i] = Convert.ToByte(hexBytes[i], 16);
			}
			return bytes;
		}
	}
}
