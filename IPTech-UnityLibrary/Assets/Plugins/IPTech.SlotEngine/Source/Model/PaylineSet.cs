using IPTech.SlotEngine.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Model
{
	[Serializable]
	public class PaylineSet : IPaylineSet
	{
		public IList<IPayline> PayLines { get; set; }

		public PaylineSet() {
			this.PayLines = new List<IPayline>();
		}
	}
}
