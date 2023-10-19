using IPTech.Model.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using IPTech.Types;
using System.Collections;
using System.Collections.ObjectModel;

namespace IPTech.Model
{
	public class Model : Model<object>
	{
		private Model() { }
		public Model(object modelObject) : base(modelObject) { }
		public Model(RName modelID, object modelObject) : base(modelID, modelObject) { }
	}

	public class Model<T> : ModelProperties, IModel where T:class, new() 
	{
		private ChangedModelProperties changedModelProperties;

		public RName ModelID { get; private set; }
		
		public Type TargetType() {
			return this.TargetObject.GetType();
		}
		
		public event ModelChangedEventHandler ModelChanged;

		public T TargetObject { get; private set; }

		object IModel.TargetObject {
			get {
				return this.TargetObject;
			}
		}

		public Model() : this(null, null) { }
		public Model(RName modelID) : this(modelID, null) { }
		public Model(T modelObject) : this(null, modelObject) { }
		
		public Model(RName modelID, T modelObject) {
			try {
				this.ModelID = modelID;
				this.TargetObject = modelObject == null ? new T() : modelObject;
				InitializeModelProperties(typeof(T));
				this.changedModelProperties = new ChangedModelProperties(this);
			} catch(Exception e) {
				Type t = modelObject == null ? typeof(T) : modelObject.GetType();
				throw new Exception("Model of type " + t.Name + " failed to construct.", e);
			}
		}

		private void InitializeModelProperties(Type forType) {
			MemberInfo[] memberInfoArray = FormatterServices.GetSerializableMembers(forType);
			foreach (MemberInfo memberInfo in memberInfoArray) {
				// Skip this field if it is marked NonSerialized.
				if (Attribute.IsDefined(memberInfo, typeof(NonSerializedAttribute))) continue;

				this.Add(new ModelProperty((FieldInfo)memberInfo, this));
			}
		}

		public void Update() {
			foreach(ModelProperty property in this) {
				property.StartUpdate();
			}
			if(this.changedModelProperties.Count>0) {
				OnModelChanged();
			}
			foreach(ModelProperty property in this) {
				property.EndUpdate();
			}
		}

		private void OnModelChanged() {
			if(this.ModelChanged!=null) {
				this.ModelChanged(this, new ModelChangedEventArgs(this.changedModelProperties));
			}
		}
	}
}
