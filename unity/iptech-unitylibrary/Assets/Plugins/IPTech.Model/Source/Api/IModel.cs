using IPTech.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace IPTech.Model.Api
{
	public interface IModelProperties : ICollection<IModelProperty>
	{
		IModelProperty this[RName propertyName] { get; }
		bool Contains(RName propertyName);
	}

	public delegate void ModelChangedEventHandler(object sender, ModelChangedEventArgs e);
	public delegate void ModelPropertyChangedEventHandler(object sender, ModelPropertyChangedEventArgs e);

	public class ModelChangedEventArgs : EventArgs {
		public IModelProperties ChangedProperties { get; private set; }

		public ModelChangedEventArgs(IModelProperties changedProperties) {
			this.ChangedProperties = changedProperties;
		}
	}

	public class ModelPropertyChangedEventArgs : EventArgs
	{
		public IModelProperty Property { get; private set; }

		public ModelPropertyChangedEventArgs(IModelProperty changedProperty) {
			this.Property = changedProperty;
		}
	}

	public interface IModel : IModelProperties {
		object TargetObject { get; }
		RName ModelID { get; }
		void Update();
		event ModelChangedEventHandler ModelChanged;
		Type TargetType();
	}

}
