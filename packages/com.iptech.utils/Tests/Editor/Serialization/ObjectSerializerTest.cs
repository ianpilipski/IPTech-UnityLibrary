using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace IPTech.Utils.Tests
{
	[TestFixture]
	class ObjectSerializerTest
	{
		private static TestClass TESTCLASSOBJECT = new TestClass() {
			IntValue = 666,
			FloatValue = 6.66f,
			StringValue = "abc123",
			DoubleValue = 666.666,
			IntProperty = 666,
			FloatProperty = 6.66f,
			StringProperty = "abc123",
			DoubleProperty = 666.666
		};

		private ObjectSerializer<TestClass> serializer = new ObjectSerializer<TestClass>();
		private ObjectSerializer<TestClass> secondarySerializer = new ObjectSerializer<TestClass>();

		public byte[] b;
		public TestClass t;

		[SetUp]
		public void Setup() {
			this.serializer = new ObjectSerializer<TestClass>() { TargetObject = TESTCLASSOBJECT };
			this.serializer.SerializeBinaryObject();

			this.secondarySerializer = new ObjectSerializer<TestClass>() { TargetObject = TESTCLASSOBJECT };
			this.secondarySerializer.SerializeBinaryObject();
		}

		[Test]
		public void SettingObjectThenCallingBytesWithoutSerializing_ThrowsException() {
			Assert.IsNotNull(this.serializer.Bytes);
			this.serializer.TargetObject = TESTCLASSOBJECT;
			Assert.Throws<ObjectSerializer<TestClass>.NeedsSerializationException>(() => { b = this.serializer.Bytes; });
		}

		[Test]
		public void SettingBytesThenGettingTargetObjectWithoutDeserializing_ThrowsException() {
			Assert.IsNotNull(this.serializer.TargetObject);
			this.serializer.Bytes = new byte[10];
			Assert.Throws<ObjectSerializer<TestClass>.NeedsSerializationException>(() => { t = this.serializer.TargetObject; } );
		}

		[Test]
		public void SettingHexStringThenGettingTargetObjectWithoutDeserializing_ThrowsException() {
			Assert.IsNotNull(this.serializer.TargetObject);
			this.serializer.HexString = BINARY_SERIALIZED_HEX_STRING;
			Assert.Throws<ObjectSerializer<TestClass>.NeedsSerializationException>(() => { t = this.serializer.TargetObject; } );
		}

		[Test]
		public void SettingHexStringSetsBytesToCorrectValue() {
			this.serializer.Bytes = null;
			Assert.IsNull(this.serializer.Bytes);
			this.serializer.HexString = BINARY_SERIALIZED_HEX_STRING;
			Assert.AreEqual(BINARY_SERIALIZED_BYTES, this.serializer.Bytes);
		}

		[Test]
		public void SettingBytesSetsHexStringToCorrectValue() {
			this.serializer.Bytes = null;
			Assert.IsNull(this.serializer.HexString);
			this.serializer.Bytes = BINARY_SERIALIZED_BYTES;
			Assert.AreEqual(BINARY_SERIALIZED_HEX_STRING, this.serializer.HexString);
		}

		[Test, Ignore("TODO: Fix test")]
		public void DeserializingHexStringProducesOriginalObject() {
			this.serializer.HexString = BINARY_SERIALIZED_HEX_STRING;
			this.serializer.DeserializeBinaryObject();
			Assert.AreEqual(TESTCLASSOBJECT, this.serializer.TargetObject);
		}

		[Test]
		public void SerializingAndDeserializingProducesSameValues() {
			this.serializer.TargetObject = TESTCLASSOBJECT;
			this.serializer.SerializeBinaryObject();

			this.secondarySerializer.Bytes = this.serializer.Bytes;
			this.secondarySerializer.DeserializeBinaryObject();

			Assert.AreEqual(TESTCLASSOBJECT, this.secondarySerializer.TargetObject);
		}

		const string BINARY_SERIALIZED_HEX_STRING =
			"00-01-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-0C-02-00-00-00-16-41-73-73-65-6D-62-6C-" +
			"79-2D-43-53-68-61-72-70-2D-45-64-69-74-6F-72-05-01-00-00-00-31-49-50-54-65-63-68-2E-55-74-" +
			"69-6C-73-2E-54-65-73-74-73-2E-54-65-73-74-4F-62-6A-65-63-74-53-65-72-69-61-6C-69-7A-65-72-" +
			"2B-54-65-73-74-43-6C-61-73-73-08-00-00-00-08-49-6E-74-56-61-6C-75-65-0A-46-6C-6F-61-74-56-" +
			"61-6C-75-65-0B-53-74-72-69-6E-67-56-61-6C-75-65-0B-44-6F-75-62-6C-65-56-61-6C-75-65-1C-3C-" +
			"49-6E-74-50-72-6F-70-65-72-74-79-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-1E-3C-46-" +
			"6C-6F-61-74-50-72-6F-70-65-72-74-79-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-1F-3C-" +
			"53-74-72-69-6E-67-50-72-6F-70-65-72-74-79-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-" +
			"1F-3C-44-6F-75-62-6C-65-50-72-6F-70-65-72-74-79-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-" +
			"6C-64-00-00-01-00-00-00-01-00-08-0B-06-08-0B-06-02-00-00-00-9A-02-00-00-B8-1E-D5-40-06-03-" +
			"00-00-00-06-61-62-63-31-32-33-17-D9-CE-F7-53-D5-84-40-9A-02-00-00-B8-1E-D5-40-09-03-00-00-" + 
			"00-17-D9-CE-F7-53-D5-84-40-0B";

		static byte[] BINARY_SERIALIZED_BYTES = new byte[] {
			0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x02, 0x00, 0x00, 0x00, 0x16, 0x41, 0x73, 0x73, 0x65, 0x6D, 0x62, 0x6C,
			0x79, 0x2D, 0x43, 0x53, 0x68, 0x61, 0x72, 0x70, 0x2D, 0x45, 0x64, 0x69, 0x74, 0x6F, 0x72, 0x05, 0x01, 0x00, 0x00, 0x00, 0x31, 0x49, 0x50, 0x54, 0x65, 0x63, 0x68, 0x2E, 0x55, 0x74,
			0x69, 0x6C, 0x73, 0x2E, 0x54, 0x65, 0x73, 0x74, 0x73, 0x2E, 0x54, 0x65, 0x73, 0x74, 0x4F, 0x62, 0x6A, 0x65, 0x63, 0x74, 0x53, 0x65, 0x72, 0x69, 0x61, 0x6C, 0x69, 0x7A, 0x65, 0x72,
			0x2B, 0x54, 0x65, 0x73, 0x74, 0x43, 0x6C, 0x61, 0x73, 0x73, 0x08, 0x00, 0x00, 0x00, 0x08, 0x49, 0x6E, 0x74, 0x56, 0x61, 0x6C, 0x75, 0x65, 0x0A, 0x46, 0x6C, 0x6F, 0x61, 0x74, 0x56,
			0x61, 0x6C, 0x75, 0x65, 0x0B, 0x53, 0x74, 0x72, 0x69, 0x6E, 0x67, 0x56, 0x61, 0x6C, 0x75, 0x65, 0x0B, 0x44, 0x6F, 0x75, 0x62, 0x6C, 0x65, 0x56, 0x61, 0x6C, 0x75, 0x65, 0x1C, 0x3C,
			0x49, 0x6E, 0x74, 0x50, 0x72, 0x6F, 0x70, 0x65, 0x72, 0x74, 0x79, 0x3E, 0x6B, 0x5F, 0x5F, 0x42, 0x61, 0x63, 0x6B, 0x69, 0x6E, 0x67, 0x46, 0x69, 0x65, 0x6C, 0x64, 0x1E, 0x3C, 0x46,
			0x6C, 0x6F, 0x61, 0x74, 0x50, 0x72, 0x6F, 0x70, 0x65, 0x72, 0x74, 0x79, 0x3E, 0x6B, 0x5F, 0x5F, 0x42, 0x61, 0x63, 0x6B, 0x69, 0x6E, 0x67, 0x46, 0x69, 0x65, 0x6C, 0x64, 0x1F, 0x3C,
			0x53, 0x74, 0x72, 0x69, 0x6E, 0x67, 0x50, 0x72, 0x6F, 0x70, 0x65, 0x72, 0x74, 0x79, 0x3E, 0x6B, 0x5F, 0x5F, 0x42, 0x61, 0x63, 0x6B, 0x69, 0x6E, 0x67, 0x46, 0x69, 0x65, 0x6C, 0x64,
			0x1F, 0x3C, 0x44, 0x6F, 0x75, 0x62, 0x6C, 0x65, 0x50, 0x72, 0x6F, 0x70, 0x65, 0x72, 0x74, 0x79, 0x3E, 0x6B, 0x5F, 0x5F, 0x42, 0x61, 0x63, 0x6B, 0x69, 0x6E, 0x67, 0x46, 0x69, 0x65,
			0x6C, 0x64, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x08, 0x0B, 0x06, 0x08, 0x0B, 0x06, 0x02, 0x00, 0x00, 0x00, 0x9A, 0x02, 0x00, 0x00, 0xB8, 0x1E, 0xD5, 0x40, 0x06, 0x03,
			0x00, 0x00, 0x00, 0x06, 0x61, 0x62, 0x63, 0x31, 0x32, 0x33, 0x17, 0xD9, 0xCE, 0xF7, 0x53, 0xD5, 0x84, 0x40, 0x9A, 0x02, 0x00, 0x00, 0xB8, 0x1E, 0xD5, 0x40, 0x09, 0x03, 0x00, 0x00, 
			0x00, 0x17, 0xD9, 0xCE, 0xF7, 0x53, 0xD5, 0x84, 0x40, 0x0B
		};

		[Serializable]
		public class TestClass
		{
			public int IntValue;
			public float FloatValue;
			public string StringValue;
			public double DoubleValue;

			public int IntProperty { get; set; }
			public float FloatProperty { get; set; }
			public string StringProperty { get; set; }
			public double DoubleProperty { get; set; }

			public TestClass() {}

			public override bool Equals(object obj) {
				TestClass testClass = obj as TestClass;
				if (testClass != null) {
					return this.Equals(testClass);
				}
				return false;
			}

			public bool Equals(TestClass testClass) {
				if (object.ReferenceEquals(this, testClass)) {
					return true;
				}

				if (this == null || testClass == null) {
					return false;
				}

				if (this.DoubleProperty == testClass.DoubleProperty &&
					this.DoubleValue == testClass.DoubleValue &&
					this.FloatProperty == testClass.FloatProperty &&
					this.FloatValue == testClass.FloatValue &&
					this.IntProperty == testClass.IntProperty &&
					this.IntValue == testClass.IntValue &&
					this.StringProperty == testClass.StringProperty &&
					this.StringValue == testClass.StringValue) {
					return true;
				}

				return false;
			}

			public override int GetHashCode() {
				return base.GetHashCode();
			}

		}
	}
}
