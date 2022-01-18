using IPTech.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTech.Types;
using System.Collections;

namespace IPTech.Model
{
	public class ModelEnumerableProperty : IModelProperties
	{
		private IModelProperty innerProperty;
		//private IList<IModelProperties> itemPropertyList;

		public ModelEnumerableProperty(IModelProperty modelProperty) {
			this.innerProperty = modelProperty;
			//this.itemPropertyList = new List<IModelProperties>();
			UpdateItemPropertyList();
		}

		private void UpdateItemPropertyList() {
			
		}

		private IEnumerable GetInnerEnumerable() {
			return (IEnumerable)this.innerProperty.CurrentValue;
		}

		public IModelProperty this[RName propertyName] {
			get {
				int index = Convert.ToInt32(propertyName.ToString());
				IEnumerable source = GetInnerEnumerable();
				int c = 0;
				IEnumerator e = source.GetEnumerator();
				while(e.MoveNext()) {
					if(c==index) {
						//return e.Current;
					}
					c++;
				}
				throw new IndexOutOfRangeException();
			}
		}

		public int Count {
			get {
				IEnumerable source = GetInnerEnumerable();
				int c = 0;
				IEnumerator e = source.GetEnumerator();
				while(e.MoveNext()) {
					c++;
				}
				return c;
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
			throw new NotImplementedException();
		}

		public bool Contains(RName propertyName) {
			throw new NotImplementedException();
		}

		public void CopyTo(IModelProperty[] array, int arrayIndex) {
			throw new NotImplementedException();
		}

		public IEnumerator<IModelProperty> GetEnumerator() {
			throw new NotImplementedException();
		}

		public bool Remove(IModelProperty item) {
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			throw new NotImplementedException();
		}
	}
}
