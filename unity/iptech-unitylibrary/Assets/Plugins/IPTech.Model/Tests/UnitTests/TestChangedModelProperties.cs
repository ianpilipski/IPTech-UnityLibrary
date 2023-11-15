using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NSubstitute;
using IPTech.Model.Api;
using IPTech.Types;
using System.Collections;

namespace IPTech.Model.Tests
{
	[TestFixture]
	class TestChangedModelProperties
	{
		private IModelProperties modelProperties;

		private ChangedModelProperties changeModelProperties;
		private IModelProperty mockModelProperty;
		private IModelProperty mockModelPropertyChanged;

		private RName CHANGED_PROPERTY_NAME = RName.Name("Property Has Changed");
		private RName UNCHANGED_PROPERTY_NAME = RName.Name("Property Has Not Changed");

		[SetUp]
		public void SetUp() {

			this.mockModelProperty = Substitute.For<IModelProperty>();
			this.mockModelProperty.HasChanged.Returns(false);
			this.mockModelProperty.PropertyName.Returns(UNCHANGED_PROPERTY_NAME);

			this.mockModelPropertyChanged = Substitute.For<IModelProperty>();
			this.mockModelPropertyChanged.HasChanged.Returns(true);
			this.mockModelPropertyChanged.PropertyName.Returns(CHANGED_PROPERTY_NAME);

			this.modelProperties = new ModelProperties();
			this.modelProperties.Add(this.mockModelProperty);
			this.modelProperties.Add(this.mockModelPropertyChanged);

			this.changeModelProperties = new ChangedModelProperties(this.modelProperties);
		}

		[Test]
		public void Count_ReturnsExpectedValue() {
			Assert.AreEqual(1, this.changeModelProperties.Count);
		}

		[Test]
		public void Indexer_ReturnsChangedPropertyByName() {
			IModelProperty changedProperty = this.changeModelProperties[CHANGED_PROPERTY_NAME];
			Assert.AreSame(this.mockModelPropertyChanged, changedProperty);
		}

		[Test]
		public void IsReadOnly_ReturnsTrue() {
			Assert.IsTrue(this.changeModelProperties.IsReadOnly);
		}

		[Test]
		public void Add_ThrowsNotImplementedException() {
            Assert.Throws<NotImplementedException>(() => {
                this.changeModelProperties.Add(this.mockModelPropertyChanged);
            });
		}

		[Test]
		public void Clear_ThrowsNotImplementedException() {
            Assert.Throws<NotImplementedException>(() => {
                this.changeModelProperties.Clear();
            });
		}

		[Test]
		public void Remove_ThrowsNotImplementedException() {
            Assert.Throws<NotImplementedException>(() => {
                this.changeModelProperties.Remove(this.mockModelPropertyChanged);
            });
		}

		[Test]
		public void ContainsForChangedPropertyReference_ReturnsTrue() {
			Assert.IsTrue(this.changeModelProperties.Contains(this.mockModelPropertyChanged));
		}

		[Test]
		public void ContainsForUnChangedPropertyReference_ReturnsFalse() {
			Assert.IsFalse(this.changeModelProperties.Contains(this.mockModelProperty));
		}

		[Test]
		public void ContainsForChangedPropertyName_ReturnsTrue() {
			Assert.IsTrue(this.changeModelProperties.Contains(CHANGED_PROPERTY_NAME));
		}

		[Test]
		public void ContainsForUnChangedPropertyName_ReturnsFalse() {
			Assert.IsFalse(this.changeModelProperties.Contains(UNCHANGED_PROPERTY_NAME));
		}

		[Test]
		public void CopyTo_CopiesOnlyTheChangedProperties() {
			IModelProperty[] changedArray = new IModelProperty[10];
			this.changeModelProperties.CopyTo(changedArray, 9);
			for(int i=0;i<8;i++) {
				Assert.IsNull(changedArray[i]);
			}
			Assert.AreSame(this.mockModelPropertyChanged, changedArray[9]);
		}

		[Test]
		public void GetEnumerator_ReturnsAnEnumeratorForChangedPropertiesOnly() {
			IEnumerator enumerator = ((IEnumerable)this.changeModelProperties).GetEnumerator();
			Assert.IsTrue(enumerator.MoveNext());
			Assert.AreSame(this.mockModelPropertyChanged, enumerator.Current);
			Assert.IsFalse(enumerator.MoveNext());
		}

		[Test]
		public void GetEnumeratorGenericType_ReturnsAnEnumeratorForChangedPropertiesOnly() {
			IEnumerator<IModelProperty> e = this.changeModelProperties.GetEnumerator();
			Assert.IsTrue(e.MoveNext());
			Assert.AreSame(this.mockModelPropertyChanged, e.Current);
			Assert.IsFalse(e.MoveNext());
		}
	}
}
