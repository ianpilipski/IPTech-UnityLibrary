using IPTech.SlotEngine.Model.Api;
using System;
using System.Runtime.Serialization;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class Symbol : ISymbol, ISerializable
	{
		private const string SERIALIZATION_IDENTIFIER_FOR_ID = "<ID>k__BackingField";
		private const string SERIALIZATION_IDENTIFIER_FOR_WEIGHT = "<Weight>k__BackingField";

		private const string WEIGHT_OUT_OF_RANGE = "The value for weight must not be a negative value.";
		private const float DEFAULT_WEIGHT = 1.0f;

		private float _weight;

		public string ID { get; set; }

		public float Weight {
			get { return this._weight; }
			set {
				if(value<0.0f) {
					throw new ArgumentOutOfRangeException(WEIGHT_OUT_OF_RANGE);
				}
				this._weight = value;
			}
		}

		public Symbol() {
			this.Weight = DEFAULT_WEIGHT;
		}

		#region ISerializable
		public Symbol(SerializationInfo info, StreamingContext context) {
			this.ID = (string)info.GetValue(SERIALIZATION_IDENTIFIER_FOR_ID, typeof(string));
			this.Weight = (float)info.GetValue(SERIALIZATION_IDENTIFIER_FOR_WEIGHT, typeof(float));
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue(SERIALIZATION_IDENTIFIER_FOR_ID, this.ID, typeof(string));
			info.AddValue(SERIALIZATION_IDENTIFIER_FOR_WEIGHT, this.Weight, typeof(float));
		}
		#endregion
	}
}