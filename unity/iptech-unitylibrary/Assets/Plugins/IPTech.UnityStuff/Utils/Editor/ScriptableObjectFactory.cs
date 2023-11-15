using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;


namespace IPTech.Unity.Utils.Editor
{
	class ScriptableObjectFactory
	{
		private const string DEFAULTNAME = "NewAsset";
		private const string DEFAULTEXTENSION = ".asset";

		public string DefaultAssetName { get; set; }
		public string DefaultAssetExtension { get; set; }
		public UnityEngine.Object SourceObject { get; set; }
		public string WorkingDir { get; set; }

		public IEditorHelper editorHelper { get; set; }

		public ScriptableObjectFactory() {
			this.editorHelper = new EditorHelper();
			this.WorkingDir = SelectedPath;
			this.DefaultAssetName = DEFAULTNAME;
			this.DefaultAssetExtension = DEFAULTEXTENSION;
		}

		public string SelectedPath {
			get {
				return editorHelper.GetCurrentlySelectedFolder();
			}
		}

		public ScriptableObject Create<T>() {
			return Create(typeof(T));
		}

		public ScriptableObject Create(UnityEngine.Object sourcObject) {
			return Create(sourcObject.GetType());
		}

		public ScriptableObject Create(Type type) {
			return ScriptableObject.CreateInstance(type);
		}

		public string Save(ScriptableObject scriptableObject, string fileName = null, string outputDir = null) {
			outputDir = (outputDir == null) ? WorkingDir : outputDir;
			fileName = (fileName == null) ? DefaultAssetName : fileName;
			if(Path.GetExtension(fileName)!=DefaultAssetExtension) {
				fileName = string.Format("{0}{1}", fileName, DefaultAssetExtension);
			}
			string saveFileName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(outputDir, fileName));
			AssetDatabase.CreateAsset(scriptableObject, saveFileName);
			return saveFileName;
		}

		public T Load<T>(string fileName, string directory = null) where T : ScriptableObject {
			return (T)Load(fileName, directory);
		}

		public ScriptableObject Load(string fileName, string directory = null) {
			directory = (directory == null) ? WorkingDir : directory;
			string sourcePath = Path.Combine(directory, fileName);
			if (!File.Exists(sourcePath)) {
				sourcePath = Path.Combine(directory, string.Format("{0}{1}", fileName, DefaultAssetExtension));
				if(!File.Exists(sourcePath)) {
					sourcePath = fileName;
					if(!File.Exists(sourcePath)) {
						throw new CreateScriptableObjectException("Could not find asset in search paths : " + fileName);
					}
				}
			}
			return (ScriptableObject)AssetDatabase.LoadAssetAtPath(sourcePath, typeof(ScriptableObject));
		}

		public class CreateScriptableObjectException : Exception
		{
			public CreateScriptableObjectException(string message) : base(message) { }
		}
	}
}
