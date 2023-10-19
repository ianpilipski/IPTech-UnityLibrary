using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using IPTech.Model.Api;
using IPTech.Types;
using System;
using System.Collections.Generic;

namespace IPTech.Model.Tests
{
	[TestFixture]
	class TestModelManager
	{
		private IModelManager modelManager;
		private IModel mockModel1;
		private IModel mockModel2;

		[Serializable]
		protected class MockModel
		{
			public int i = 0;
		}

		[SetUp]
		public void SetUp() {
			this.modelManager = new ModelManager();
			MockModel m1 = new MockModel() { i = 1 };
			MockModel m2 = new MockModel() { i = 2 };
			this.mockModel1 = new Model<MockModel>(RName.Name("model1"), m1);
			this.mockModel2 = new Model<MockModel>(RName.Name("model2"), m2);
		}

		[Test]
		public void AddingModel_AddsModelToTheManager() {
			Assert.AreEqual(0, this.modelManager.Count);
			this.modelManager.Add(this.mockModel1);
			Assert.AreEqual(1, this.modelManager.Count);
		}

		[Test]
		public void RemovingModel_RemovesModelFromTheManager() {
			this.modelManager.Add(this.mockModel1);
			this.modelManager.Remove(this.mockModel1);
			Assert.AreEqual(0, this.modelManager.Count);
		}

		[Test]
		public void RemovingModelAtIndex_RemovesModelFromTheManager() {
			this.modelManager.Add(this.mockModel1);
			this.modelManager.RemoveAt(0);
			Assert.AreEqual(0, this.modelManager.Count);
		}

		[Test]
		public void Indexer_RetrievesModelAtIndex() {
			this.modelManager.Add(this.mockModel1);
			this.modelManager.Add(this.mockModel2);
			Assert.AreSame(this.mockModel1, this.modelManager[0]);
			Assert.AreSame(this.mockModel2, this.modelManager[1]);
		}

		[Test]
		public void Indexer_RetrievesModelByModelID() {
			this.modelManager.Add(this.mockModel1);
			this.modelManager.Add(this.mockModel2);
			Assert.AreSame(this.mockModel1, this.modelManager[this.mockModel1.ModelID]);
			Assert.AreSame(this.mockModel2, this.modelManager[this.mockModel2.ModelID]);
		}

		[Test]
		public void Indexer_ThrowsOutOfRangeException() {
            Assert.Throws<ArgumentOutOfRangeException>(() => {
                this.mockModel1 = this.modelManager[1];
            });
		}

		[Test]
		public void AddingModel_FiresModelManagerChangedEventWithModelAdded() {
			bool fired = false;
			this.modelManager.ModelManagerUpdated += (s, a) => { fired = a.UpdateType== ModelManagerUpdateType.ModelAdded; };
			this.modelManager.Add(this.mockModel1);
			Assert.IsTrue(fired);
		}

		[Test]
		public void AddingSameModelMoreThanOnce_FiresModelManagerChangedEventOnly1Time() {
			int firedCount = 0;
			this.modelManager.ModelManagerUpdated += (s, a) => { firedCount++; };
			this.modelManager.Add(this.mockModel1);
			this.modelManager.Add(this.mockModel1);
			Assert.AreEqual(1, firedCount);
		}

		[Test]
		public void RemovingModel_FiresModelManagerChangedEventWithModelRemoved() {
			bool fired = false;
			this.modelManager.Add(this.mockModel1);
			this.modelManager.ModelManagerUpdated += (s, a) => { fired = a.UpdateType == ModelManagerUpdateType.ModelRemoved; };
			this.modelManager.Remove(this.mockModel1);
			Assert.IsTrue(fired);
		}

		[Test]
		public void RemovingSameModelMoreThanOnce_FiresModelManagerChangedEventOnly1Time() {
			int firedCount = 0;
			this.modelManager.Add(this.mockModel1);
			this.modelManager.Add(this.mockModel1);
			this.modelManager.ModelManagerUpdated += (s, a) => { firedCount++; };
			this.modelManager.Remove(this.mockModel1);
			this.modelManager.Remove(this.mockModel1);
			Assert.AreEqual(1, firedCount);
		}

		[Test]
		public void ClearingModels_FiresModelManagerChangedEventWithModelRemoved() {
			int firedCount = 0;
			this.modelManager.Add(this.mockModel1);
			this.modelManager.Add(this.mockModel2);
			this.modelManager.ModelManagerUpdated += (s, a) => { if (a.UpdateType == ModelManagerUpdateType.ModelRemoved) { firedCount++; } };
			this.modelManager.Clear();
			Assert.AreEqual(2, firedCount);
		}

		[Test]
		public void IndexerWithInvalidModelID_ThrowsKeyNotFoundException() {
            Assert.Throws<KeyNotFoundException>(() => {
                this.mockModel1 = this.modelManager[RName.Name("somebadid")];
            });
		}

		[Test]
		public void SetByIndex_SetsModel() {
			this.modelManager.Add(null);
			this.modelManager[0] = this.mockModel1;
			Assert.AreEqual(this.mockModel1, this.modelManager[0]);
		}

		[Test]
		public void SetByIndex_FiresModelManagerChangedEvent() {
			int firedCount = 0;
			this.modelManager.ModelManagerUpdated += (s, a) => { firedCount++; };
			this.modelManager.Add(null);
			this.modelManager[0] = this.mockModel1;
			Assert.AreEqual(1, firedCount);
		}

		[Test]
		public void SetByIndexToSameModel_FiresModelManagerChangedEvent1Time() {
			int firedCount = 0;
			this.modelManager.ModelManagerUpdated += (s, a) => { firedCount++; };
			this.modelManager.Add(null);
			this.modelManager[0] = this.mockModel1;
			this.modelManager[0] = this.mockModel1;
			Assert.AreEqual(1, firedCount);
		}

		[Test]
		public void SetByIndexReplacingModel_FiresModelManagerChangedEvent3Times() {
			int firedCount = 0;
			this.modelManager.ModelManagerUpdated += (s, a) => { firedCount++; };
			this.modelManager.Add(null);
			this.modelManager[0] = this.mockModel1;
			this.modelManager[0] = this.mockModel2;
			Assert.AreEqual(3, firedCount);
		}
	}
}
