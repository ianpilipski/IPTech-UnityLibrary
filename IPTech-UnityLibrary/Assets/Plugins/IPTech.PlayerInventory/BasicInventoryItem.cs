using IPTech.PlayerInventory.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.PlayerInventory
{
	[Serializable]
	public class BasicInventoryItem : IInventoryItem
	{
        public virtual string ItemName {
            get {
                return this.GetType().Name;
            }
            set {
                throw new ItemNameIsReadOnly();
            }
        }

		public virtual long Amount {
			get;
			protected set;
		}

		public virtual string InventoryItemID {
			get {
				return this.GetType().FullName;
			}
			set {
				throw new InventoryIDIsReadOnly();
			}
		}

		public virtual bool IsInventoryTypeOf(string inventoryItemID) {
			Type typeForInventoryItemID = Type.GetType(inventoryItemID);
			if (typeForInventoryItemID != null) {
				return typeForInventoryItemID.IsInstanceOfType(this);
			}
			return false;
		}

		public virtual long Adjust(long amount) {
			this.Amount = Math.Max(0, this.Amount + amount);
			return this.Amount;
		}

		public virtual void SetAmount(long newAmount) {
			this.Amount = Math.Max(0, newAmount);
		}
	}
}
