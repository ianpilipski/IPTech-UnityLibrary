using System;
using UnityEngine;

namespace IPTech.EditorExtensions {
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public class ScenePickerAttribute : PropertyAttribute { }
}