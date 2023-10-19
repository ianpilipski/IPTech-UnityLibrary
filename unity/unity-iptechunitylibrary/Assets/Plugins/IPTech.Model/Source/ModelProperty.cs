using IPTech.Model.Api;
using IPTech.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IPTech.Model
{
	public class ModelProperty : IModelProperty
	{
		private FieldInfo fieldInfo;
		private RName propertyName;
		private IModelProperties enumerableElements;

		public object ChangeValue {
			get; private set;
		}

		public object CurrentValue {
			get {
				return fieldInfo.GetValue(this.Owner.TargetObject);
			}
			set {
				fieldInfo.SetValue(this.Owner.TargetObject, value);
			}
		}

		public object PreviousValue {
			get; private set;
		}

		public RName PropertyName {
			get {
				if (this.propertyName == null) {
					this.propertyName = RName.Name(this.fieldInfo.Name);
				}
				return this.propertyName;
			}
		}

		public bool HasChanged {
			get; private set;
		}

		public bool IsArray {
			get {
				return (typeof(IEnumerable).IsAssignableFrom(fieldInfo.FieldType));
			}
		}

		public IModelProperties EnumerableElements {
			get {
				if(IsArray) {
					EnsureEnumerablePropertyWrapperIsCreated();
					return this.enumerableElements;
				}
				throw new ModelPropertyIsNotEnumerableException(this);
			}
		}

		private void EnsureEnumerablePropertyWrapperIsCreated() {
			if(this.enumerableElements==null) {
				this.enumerableElements = new ModelEnumerableProperty(this);
			}
		}

		public IModel Owner { get; private set; }

		public ModelProperty(FieldInfo fieldInfo, IModel model) {
			this.Owner = model;
			this.fieldInfo = fieldInfo;
			this.ChangeValue = this.CurrentValue;
			this.PreviousValue = this.ChangeValue;
		}

		public void StartUpdate() {
			this.ChangeValue = this.CurrentValue;
			this.HasChanged = (!this.PreviousValue.Equals(this.ChangeValue));
		}

		public void EndUpdate() {
			this.PreviousValue = this.ChangeValue;
			this.HasChanged = false;
		}
	}
}
