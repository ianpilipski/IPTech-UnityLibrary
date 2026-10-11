using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Model.Api
{
	public interface IPaylineSet
	{
		List<IPayline> PayLines { get; set; }
	}
}
