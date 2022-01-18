using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using IPTech.SlotEngine.Model.Api;
using System.Collections;

namespace IPTech.SlotEngine.Tests
{
	[TestFixture]
	class TestSlotEngineBasic {
		private SlotEngineBasic slotEngineBasic;

		public static ISlotModel[] slotModelData = { null, new MockSlotModel() };
		public static Random[] randomNumberGeneratorData = { null, new Random() };
		public static uint[] reelDisplayHeightData = { uint.MinValue, 666, uint.MaxValue };


		[SetUp]
		public void SetUp() {
			this.slotEngineBasic = new SlotEngineBasic();
			this.slotEngineBasic.SlotModel = new MockSlotModel();
		}

		[Test, TestCaseSource("slotModelData")]
		public void SettingAndGettingTheSlotModel_ReturnsTheSameReference(ISlotModel value) {
			this.slotEngineBasic.SlotModel = value;
			Assert.AreSame(value, this.slotEngineBasic.SlotModel);
		}

		[Test, TestCaseSource("randomNumberGeneratorData")]
		public void SettingAndgettingTheRandomNumberGenerator_ReturnsTheSameReference(Random value) {
			this.slotEngineBasic.RandomNumberGenerator = value;
			Assert.AreSame(value, this.slotEngineBasic.RandomNumberGenerator);
		}

		[Test, TestCaseSource("reelDisplayHeightData")]
		public void SettingAndGettingTheReelDisplayHeight_ReturnsTheSameValue(uint value) {
			this.slotEngineBasic.ReelDisplayHeight = value;
			Assert.AreEqual(value, this.slotEngineBasic.ReelDisplayHeight);
		}

		[Test]
		public void WhenNotSpinning_IsSpinningIsFalse() {
			Assert.IsFalse(this.slotEngineBasic.IsSpinning);
		}

		[Test]
		public void WhenSpinning_IsSpinningIsTrue() {
			bool spinning = false;
			this.slotEngineBasic.Spin(x => { spinning = this.slotEngineBasic.IsSpinning; });
			Assert.IsTrue(spinning);
		}

		[Test]
		public void DefaultSpinEvaluationResult_IsNotNull() {
			Assert.IsNotNull(this.slotEngineBasic.SpinEvaluationResult);
		}

		[Test]
		public void WhenSpinIsFinished_SpinResultCallbackIsCalled() {
			bool called = false;
			this.slotEngineBasic.Spin(x => called=true );
			Assert.IsTrue(called);
		}

		[Test]
		public void SettingWeightToZero_EliminatesThosePossiblities() {
			this.slotEngineBasic.SlotModel.ReelSet = new MockReelSet() {
				new MockReel("1") {
					new MockSymbol() { ID="A", Weight=1F },
					new MockSymbol() { ID="B", Weight=0F },
					new MockSymbol() { ID="C", Weight=0F },
				},
				new MockReel("2") {
					new MockSymbol() { ID="A", Weight=0F },
					new MockSymbol() { ID="B", Weight=1F },
					new MockSymbol() { ID="C", Weight=0F }
				},
				new MockReel("2") {
					new MockSymbol() { ID="A", Weight=0F },
					new MockSymbol() { ID="B", Weight=0F },
					new MockSymbol() { ID="C", Weight=1F }
				}
			};
			this.slotEngineBasic.Spin(null);

			Assert.AreEqual(0, this.slotEngineBasic.SpinEvaluationResult.WinningPaylines.Count);

			IReelSet reelSet = this.slotEngineBasic.SpinEvaluationResult.SpinResult.ReelSet;
			Assert.IsTrue(reelSet.ElementAt(0).All(symbol => symbol.ID == "A"));
			Assert.IsTrue(reelSet.ElementAt(1).All(symbol => symbol.ID == "B"));
			Assert.IsTrue(reelSet.ElementAt(2).All(symbol => symbol.ID == "C"));
		}

		public static IEnumerable allReelsOneSymbolTestCases {
			get {
				string[] symbols = { "A", "B", "C" };
				for (int i = 0; i < symbols.Length; i++) {
					ISlotModel mockSlotModel = new MockSlotModelAllOneSymbol(symbols[i]);
					IPayoutTableEntry expectedPayoutTableEntry = mockSlotModel.PayoutTable.PayoutTableEntries[i];
					yield return new TestCaseData(expectedPayoutTableEntry, mockSlotModel).SetName(symbols[i]);
				}
				yield break;
			}
		}

		[Test, TestCaseSource("allReelsOneSymbolTestCases")]
		public void WhenAllReelsAreOneSymbol_ProperPaylineIsAwarded(IPayoutTableEntry expectedPayoutTableEntry, ISlotModel slotModel) {
			this.slotEngineBasic.SlotModel = slotModel;
			this.slotEngineBasic.Spin(null);
			Assert.IsTrue(this.slotEngineBasic.SpinEvaluationResult.WinningPaylines.Values.All(payline => payline == expectedPayoutTableEntry));
		}

		[Test, Ignore("Not yet implemented")]
		public void WhenWildCardIsPresent_ProperAwardIsDetected() {

		}

		#region Mocks

		public class MockSlotModelAllOneSymbol : MockSlotModel
		{
			public MockSlotModelAllOneSymbol(string symbol) {
				this.ReelSet = new MockReelSet() {
					new MockReel("1") {
						new MockSymbol() { ID=symbol, Weight=1F }
					},
					new MockReel("2") {
						new MockSymbol() { ID=symbol, Weight=1F }
					},
					new MockReel("3") {
						new MockSymbol() { ID=symbol, Weight=1F }
					},
				};
			}
		}

		public class MockSlotModel : ISlotModel {
			public IPaylineSet PaylineSet { get; set; }
			public IPayoutTable PayoutTable { get; set; }
			public IReelSet ReelSet { get; set; }
			public IWildSymbolSet WildSymbolSet { get; set; }
			public MockSlotModel() {
				this.PaylineSet = new MockPaylineSet() {
					PayLines = new List<IPayline>() {
						new MockPayline(-1),
						new MockPayline(0),
						new MockPayline(1)
					}
				};
				this.PayoutTable = new MockPayoutTable("payoutTable1") {
					PayoutTableEntries = new List<IPayoutTableEntry>() {
						new MockPayoutTableEntry("A") { PayoutMultiplier = 1F },
						new MockPayoutTableEntry("B") { PayoutMultiplier = 5F },
						new MockPayoutTableEntry("C") { PayoutMultiplier = 20F },
					}
				};
				this.ReelSet = new MockReelSet() {
					new MockReel("reel1") {
						new MockSymbol() { ID = "A", Weight=1F },
						new MockSymbol() { ID = "B", Weight=1F },
						new MockSymbol() { ID = "C", Weight=1F },
					},
					new MockReel("reel2") {
						new MockSymbol() { ID = "A", Weight=1F },
						new MockSymbol() { ID = "B", Weight=1F },
						new MockSymbol() { ID = "C", Weight=1F },
					},
					new MockReel("reel3") {
						new MockSymbol() { ID = "A", Weight=1F },
						new MockSymbol() { ID = "B", Weight=1F },
						new MockSymbol() { ID = "C", Weight=1F },
					}
				};

				this.WildSymbolSet = new MockWildSymbolSet() {
					WildSymbols = new List<IWildSymbol>() {
						new MockWildSymbol() { ID="D" }
					}
				};
			}
		}

		public class MockPaylineSet : IPaylineSet
		{
			public IList<IPayline> PayLines { get; set; }
			public MockPaylineSet() {}
		}

		public class MockPayline : IPayline
		{
			public IList<IPaylineEntry> PaylineEntries { get; set; }
			public MockPayline(int row) {
				this.PaylineEntries = new List<IPaylineEntry>() {
					new MockPaylineEntry() { ReelColumn=0, ReelRow=row },
					new MockPaylineEntry() { ReelColumn=1, ReelRow=row },
					new MockPaylineEntry() { ReelColumn=2, ReelRow=row },
				};
			}
		}

		public class MockPaylineEntry : IPaylineEntry
		{
			public int ReelColumn { get; set; }
			public int ReelRow { get; set; }
		}

		public class MockPayoutTable : IPayoutTable
		{
			public string ID { get; set; }
			public IList<IPayoutTableEntry> PayoutTableEntries { get; set; }

			public MockPayoutTable(string ID) {
				this.ID = ID;
			}
		}

		public class MockPayoutTableEntry : IPayoutTableEntry
		{
			public string ID { get; set; }
			public float PayoutMultiplier { get; set; }
			public IList<string> SymbolIDList { get; set; }

			public MockPayoutTableEntry(string symbol) {
				this.ID = symbol;
				this.SymbolIDList = new List<string>() {
					symbol, symbol, symbol
				};
			}
		}

		public class MockReelSet : List<IReel>, IReelSet
		{
			public string ID { get; set; }
		}

		public class MockReel : List<ISymbol>, IReel
		{
			public string ID { get; set; }
			public MockReel(string ID) {
				this.ID = ID;
			}
		}

		public class MockSymbol : ISymbol
		{
			public string ID { get; set; }
			public float Weight { get; set; }
		}

		public class MockWildSymbolSet : IWildSymbolSet
		{
			public IList<IWildSymbol> WildSymbols { get; set; }
			public MockWildSymbolSet() {}
		}

		public class MockWildSymbol : IWildSymbol
		{
			public string ID { get; set; }
		}
		#endregion
	}
}
