using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using IPTech.Unity.Particles;
using System;

namespace IPTech.Unity.Core {
	[CustomPropertyDrawer(typeof(InterfaceReference), true)]
	public class InterfacePropertyDrawer : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			EditorGUI.BeginProperty(position, label, property);

			InterfaceReference irb = property.objectReferenceValue as InterfaceReference;
			if (irb == null) {
				Type t = this.fieldInfo.FieldType;
				ScriptableObject so = ScriptableObject.CreateInstance(t);
				irb = (InterfaceReference)so;
				property.objectReferenceValue = irb;
			}

			UnityEngine.Object obj = irb.ReferencedObject;

			bool valid = false;
			//if (Event.current.type == EventType.DragUpdated) {
				if(DragAndDrop.objectReferences.Length==1) {
					UnityEngine.Object dragObj = DragAndDrop.objectReferences[0] as UnityEngine.Object;
					if(dragObj!=null) {
						if(irb.IsValid(dragObj)) {
							valid = true;
						}
					}
				}
			//}

			Type tt = valid ? typeof(UnityEngine.Object) : irb.ReferencedObjectType;
			UnityEngine.Object newObj = EditorGUI.ObjectField(position, label, obj, tt, true);
			if (newObj != obj) {
				irb.ReferencedObject = newObj;	
			}

			EditorGUI.EndProperty();
			
		}
	}
}