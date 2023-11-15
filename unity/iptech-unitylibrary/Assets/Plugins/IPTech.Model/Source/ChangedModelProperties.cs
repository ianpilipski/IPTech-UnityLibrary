using IPTech.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using IPTech.Types;

namespace IPTech.Model
{
	public class ChangedModelProperties : IModelProperties
	{
		private IModelProperties innerModelProperties;

		public ChangedModelProperties(IModelProperties modelProperties) {
			this.innerModelProperties = modelProperties;
		}

		
		public IModelProperty this[RName propertyName] {
			get {
				IModelProperty property = this.innerModelProperties[propertyName];
				if(property.HasChanged) {
					return property;
				}
				throw new KeyNotFoundException();
			}
		}

		public int Count {
			get {
				return this.innerModelProperties.Count(p => p.HasChanged);
			}
		}

		public bool IsReadOnly {
			get {
				return true;
			}
		}

		public void Add(IModelProperty item) {
			throw new NotImplementedException();
		}

		public void Clear() {
			throw new NotImplementedException();
		}

		public bool Contains(IModelProperty item) {
			return this.innerModelProperties.Where(p => p.HasChanged).Contains(item);
		}

		public bool Contains(RName propertyName) {
			return this.innerModelProperties.Any(p => p.HasChanged && p.PropertyName == propertyName);
		}

		public void CopyTo(IModelProperty[] array, int arrayIndex) {
			this.innerModelProperties.Where(p => p.HasChanged).ToList().CopyTo(array, arrayIndex);
		}

		public IEnumerator<IModelProperty> GetEnumerator() {
			return this.innerModelProperties.Where(p => p.HasChanged).GetEnumerator();
		}

		public bool Remove(IModelProperty item) {
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return this.innerModelProperties.Where(p => p.HasChanged).GetEnumerator();
		}
	}
}
