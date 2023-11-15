using IPTech.PlayerInventory.Api;
using strange.extensions.signal.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.PlayerInventory.Strange
{
	public class InventoryItemAmountChangedSignal : Signal<IInventoryItem>{}
	public class NeedInventoryAmountChangedSignal : Signal<string> { }
}
