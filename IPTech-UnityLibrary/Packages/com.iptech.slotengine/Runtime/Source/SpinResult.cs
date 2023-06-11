using IPTech.SlotEngine.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTech.SlotEngine.Model.Api;

namespace IPTech.SlotEngine
{
	class SpinResult : ISpinResult
	{
		public IReelSet ReelSet { get; set; }
	}
}
