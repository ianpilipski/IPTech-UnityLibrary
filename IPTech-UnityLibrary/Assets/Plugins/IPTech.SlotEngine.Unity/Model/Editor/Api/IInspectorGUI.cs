using System;
using System.Collections.Generic;
using UnityEditor;

namespace IPTech.SlotEngine.Unity.Model.Editor.Api
{
	public interface IInspectorGUI
	{
		bool OnInspectorGUI(object targetObject);
	}
}
