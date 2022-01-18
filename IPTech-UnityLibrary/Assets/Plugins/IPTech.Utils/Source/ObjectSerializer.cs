using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace IPTech.Utils
{
	public class ObjectSerializer<T>
	{
		private byte[] _bytes;
		private T _targetObject;

		private enum ESerializationState
		{
			UnInitialized,
			NeedsSerialization,
			NeedsDeserialization,
			Serialized,
		}

		private ESerializationState _serializationState;
		private ESerializationState serializationState {
			get {
				return this._serializationState;
			}
			set {
				switch(value) {
					case ESerializationState.NeedsDeserialization:
						this._targetObject = default(T);
						break;
					case ESerializationState.NeedsSerialization:
						this._bytes = null;
						break;
					default:
						break;
				}
				this._serializationState = value;
			}
		}
		

		public T TargetObject {
			get {
				CheckIsInitialized();
				CheckNeedsDeserialization();
				return this._targetObject;
			}
			set {
				this.serializationState = ESerializationState.NeedsSerialization;
				this._targetObject = value;
			}
		}

		public byte[] Bytes {
			get {
				CheckIsInitialized();
				CheckNeedsSerialization();
				return this._bytes;
			}
			set {
				this.serializationState = ESerializationState.NeedsDeserialization;
				this._targetObject = default(T);
				this._bytes = value;
			}
		}

		public string HexString {
			get {
				if (this.Bytes != null) {
					return BitConverter.ToString(this.Bytes);
				}
				return null;
			}
			set {
				this.Bytes = StringToByteArray(value);
			}
		}

		public ObjectSerializer() {
			this.serializationState = ESerializationState.UnInitialized;
		}

		public void SerializeBinaryObject() {
			CheckIsInitialized();
			CheckNeedsDeserialization();
			using (MemoryStream ms = new MemoryStream()) {
				BinaryFormatter serializer = new BinaryFormatter();
				serializer.Serialize(ms, this._targetObject);
				this._bytes = ms.ToArray();
			}
			this.serializationState = ESerializationState.Serialized;
		}

		public void DeserializeBinaryObject() {
			CheckNeedsSerialization();
			using (MemoryStream ms = new MemoryStream(this.Bytes)) {
				BinaryFormatter serializer = new BinaryFormatter();
				this._targetObject = (T)serializer.Deserialize(ms);
			}
			this.serializationState = ESerializationState.Serialized;
		}

		private static byte[] StringToByteArray(String hex) {
			string[] hexBytes = hex.Split('-');
			byte[] bytes = new byte[hexBytes.Length];
			for (int i = 0; i < hexBytes.Length; i++) {
				bytes[i] = Convert.ToByte(hexBytes[i], 16);
			}
			return bytes;
		}

		private void CheckIsInitialized() {
			if(this.serializationState == ESerializationState.UnInitialized) {
				throw new NeedsInitializationException();
			}
		}

		private void CheckNeedsDeserialization() {
			if(this.serializationState== ESerializationState.NeedsDeserialization ) {
				throw new NeedsSerializationException();
			}
		}

		private void CheckNeedsSerialization() {
			if(this.serializationState==ESerializationState.NeedsSerialization) {
				throw new NeedsSerializationException();
			}
		}

		public class NeedsSerializationException : Exception
		{
			public NeedsSerializationException() : base("This object is in a dirty state and needs to be serialized or deserialized before reading this property.") { }
		}

		public class NeedsInitializationException : Exception
		{
			public NeedsInitializationException() : base("This object must first be initialized with a targetObject or byte/hex data.") { }
		}
	}
}
