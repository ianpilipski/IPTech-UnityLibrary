using IPTech.SlotEngine.Model.Api;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using UnityEngine;
using System.Collections;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class Reel : IReel, ISerializable
	{
		const string SERIALIZATION_IDENTIFIER_FOR_ID = "<ID>k__BackingField";
		const string SERIALIZATION_IDENTIFIER_FOR_SYMBOLS = "<Symbols>k__BackingField";

		public string ID { get; set; }

		private IList<ISymbol> symbols { get; set; }

		public Reel() {
			this.symbols = new List<ISymbol>();
		}

		#region ICollection<ISymbol>
		public int Count {
			get {
				return this.symbols.Count;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public ISymbol this[int index] {
			get {
				return this.symbols[index];
			}
		}

		public void Add(ISymbol item) {
			this.symbols.Add(item);
		}

		public void Clear() {
			this.symbols.Clear();
		}

		public bool Contains(ISymbol item) {
			return this.symbols.Contains(item);
		}

		public void CopyTo(ISymbol[] array, int arrayIndex) {
			this.symbols.CopyTo(array, arrayIndex);
		}

		public bool Remove(ISymbol item) {
			return this.symbols.Remove(item);
		}

		public IEnumerator<ISymbol> GetEnumerator() {
			return this.symbols.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.symbols.GetEnumerator();
		}
		#endregion

		#region ISerializable
		public Reel(SerializationInfo info, StreamingContext context) {
			this.ID = info.GetString(SERIALIZATION_IDENTIFIER_FOR_ID);
			this.symbols = (IList<ISymbol>)info.GetValue(SERIALIZATION_IDENTIFIER_FOR_SYMBOLS, typeof(IList<ISymbol>));
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			info.AddValue(SERIALIZATION_IDENTIFIER_FOR_ID, this.ID, typeof(string));
			info.AddValue(SERIALIZATION_IDENTIFIER_FOR_SYMBOLS, this.symbols, this.symbols.GetType());
		}
		#endregion
	}
}