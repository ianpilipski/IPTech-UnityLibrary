using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace IPTech.SlotEngine.Unity.Model.Editor.Api
{
	public interface IReelEditor : IInspectorGUI
	{
		ISymbolEditor symbolEditor { get; set; }
	}
}
