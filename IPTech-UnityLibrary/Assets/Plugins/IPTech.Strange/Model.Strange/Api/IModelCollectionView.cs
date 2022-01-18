using IPTech.Model.Api;
using strange.extensions.mediation.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.Model.Strange.Api
{
	public interface IModelCollectionView : IView
	{
		ICollection<IModel> ModelCollection { get; set; }
		void Refresh();
	}
}
