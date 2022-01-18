using IPTech.PlayerInventory.Api;
using strange.extensions.mediation.api;
using strange.extensions.signal.impl;

namespace IPTech.PlayerInventory.Strange.Api
{
	interface IInventoryItemView : IView
	{
		Signal<string> InventoryItemIDChangedSignal { get; }
		void SetInventoryItemID(string newInventoryItemID);
		void SetAmount(IInventoryItem newInventoryItemAmount);
		void AddAmount(IInventoryItem additionalInventoryItemAmount);
	}
}
