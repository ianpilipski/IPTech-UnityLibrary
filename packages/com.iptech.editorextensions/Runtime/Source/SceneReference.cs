using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.EditorExtensions {
	[Serializable]
	public class SceneReference {
		[HideInInspector]
		[SerializeField]
		string _scenePath;

		public string ScenePath {
			get {
				return _scenePath;
			}
			set {
				_scenePath = value;
				UpdateSceneAsset();
			}
		}

		public string SceneName {
			get {
				return string.IsNullOrEmpty(ScenePath) ? null : Path.GetFileNameWithoutExtension(ScenePath);
			}
		}

#if UNITY_EDITOR
		[SerializeField]
		UnityEditor.SceneAsset _scene;

		void UpdateSceneAsset() {
			if(!string.IsNullOrEmpty(_scenePath)) {
				_scene = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(_scenePath);
			} else {
				_scene = null;
			}
		}
#else
		void UpdateSceneAsset() {}
#endif
	}
}
