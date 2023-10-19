using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.PlayerInventory.Api
{
	public class InventoryIDIsReadOnly : Exception
	{
		public InventoryIDIsReadOnly() : base("Inventory ID is read only.") { }
    }

    public class ItemNameIsReadOnly : Exception {
        public ItemNameIsReadOnly() : base("ItemName is read only.") { }
    }
}
