using IPTech.SlotEngine.Model.Api;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine;

namespace IPTech.SlotEngine.Unity
{
	[CreateAssetMenu(menuName = "IPTech/SlotEngine/Create Slot Model", fileName = "SlotModel")]
	public class SlotModelScriptableObject : ScriptableObject, ISerializationCallbackReceiver {

		public ISlotModel slotModel { get; set; }

		[SerializeField, HideInInspector]
		private string serializedObjectAsString;
		
		private void Serialize() {
			if(slotModel==null) {
				return;
			}

			this.serializedObjectAsString = SerializationUtil.SerializeToBase64String(slotModel);
		}

		private void Deserialize() {
			if(string.IsNullOrEmpty(this.serializedObjectAsString)) {
				return;
			}

			this.slotModel = SerializationUtil.DeserializeFromBase64String(serializedObjectAsString) as ISlotModel;
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
