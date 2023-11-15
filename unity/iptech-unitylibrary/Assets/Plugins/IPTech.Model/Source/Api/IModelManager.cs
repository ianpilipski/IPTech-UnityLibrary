using IPTech.Model.Api;
using IPTech.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.Model.Api
{
	public delegate void ModelManagerUpdatedDelegate(object sender, ModelManagerUpdatedEventArgs modelManagerUpdatedEventArgs);

	public enum ModelManagerUpdateType
	{
		ModelAdded,
		ModelRemoved,
	}

	public class ModelManagerUpdatedEventArgs : EventArgs
	{
		public IModel Model { get; private set; }
		public ModelManagerUpdateType UpdateType { get; private set; }

		public ModelManagerUpdatedEventArgs(ModelManagerUpdateType updateType, IModel model) {
			this.UpdateType = updateType;
			this.Model = model;
		}
	}

	public interface IModelManager : IList<IModel>, 
		ICollection<IModel>, IEnumerable<IModel>
	{
		event ModelManagerUpdatedDelegate ModelManagerUpdated;
		bool Contains(RName modelID);
		IModel this[RName modelID] { get; }
	}
}
