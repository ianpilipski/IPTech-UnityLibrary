using IPTech.SlotEngine.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Api
{
	public interface ISpinResult
	{
		IReelSet ReelSet { get; set; }
	}
}
