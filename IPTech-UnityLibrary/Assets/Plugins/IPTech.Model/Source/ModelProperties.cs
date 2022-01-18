using IPTech.Model.Api;
using IPTech.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace IPTech.Model
{
	public class ModelProperties : Collection<IModelProperty>, IModelProperties
	{
		public ModelProperties() {
		}

		public bool Contains(RName propertyName) {
			return this.Any(v => v.PropertyName.Equals(propertyName));
		}

		public IModelProperty this[RName propertyName] {
			get {
				return this.Where(v => v.PropertyName.Equals(propertyName)).First();
			}
		}
	}
}
