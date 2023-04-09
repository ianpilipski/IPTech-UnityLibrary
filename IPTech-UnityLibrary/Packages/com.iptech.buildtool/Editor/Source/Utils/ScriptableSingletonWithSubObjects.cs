using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;
using System.Linq;

namespace IPTech.BuildTool {
    public abstract class ScriptableSingletonWithSubObjects<T> : ScriptableSingleton<T> where T : ScriptableObject {
        protected virtual void GetSubObjectsToSave(List<UnityEngine.Object> objs) {  }

        protected override void Save(bool saveAsText) {
			if((Object)instance == (Object)null) {
				Debug.LogError("Cannot save ScriptableSingleton: no instance!");
				return;
			}
			string filePath = GetFilePath();
			if(!string.IsNullOrEmpty(filePath)) {
				string directoryName = Path.GetDirectoryName(filePath);
				if(!Directory.Exists(directoryName)) {
					Directory.CreateDirectory(directoryName);
				}
				List<UnityEngine.Object> objs = new List<Object>();
				GetSubObjectsToSave(objs);
				for(int i=objs.Count-1;i>=0;i--) {
					if(objs[i] == null) {
						objs.RemoveAt(i);
                    } else if(!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(objs[i]))) {
						objs.RemoveAt(i);
                    }
                }
				objs.Insert(0, instance);

				
				InternalEditorUtility.SaveToSerializedFileAndForget(objs.ToArray(), filePath, saveAsText);
			}
		}
    }
}
