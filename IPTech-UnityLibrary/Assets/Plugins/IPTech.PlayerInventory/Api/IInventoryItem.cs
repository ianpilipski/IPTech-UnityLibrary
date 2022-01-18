using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.PlayerInventory.Api
{
	public interface IInventoryItem
	{
        string ItemName { get; }
		string InventoryItemID { get; set; }
		long Amount { get; }
		long Adjust(long amount);
		void SetAmount(long newAmount);
		bool IsInventoryTypeOf(string inventoryItemID);
	}
}
