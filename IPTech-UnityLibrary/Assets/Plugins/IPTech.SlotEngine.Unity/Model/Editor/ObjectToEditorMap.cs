using strange.extensions.injector.api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.SlotEngine.Unity.Model.Editor
{
	public class ObjectToEditorMap<TObject, TObjectEditor>
	{
		private IDictionary<TObject, TObjectEditor> objectToEditorMap;

		[Inject]
		public IInjectionBinder injectionBinder { get; set; }

		
		public ObjectToEditorMap() {
			this.objectToEditorMap = new Dictionary<TObject, TObjectEditor>();
		}

		public TObjectEditor GetOrCreateEditorFor(TObject forObject) {
			TObjectEditor objectEditor = default(TObjectEditor);
			if(this.objectToEditorMap.TryGetValue(forObject,out objectEditor)) {
				return objectEditor;
			}
			objectEditor = this.injectionBinder.GetInstance<TObjectEditor>();
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
