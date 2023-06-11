using IPTech.SlotEngine.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPTech.SlotEngine.Model.Api;
using IPTech.SlotEngine.Model;

namespace IPTech.SlotEngine
{
	public class SlotEngineBasic : ISlotEngine {

		private const uint DEFAULT_REEL_DISPLAY_HEIGHT = 3;

		private double totalWeightForReel;

		public ISlotModel SlotModel { get; set; }

		public ISlotEngineSpinEvaluationResult SpinEvaluationResult { get; protected set; }

		public Random RandomNumberGenerator { get; set; }

		public uint ReelDisplayHeight { get; set; }

		public bool IsSpinning { get; protected set; }

		public SlotEngineBasic() {
			this.SpinEvaluationResult = new SlotEngineSpinEvaluationResult();
			this.RandomNumberGenerator = new Random();
			this.ReelDisplayHeight = DEFAULT_REEL_DISPLAY_HEIGHT;
		}

		public void Spin(Action<ISlotEngineSpinEvaluationResult> spinEvaluationResultCallback) {
			IsSpinning = true;
			try {
				IReelSet resultReelSet = new ReelSet();
				foreach (IReel reel in this.SlotModel.ReelSet) {
					IReel resultReel = CalculateResultantReelFrom(reel);
					resultReelSet.Add(resultReel);
				}
				this.SpinEvaluationResult = new SlotEngineSpinEvaluationResult();
				this.SpinEvaluationResult.SpinResult = new SpinResult() {
					ReelSet = resultReelSet
				};
				CalculateWinningPaylines(resultReelSet);
				if(spinEvaluationResultCallback!=null) {
					spinEvaluationResultCallback(this.SpinEvaluationResult);
				}
			} finally {
				IsSpinning = false;
			}
		}

		private IReel CalculateResultantReelFrom(IReel reel) {
			IReel resultReel = new Reel();
			CalculateTotalWeightForReel(reel);
			for (int i = 0; i < this.ReelDisplayHeight; i++) {
				ISymbol symbol = GetRandomWeightedSymbol(reel);
				resultReel.Add(symbol);
			}
			return resultReel;
		}

		private void CalculateTotalWeightForReel(IReel reel) {
			this.totalWeightForReel = 0.0;
			foreach (ISymbol symbol in reel) {
				this.totalWeightForReel += symbol.Weight;
			}
		}

		private ISymbol GetRandomWeightedSymbol(IReel reel) {
			double targetValue = this.RandomNumberGenerator.NextDouble() * this.totalWeightForReel;
			double accumulatedWeight = 0.0;
			foreach(ISymbol symbol in reel) {
				accumulatedWeight += symbol.Weight;
				if(accumulatedWeight >= targetValue) {
					return symbol;
				}
			}
			return reel[reel.Count - 1];
		}

		private void CalculateWinningPaylines(IReelSet resultReelSet) {
			foreach (IPayline payline in this.SlotModel.PaylineSet.PayLines) {
				IList<ISymbol> symbolsForPayline = GetSymbolsForPayline(payline, resultReelSet);
				IPayoutTableEntry winningPayoutTableEntry = MatchSymbolsToBestPayoutTableEntry(symbolsForPayline, this.SlotModel.PayoutTable.PayoutTableEntries);
				if (winningPayoutTableEntry != null) {
					this.SpinEvaluationResult.WinningPaylines.Add(payline, winningPayoutTableEntry);
				}
			}
		}

		private IList<ISymbol> GetSymbolsForPayline(IPayline payline, IReelSet reelSet) {
			IList<ISymbol> symbols = new List<ISymbol>();
			foreach (IPaylineEntry paylineEntry in payline.PaylineEntries) {
				int column = paylineEntry.ReelColumn;
				int row = paylineEntry.ReelRow;
				ISymbol symbol = GetSymbolAt(column, row, reelSet);
				symbols.Add(symbol);
			}
			return symbols;
		}

		private ISymbol GetSymbolAt(int column, int row, IReelSet reelSet) {
			IReel reel = reelSet[column];
			int offsetRow = CalculateOffsetRow(row, reel);
			ISymbol symbol = reel[offsetRow];
			return symbol;
		}

		private int CalculateOffsetRow(int row, IReel reel) {
			bool isOddRowCount = (reel.Count % 2) != 0;
			if (isOddRowCount) {
				return (row + (reel.Count - 1) / 2);
			} else {
				if(row==0) {
					throw new Exception("Invalid 0 row for even reel");
				}
				return (row + reel.Count / 2) - 1;
			}
		}

		private IPayoutTableEntry MatchSymbolsToBestPayoutTableEntry(IList<ISymbol> symbols, ICollection<IPayoutTableEntry> payoutTableEntries) {
			IPayoutTableEntry bestMatchingPayoutTableEntry = null;
			foreach (IPayoutTableEntry payoutTableEntry in payoutTableEntries) {
				int matchCount = 0;
				for(int i=0;i<payoutTableEntry.SymbolIDList.Count;i++) {
					if (i < symbols.Count) {
						ISymbol symbol = symbols[i];
						//TODO: Add wild card check here
						if (symbol.ID != payoutTableEntry.SymbolIDList[i]) {
							break;
						}
						matchCount++;
					}
				}
				if(matchCount==payoutTableEntry.SymbolIDList.Count) {
					if (bestMatchingPayoutTableEntry == null) {
						bestMatchingPayoutTableEntry = payoutTableEntry;
					} else {
						if(bestMatchingPayoutTableEntry.PayoutMultiplier < payoutTableEntry.PayoutMultiplier) {
							bestMatchingPayoutTableEntry = payoutTableEntry;
						} else if(bestMatchingPayoutTableEntry.PayoutMultiplier == payoutTableEntry.PayoutMultiplier) {
							if(bestMatchingPayoutTableEntry.SymbolIDList.Count < payoutTableEntry.SymbolIDList.Count) {
								bestMatchingPayoutTableEntry = payoutTableEntry;
							}
						}
					}
				}
			}
			return bestMatchingPayoutTableEntry;
		}
	}
}
