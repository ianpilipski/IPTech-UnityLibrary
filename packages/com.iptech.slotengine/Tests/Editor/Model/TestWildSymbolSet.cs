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
		private ObjectSerializer<WildSymbolSet> wildSymbolSetSerializer;

		public static IList<IWildSymbol>[] wildSymbolListData = new IList<IWildSymbol>[] {
			null,
			new List<IWildSymbol>(),
			new List<IWildSymbol>()
		};

		[SetUp]
		public void SetUp() {
			this.wildSymbolSet = new WildSymbolSet();
			this.wildSymbolSetSerializer = new ObjectSerializer<WildSymbolSet>() { TargetObject = this.wildSymbolSet };
		}

		[Test]
		public void DefaultList_IsNotNull() {
			Assert.IsNotNull(this.wildSymbolSet.WildSymbols);
		}

		[Test, TestCaseSource("wildSymbolListData")]
		public void SettingAndGettingTheWildSymbolSet_ReturnsTheSameReference(IList<IWildSymbol> wildSymbolList) {
			this.wildSymbolSet.WildSymbols = wildSymbolList;
			Assert.AreSame(wildSymbolList, this.wildSymbolSet.WildSymbols);
		}

		[Test, Ignore("TODO: Fix test")]
		public void SerializationWithOlderVersion_IsCompatible() {
			this.wildSymbolSetSerializer.SerializeBinaryObject();
			Assert.AreEqual(SERIALIZED_HEX_STRING, this.wildSymbolSetSerializer.HexString);
		}

		const string SERIALIZED_HEX_STRING = 
			"00-01-00-00-00-FF-FF-FF-FF-01-00-00-00-00-00-00-00-0C-02-00-00-00-0F-41-73-73-65-6D-62-6C-79-2D-43-53-68-61-72-70-05-" +
			"01-00-00-00-25-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-57-69-6C-64-53-79-6D-62-6F-6C-" +
			"53-65-74-01-00-00-00-1C-3C-57-69-6C-64-53-79-6D-62-6F-6C-73-3E-6B-5F-5F-42-61-63-6B-69-6E-67-46-69-65-6C-64-03-95-01-" +
			"53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-49-4C-69-73-74-60-31-5B-5B-49-50-54-" +
			"65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-57-69-6C-64-53-79-6D-62-6F-6C-2C-20-41-" +
			"73-73-65-6D-62-6C-79-2D-43-53-68-61-72-70-2C-20-56-65-72-73-69-6F-6E-3D-30-2E-30-2E-30-2E-30-2C-20-43-75-6C-74-75-72-" +
			"65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-6E-75-6C-6C-5D-5D-02-00-00-00-09-03-00-" +
			"00-00-04-03-00-00-00-94-01-53-79-73-74-65-6D-2E-43-6F-6C-6C-65-63-74-69-6F-6E-73-2E-47-65-6E-65-72-69-63-2E-4C-69-73-" +
			"74-60-31-5B-5B-49-50-54-65-63-68-2E-53-6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-57-69-6C-64-53-" +
			"79-6D-62-6F-6C-2C-20-41-73-73-65-6D-62-6C-79-2D-43-53-68-61-72-70-2C-20-56-65-72-73-69-6F-6E-3D-30-2E-30-2E-30-2E-30-" +
			"2C-20-43-75-6C-74-75-72-65-3D-6E-65-75-74-72-61-6C-2C-20-50-75-62-6C-69-63-4B-65-79-54-6F-6B-65-6E-3D-6E-75-6C-6C-5D-" +
			"5D-03-00-00-00-06-5F-69-74-65-6D-73-05-5F-73-69-7A-65-08-5F-76-65-72-73-69-6F-6E-04-00-00-29-49-50-54-65-63-68-2E-53-" +
			"6C-6F-74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-57-69-6C-64-53-79-6D-62-6F-6C-5B-5D-02-00-00-00-08-08-" +
			"09-04-00-00-00-00-00-00-00-00-00-00-00-07-04-00-00-00-00-01-00-00-00-00-00-00-00-04-27-49-50-54-65-63-68-2E-53-6C-6F-" + 
			"74-45-6E-67-69-6E-65-2E-4D-6F-64-65-6C-2E-41-70-69-2E-49-57-69-6C-64-53-79-6D-62-6F-6C-02-00-00-00-0B";
	}
}
