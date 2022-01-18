using IPTech.PlayerInventory.Api;
using strange.extensions.mediation.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.PlayerInventory.Strange.Api
{
	public interface IInventoryCollectionView : IView
	{
		string CollectionName { get; }
		void UpdateFor(IInventoryCollection inventoryCollection);
	}
}
