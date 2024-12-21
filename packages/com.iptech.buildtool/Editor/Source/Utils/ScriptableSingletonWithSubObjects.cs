using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;

namespace IPTech.BuildTool
{
    public abstract class ScriptableSingletonWithSubObjects<T> : ScriptableSingleton<T> where T : ScriptableObject {
        protected virtual void GetSubObjectsToSave(List<UnityEngine.Object> objs) {  }

		void Awake() {
			var objs = new List<UnityEngine.Object>();
			GetSubObjectsToSave(objs);	
			foreach(var oo in objs) {
				if(ObjectIsInstance(oo)) {
					oo.hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
				}
			}
		}

        protected override void Save(bool saveAsText) {
			if((Object)instance == (Object)null) {
				BuildToolLogger.LogError("Cannot save ScriptableSingleton: no instance!");
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
					if(!ObjectIsInstance(objs[i])) {
						objs.RemoveAt(i);
                    } else {
						objs[i].hideFlags = HideFlags.HideAndDontSave & ~HideFlags.NotEditable;
					}
                }
				objs.Insert(0, instance);

				
				InternalEditorUtility.SaveToSerializedFileAndForget(objs.ToArray(), filePath, saveAsText);
			}
		}

		bool ObjectIsInstance(UnityEngine.Object oo) {
			return oo!=null && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(oo));
		}
    }
}
