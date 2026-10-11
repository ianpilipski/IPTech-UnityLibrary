using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using IPTech.Utils;
using UnityEngine;
using IPTech.SlotEngine.Model;
using IPTech.SlotEngine.Model.Api;

namespace IPTech.SlotEngine.Tests.Model
{
	[TestFixture]
	class TestWildSymbolSet
	{
		private WildSymbolSet wildSymbolSet;
		
		public static List<IWildSymbol>[] wildSymbolListData = new List<IWildSymbol>[] {
			null,
			new List<IWildSymbol>(),
			new List<IWildSymbol>()
		};

		[SetUp]
		public void SetUp() {
			this.wildSymbolSet = new WildSymbolSet();
		}

		[Test]
		public void DefaultList_IsNotNull() {
			Assert.IsNotNull(this.wildSymbolSet.WildSymbols);
		}

		[Test, TestCaseSource("wildSymbolListData")]
		public void SettingAndGettingTheWildSymbolSet_ReturnsTheSameReference(List<IWildSymbol> wildSymbolList) {
			this.wildSymbolSet.WildSymbols = wildSymbolList;
			Assert.AreSame(wildSymbolList, this.wildSymbolSet.WildSymbols);
		}
	}
}
