using IPTech.SlotEngine.Model.Api;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using UnityEngine;

namespace IPTech.SlotEngine.Unity
{
	[CreateAssetMenu(menuName = "IPTech/SlotEngine/Create Slot Model", fileName = "SlotModel")]
	public class SlotModelScriptableObject : ScriptableObject 
	{
		[SerializeReference] private ISlotModel _slotModel;
		
		public ISlotModel slotModel {
			get => _slotModel;
			set => _slotModel = value;
		}

	}
}
