using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.Unity.Core
{
	public class InterfaceReference<T> : InterfaceReference where T : class
	{
		public override Type ReferencedObjectType {
			get {
				return typeof(T);
			}
		}

		public T InterfaceObject {
			get {
				return this.ReferencedObject as T;
			}
			set {
				UnityEngine.Object obj = value as UnityEngine.Object;
				if (obj != null) {
					this.ReferencedObject = obj;
					return;
				}

				throw new InvalidCastException();
			}
		}
	}
}
