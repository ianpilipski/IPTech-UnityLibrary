using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using IPTech.Model.Api;
using IPTech.Types;
using System;

namespace IPTech.Model.Tests
{
	public class TestModel
	{
		[Serializable]
		private class SimpleModel
		{
			public int IntValue = 0;
			public float FloatValue = 0F;
			public double DoubleValue = 0.0;
			public long LongValue = 0;
			public uint UIntValue = 0;
			public ulong ULongValue = 0;
			public string StringValue = string.Empty;

			public SimpleModel() { }
		}

		private RName[] ExpectedPropertyNames = {
			RName.Name("IntValue"),
			RName.Name("FloatValue"),
			RName.Name("DoubleValue"),
			RName.Name("LongValue"),
			RName.Name("UIntValue"),
			RName.Name("ULongValue"),
			RName.Name("StringValue"),
		};

		private Model<SimpleModel> simpleModel;

		[SetUp]
		public void SetUp() {
			this.simpleModel = new Model<SimpleModel>();
		}

		[Test]
		public void ModelID_HasValuePassedByConstructor() {
			Assert.IsNull(this.simpleModel.ModelID);

			IModel model = new Model<SimpleModel>(RName.Name("constructorvalue"));
			Assert.AreEqual("constructorvalue", model.ModelID.ToString());
		}

		[Test]
		public void GenericSimpleModel_HasValuePassedByConstructor() {
			SimpleModel testModel = new SimpleModel();
			Model model = new Model(RName.Name("constructorvalue"), testModel);
			Assert.AreSame(testModel, model.TargetObject);
		}

		[Test]
		public void PropertiesForSimpleModel_HaveExcpectedCount() {
			IModelProperties modelProperties = simpleModel;

			Assert.AreEqual(7, modelProperties.Count);
		}

		[Test]
		public void PropertiesForSimpleModel_ContainsExpectedPropertyNames() {
			IModelProperties modelProperties = simpleModel;
			foreach(RName rName in this.ExpectedPropertyNames) {
				Assert.IsTrue(modelProperties.Contains(rName));
			}
		}

		[Test]
		public void OnModelChanged_IsFiredForPropertyChanges() {
			bool eventFired = false;
			this.simpleModel.ModelChanged+= (sender, eventArgs) => {
				eventFired = true;
			};

			this.simpleModel.Update();
			Assert.IsFalse(eventFired);

			this.simpleModel.TargetObject.IntValue++;
			this.simpleModel.Update();
			Assert.IsTrue(eventFired);
		}

		[Test]
		public void Data_IsOfTypeConstructed() {
			Assert.AreSame(typeof(SimpleModel), this.simpleModel.TargetObject.GetType());
		}

		[Test]
		public void SettingPropertyByName_ChangesUnderlyingProperty() {
			this.simpleModel[RName.Name("IntValue")].CurrentValue = 10;
			Assert.AreEqual(10, this.simpleModel.TargetObject.IntValue);
		}
		
	}
}