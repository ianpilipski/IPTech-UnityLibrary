using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Model.Api
{
	public interface IPaylineEntry
	{
		int ReelColumn { get; set; }
		int ReelRow { get; set; }
	}
}
