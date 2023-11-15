using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace IPTech.Unity.Core
{
	[CustomPropertyDrawer(typeof(ScriptableObjectSelector))]
	public class ScriptableObjectSelectorPropertyDrawer : PropertyDrawer
	{
		private ScriptableObjectSelector scriptableObjectSelector {
			get {
				return this.attribute as ScriptableObjectSelector;
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			ScriptableObjectSelector eso = this.scriptableObjectSelector;

			ScriptableObject[] foundScriptableObjectsArray = eso.FoundScriptableObjects.ToArray();
			string[] nameArray = new string[foundScriptableObjectsArray.Length];

			ScriptableObject scriptableObject = property.objectReferenceValue as ScriptableObject;

			int selectedIndex = 0;
			bool foundSelectedValue = false;
			for (int i=0;i<foundScriptableObjectsArray.Length;i++) {
				nameArray[i] = foundScriptableObjectsArray[i].name;
				if(!foundSelectedValue && foundScriptableObjectsArray[i] == scriptableObject) {
					selectedIndex = i;
					foundSelectedValue = true;
				}
			}

			bool hasValues = foundScriptableObjectsArray.Length > 0;

			position.Set(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.BeginProperty(position, label, property);
			int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, nameArray);
			if(hasValues && (newIndex!=selectedIndex || property.objectReferenceValue==null)) {
				property.objectReferenceValue = foundScriptableObjectsArray[newIndex];
			}

			if (property.objectReferenceValue != null && this.scriptableObjectSelector.ShowProperties) {
				SerializedObject so = new SerializedObject(property.objectReferenceValue);
				SerializedProperty sp = so.GetIterator();
				Rect pos = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, GUI.skin.box.padding.top * 2);
				sp.NextVisible(true);
				while(sp.NextVisible(true)) {
					pos.Set(pos.x, pos.y, pos.width, pos.height + EditorGUIUtility.singleLineHeight);
				}
				pos = EditorGUI.IndentedRect(pos);
				EditorGUI.DrawRect(pos, Color.gray);

				EditorGUI.indentLevel++;
				pos = new Rect(position.x + GUI.skin.box.padding.left,
					position.y + GUI.skin.box.padding.top,
					position.width - GUI.skin.box.padding.left * 2,
					EditorGUIUtility.singleLineHeight);
				sp.Reset();
				sp.NextVisible(true);
				while(sp.NextVisible(true)) {
					pos.Set(pos.x, pos.y + EditorGUIUtility.singleLineHeight, pos.width, EditorGUIUtility.singleLineHeight);
					EditorGUI.PropertyField(pos, sp);
				}
				EditorGUI.indentLevel--;
				so.ApplyModifiedProperties();
			}
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			float height = base.GetPropertyHeight(property, label);

			if (property.objectReferenceValue != null && this.scriptableObjectSelector.ShowProperties) {
				SerializedObject so = new SerializedObject(property.objectReferenceValue);
				so.Update();

				SerializedProperty sp = so.GetIterator();
				sp.NextVisible(true);
				while (sp.NextVisible(true)) {
					height += EditorGUIUtility.singleLineHeight;
				}
				height += GUI.skin.box.padding.top * 2;
			}

			return height;
		}
	}
}