using IPTech.Model.Api;
using IPTech.Types;
using strange.extensions.mediation.api;
using strange.extensions.signal.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.Model.Strange.Api
{
	public interface IModelView : IView
	{
		RName ModelID { get; }
		IModel Model { get; set; }
		void Refresh();

		Signal ModelIDChangedSignal { get; }
	}
}
