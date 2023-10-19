using System;
using System.Collections.Generic;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	public class ObjectToEditorMap<TObject, TObjectEditor>
	{
		Type objectEditorType;
		IDictionary<TObject, TObjectEditor> objectToEditorMap;

		public ObjectToEditorMap(Type objectEditorType) {
			this.objectEditorType = objectEditorType;
			this.objectToEditorMap = new Dictionary<TObject, TObjectEditor>();
		}

		public TObjectEditor GetOrCreateEditorFor(TObject forObject) {
            if (this.objectToEditorMap.TryGetValue(forObject, out TObjectEditor objectEditor))
            {
                return objectEditor;
            }
			objectEditor = (TObjectEditor)Activator.CreateInstance(objectEditorType);
            this.objectToEditorMap.Add(forObject, objectEditor);
			return objectEditor;
		}

		public void RemoveMappingsNotInCollection(ICollection<TObject> collection) {
			ICollection<TObject> mappedObjects = new List<TObject>(this.objectToEditorMap.Keys);
			foreach(TObject mappedObject in mappedObjects) {
				if(!collection.Contains(mappedObject)) {
					this.objectToEditorMap.Remove(mappedObject);
				}
			}
		}
	}
}
