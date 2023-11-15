using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NSubstitute;
using IPTech.Model.Api;
using IPTech.Types;

namespace IPTech.Model.Tests
{
	[TestFixture]
	class TestModelProperties
	{
		IModelProperty mockModelProperty1;
		IModelProperty mockModelProperty2;

		private RName mockName1 = RName.Name("mock1");
		private RName mockName2 = RName.Name("mock2");

		private ModelProperties modelProperties;

		private RName[] testNames;

		public static int[] testIndicies = { 0, 1 };

		private IModelProperty[] mockProperties;

		[SetUp]
		public void SetUp() {
			this.mockModelProperty1 = Substitute.For<IModelProperty>();
			this.mockModelProperty1.PropertyName.Returns(mockName1);

			this.mockModelProperty2 = Substitute.For<IModelProperty>();
			this.mockModelProperty2.PropertyName.Returns(mockName2);

			this.modelProperties = new ModelProperties();
			this.modelProperties.Add(this.mockModelProperty1);
			this.modelProperties.Add(this.mockModelProperty2);

			this.mockProperties = new IModelProperty[2] {
				this.mockModelProperty1, this.mockModelProperty2
			};

			this.testNames = new RName[2] {
				this.mockName1, this.mockName2
			};
		}

		[Test, TestCaseSource("testIndicies")]
		public void ContainsRNameForExpectedProperties_ReturnsTrue(int i) {
			Assert.IsTrue(this.modelProperties.Contains(this.testNames[i]));
		}

		[Test]
		public void ContainsRNameForNonExpectedProperty_ReturnsFalse() {
			Assert.IsFalse(this.modelProperties.Contains(RName.Name("should not exist")));
		}

		[Test, TestCaseSource("testIndicies")]
		public void IndexerByRNameForExpectedProperties_ReturnsTrue(int i) {
			Assert.AreSame(this.mockProperties[i], this.modelProperties[this.testNames[i]]);
		}

		[Test]
		public void Count_ReturnsExpectedValue() {
			Assert.AreEqual(2, this.modelProperties.Count);
		}

		[Test]
		public void Add_AddsNewProperty() {
			int count = this.modelProperties.Count;
			this.modelProperties.Add(this.mockProperties[0]);
			Assert.AreEqual(count + 1, this.modelProperties.Count);
		}

		[Test]
		public void Clear_ClearsProperties() {
			this.modelProperties.Clear();
			Assert.AreEqual(0, this.modelProperties.Count);
		}

		[Test, TestCaseSource("testIndicies")]
		public void ContainsByRefererencForExpectedProperties_ReturnsTrue(int i) {
			Assert.IsTrue(this.modelProperties.Contains(this.mockProperties[i]));
		}

		[Test]
		public void ContainsByReferenceForANonContainedProperty_ReturnsFalse() {
			Assert.IsFalse(this.modelProperties.Contains(Substitute.For<IModelProperty>()));
		}

		[Test]
		public void CopyTo_CopiesExpectedPropertiesToArray() {
			IModelProperty[] destArray = new IModelProperty[2];
			this.modelProperties.CopyTo(destArray, 0);
			for(int i=0;i<2;i++) {
				Assert.IsNotNull(destArray[i]);
				Assert.AreSame(this.mockProperties[i], destArray[i]);
			}
		}

		[Test]
		public void GetEnumerator_ReturnsExpectedEnumertor() {
			IEnumerator<IModelProperty> e = this.modelProperties.GetEnumerator();
			Assert.IsTrue(e.MoveNext());
			Assert.AreSame(this.mockProperties[0], e.Current);
			Assert.IsTrue(e.MoveNext());
			Assert.AreSame(this.mockProperties[1], e.Current);
			Assert.IsFalse(e.MoveNext());
		}

		[Test]
		public void Remove_RemovesItem() {
			this.modelProperties.Remove(this.mockModelProperty1);
			Assert.AreEqual(1, this.modelProperties.Count);
			this.modelProperties.Remove(this.mockModelProperty2);
			Assert.AreEqual(0, this.modelProperties.Count);
		}
	}
}
