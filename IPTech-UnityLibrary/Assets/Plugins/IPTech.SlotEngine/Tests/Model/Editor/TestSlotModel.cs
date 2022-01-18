using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using IPTech.SlotEngine.Model;
using IPTech.Utils;
using IPTech.SlotEngine.Model.Api;
using System.Collections;

namespace IPTech.SlotEngine.Tests.Model
{
	[TestFixture]
	class TestSlotModel
	{
		private SlotModel slotModel;
		private ObjectSerializer<SlotModel> slotModelSerializer;

		public static IPaylineSet[] paylineSetData = new IPaylineSet[] { null, new MockPaylineSet() };
		public static IPayoutTable[] payoutTableData = new IPayoutTable[] { null, new MockPayoutTable() };
		public static IReelSet[] reelSetData = new IReelSet[] { null, new MockReelSet() };
		public static IWildSymbolSet[] wildSymbolSetData = new IWildSymbolSet[] { null, new MockWildSymbolSet() };

		[SetUp]
		public void SetUp() {
			this.slotModel = new SlotModel();
			this.slotModelSerializer = new ObjectSerializer<SlotModel>() { TargetObject = this.slotModel };
		}

		[Test, TestCaseSource("paylineSetData")]
		public void SettingAndGettingPaylineSet_ReturnsSameReference(IPaylineSet paylineSet) {
			this.slotModel.PaylineSet = paylineSet;
			Assert.AreSame(paylineSet, this.slotModel.PaylineSet);
		}

		[Test, TestCaseSource("payoutTableData") ]
		public void SettingAndGettingPayoutTable_ReturnsSameReference(IPayoutTable payoutTable) {
			this.slotModel.PayoutTable = payoutTable;
			Assert.AreSame(payoutTable, this.slotModel.PayoutTable);
		}

		[Test, TestCaseSource("reelSetData")]
		public void SettingAndGettingReelSet_ReturnsSameReference(IReelSet reelSet) {
			this.slotModel.ReelSet = reelSet;
			Assert.AreSame(reelSet, this.slotModel.ReelSet);
		}

		[Test, TestCaseSource("wildSymbolSetData")]
		public void SettingAndGettingWildSymbolSet_ReturnsSameReference(IWildSymbolSet wildSymbolSet) {
			this.slotModel.WildSymbolSet = wildSymbolSet;
			Assert.AreSame(wildSymbolSet, this.slotModel.WildSymbolSet);
		}

		[Test]
		public void SerializationWithOlderVersion_IsCompatible() {
			this.slotModelSerializer.SerializeBinaryObject();
			Assert.AreEqual(SERIALIZED_HEX_STRING, this.slotModelSerializer.HexString);
		}

		const string SERIALIZED_HEX_STRING = 
			"00-01-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-0C-02-00-00-00-0F-41-73-73-65-6D-62-6C-79-2D-" +
			"43-53-68-61-72-70-05-01-00-00-00-21-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-" +
			"64-65-6C-2E-53-6C-6F-74-4D-6F-64-65-6C-04-00-00-00-1B-3C-50-61-79-6C-69-6E-65-53-65-74-3E-6B-5F-" +
			"5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-1C-3C-50-61-79-6F-75-74-54-61-62-6C-65-3E-6B-5F-5F-42-61-" +
			"63-6B-69-6E-67-46-69-65-6C-64-18-3C-52-65-65-6C-53-65-74-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-" +
			"65-6C-64-1E-3C-57-69-6C-64-53-79-6D-62-6F-6C-53-65-74-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-" +
			"6C-64-04-04-04-04-27-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-" +
			"70-69-2E-49-50-61-79-6C-69-6E-65-53-65-74-02-00-00-00-28-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-" +
			"67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-50-61-79-6F-75-74-54-61-62-6C-65-02-00-00-00-24-" +
			"49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-52-65-65-" +
			"6C-53-65-74-02-00-00-00-2A-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-" + 
			"2E-41-70-69-2E-49-57-69-6C-64-53-79-6D-62-6F-6C-53-65-74-02-00-00-00-02-00-00-00-0A-0A-0A-0A-0B";
		
		#region Mocks
		public class MockPaylineSet : IPaylineSet
		{
			public IList<IPayline> PayLines {
				get {
					throw new NotImplementedException();
				}

				set {
					throw new NotImplementedException();
				}
			}
		}

		public class MockPayoutTable : IPayoutTable
		{
			public string ID {
				get {
					throw new NotImplementedException();
				}

				set {
					throw new NotImplementedException();
				}
			}

			public IList<IPayoutTableEntry> PayoutTableEntries {
				get {
					throw new NotImplementedException();
				}

				set {
					throw new NotImplementedException();
				}
			}
		}

		public class MockReelSet : IReelSet
		{
			public IReel this[int index] {
				get {
					throw new NotImplementedException();
				}
			}

			public int Count {
				get {
					throw new NotImplementedException();
				}
			}

			public string ID {
				get {
					throw new NotImplementedException();
				}

				set {
					throw new NotImplementedException();
				}
			}

			public bool IsReadOnly {
				get {
					throw new NotImplementedException();
				}
			}

			public void Add(IReel item) {
				throw new NotImplementedException();
			}

			public void Clear() {
				throw new NotImplementedException();
			}

			public bool Contains(IReel item) {
				throw new NotImplementedException();
			}

			public void CopyTo(IReel[] array, int arrayIndex) {
				throw new NotImplementedException();
			}

			public IEnumerator<IReel> GetEnumerator() {
				throw new NotImplementedException();
			}

			public bool Remove(IReel item) {
				throw new NotImplementedException();
			}

			IEnumerator IEnumerable.GetEnumerator() {
				throw new NotImplementedException();
			}
		}

		public class MockWildSymbolSet : IWildSymbolSet
		{
			public IList<IWildSymbol> WildSymbols {
				get {
					throw new NotImplementedException();
				}

				set {
					throw new NotImplementedException();
				}
			}
		}
		#endregion
	}
}
