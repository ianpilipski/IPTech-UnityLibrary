using UnityEditor;
using System.IO;

namespace IPTech.Unity.Utils.Editor
{
	public interface IEditorHelper
	{
		string GetCurrentlySelectedFolder();
	}

	public class EditorHelper : IEditorHelper
	{
		public EditorHelper() { }

		public string GetCurrentlySelectedFolder() {
			string path = "Assets";
			foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets)) {
				path = AssetDatabase.GetAssetPath(obj);
				if (File.Exists(path)) {
					path = Path.GetDirectoryName(path);
				}
				break;
			}
			return path;
		}
	}
}
