using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IPTech.Unity.Core
{
	public class ScriptableObjectSelector : PropertyAttribute
	{
		private IList<ScriptableObject> foundScriptableObjects;

		public Type ScriptableObjectType { get; private set; }
		public IEnumerable<ScriptableObject> FoundScriptableObjects {
			get {
                GenerateFoundTypesList();
				return this.foundScriptableObjects;
			}
		}

		public bool ShowProperties {
			get;
			private set;
		}

		public ScriptableObjectSelector(Type scriptableObjectType, bool showProperties=false) {
			this.foundScriptableObjects = new List<ScriptableObject>();
			this.ShowProperties = showProperties;
			if(typeof(ScriptableObject).IsAssignableFrom(scriptableObjectType)) {
				this.ScriptableObjectType = scriptableObjectType;
				return;
			}
			throw new ArgumentException("Type must be derived from ScriptableObject");
		}

		private void GenerateFoundTypesList() {
#if UNITY_EDITOR
			string[] monoScriptsGUIDs = AssetDatabase.FindAssets("t:ScriptableObject");
			foreach (string monoScriptGUID in monoScriptsGUIDs) {
				string assetPath = AssetDatabase.GUIDToAssetPath(monoScriptGUID);
				ScriptableObject monoScript = (ScriptableObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject));
				if (this.ScriptableObjectType.IsAssignableFrom(monoScript.GetType())) {
					this.foundScriptableObjects.Add(monoScript);
				}
			}
#endif
		}
	}
}