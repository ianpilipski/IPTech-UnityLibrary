using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using IPTech.Utils;
using System.Runtime.Serialization;

namespace IPTech.Unity.Core
{
	public abstract class InterfaceReference : ScriptableObject
	{
		[SerializeField]
		private UnityEngine.Object referencedObject;

		public UnityEngine.Object ReferencedObject {
			get {
				return FindTypeOnObject(this.referencedObject);
			}

			set {
				if (value == null) {
					this.referencedObject = null;
					return;
				}

				UnityEngine.Object obj = FindBaseObject(value);
				if (obj != null) {
					if (FindTypeOnObject(obj) != null) {
						this.referencedObject = obj;
						return;
					}
				}

				throw new InvalidCastException(value.GetType().ToString());
			}
		}

		public abstract Type ReferencedObjectType { get; }

		public bool IsValid(UnityEngine.Object obj) {
			UnityEngine.Object baseObj = FindBaseObject(obj);
			return FindTypeOnObject(baseObj) != null;
		}

		private UnityEngine.Object FindBaseObject(UnityEngine.Object obj) {
			ScriptableObject so = obj as ScriptableObject;
			if(so!=null) {
				return so;
			}

			GameObject go = obj as GameObject;
			if(go!=null) {
				return go;
			}

			MonoBehaviour mb = obj as MonoBehaviour;
			if(mb!=null) {
				return mb.gameObject;
			}

			return null;
		}

		private UnityEngine.Object FindTypeOnObject(UnityEngine.Object obj) {
			if(obj==null) {
				return null;
			}

			Type t = this.ReferencedObjectType;
			if(t.IsAssignableFrom(obj.GetType())) {
				return obj;
			}

			GameObject go = obj as GameObject;
			if(go!=null) {
				MonoBehaviour[] monoBehaviours = go.GetComponents<MonoBehaviour>();
				foreach(MonoBehaviour mb in monoBehaviours) {
					if(t.IsAssignableFrom(mb.GetType())) {
						return mb;
					}
				}
			} else {
				ScriptableObject so = obj as ScriptableObject;
				if(so!=null) {
					if(t.IsAssignableFrom(so.GetType())) {
						return so;
					}
				}
			}

			return null;
		}
	}

	
}
