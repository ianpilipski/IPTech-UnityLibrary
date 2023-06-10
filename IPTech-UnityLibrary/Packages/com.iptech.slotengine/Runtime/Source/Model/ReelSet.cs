using System.Collections.Generic;
using IPTech.SlotEngine.Model.Api;
using System;
using System.Collections;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class ReelSet : IReelSet
	{
		protected IList<IReel> Reels { get; set; }

		public string ID { get; set; }

		public int Count {
			get {
				return Reels.Count;
			}
		}

		public bool IsReadOnly {
			get {
				return false;
			}
		}

		public IReel this[int index] {
			get {
				return this.Reels[index];
			}
		}

		public ReelSet() {
			this.Reels = new List<IReel>();
		}

		public void Add(IReel reel) {
			this.Reels.Add(reel);
		}

		public void Clear() {
			this.Reels.Clear();
		}

		public bool Contains(IReel reel) {
			return this.Reels.Contains(reel);
		}

		public void CopyTo(IReel[] array, int arrayIndex) {
			this.Reels.CopyTo(array, arrayIndex);
		}

		public bool Remove(IReel item) {
			return this.Reels.Remove(item);
		}

		public IEnumerator<IReel> GetEnumerator() {
			return this.Reels.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.Reels.GetEnumerator();
		}
	}

}