using IPTech.SlotEngine.Model.Api;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine;

namespace IPTech.SlotEngine.Unity
{
	public class SlotModelScriptableObject : ScriptableObject, ISerializationCallbackReceiver {

		public ISlotModel slotModel { get; set; }

		[SerializeField, HideInInspector]
		private string serializedObjectAsString;
		private BinaryFormatter serializer;

		public SlotModelScriptableObject() {
			this.serializer = new BinaryFormatter();
		}

		private void Serialize() {
			if(slotModel==null) {
				return;
			}

			using (MemoryStream stream = new MemoryStream()) {
				serializer.Serialize(stream, slotModel);
				stream.Flush();
				this.serializedObjectAsString = Convert.ToBase64String(stream.ToArray());
			}
		}

		private void Deserialize() {
			if(string.IsNullOrEmpty(this.serializedObjectAsString)) {
				return;
			}

			byte[] bytes = Convert.FromBase64String(serializedObjectAsString);
			using (var stream = new MemoryStream(bytes))
			this.slotModel = (ISlotModel)serializer.Deserialize(stream);
		}

		#region ISerializationCallbackReceiver

		public void OnBeforeSerialize() {
			Serialize();
		}

		public void OnAfterDeserialize() {
			Deserialize();
		}

		#endregion
	}
}
