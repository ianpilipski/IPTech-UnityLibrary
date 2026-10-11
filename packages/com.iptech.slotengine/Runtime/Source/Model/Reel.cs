using IPTech.SlotEngine.Model.Api;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using UnityEngine;
using System.Collections;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class Reel : IReel
	{
		[SerializeField] private string _id;
		[SerializeReference] private List<ISymbol> symbols;

		public Reel() {
			this.symbols = new List<ISymbol>();
		}

		public string ID 
		{
			get => _id;
			set => _id = value;
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
	}
}