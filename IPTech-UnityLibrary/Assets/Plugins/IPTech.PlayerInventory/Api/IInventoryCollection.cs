using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.PlayerInventory.Api
{
	public interface IInventoryCollection : ICollection<IInventoryItem>
	{
		string ID { get; set; }
		IInventoryItem this[int index] { get; }
		bool TryFindInventoryForID(string inventoryID, out IInventoryItem inventoryFound);
		bool Contains(string InventoryItemID);
	}
}
