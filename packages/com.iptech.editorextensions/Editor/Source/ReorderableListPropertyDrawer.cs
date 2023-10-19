using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomPropertyDrawer(typeof(ReorderableAttribute))]
public class ReorderableListPropertyDrawer : PropertyDrawer {
	static int _inList;

	ReorderableList _list;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		SerializedProperty parentArrayProperty = GetParentArray(property);
		if(parentArrayProperty!=null) {
			ReorderableList list = GetReorderableList(parentArrayProperty);
			list.DoList(position);
		} else if(_inList>0) {
			DrawElement(position, property);
		}
	}


	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		if(_inList>0) {
			return EditorGUI.GetPropertyHeight(property, true);
		} else {
			float height = 0F;
			SerializedProperty parentArrayProperty = GetParentArray(property);
			if(parentArrayProperty != null) {
				height = GetReorderableList(parentArrayProperty).GetHeight();
			}
			return height;
		}
	}

	SerializedProperty GetParentArray(SerializedProperty property) {
		if(property.propertyPath.EndsWith(".Array.data[0]", System.StringComparison.InvariantCulture)) {
			return property.serializedObject.FindProperty(property.propertyPath.Substring(0, property.propertyPath.Length - ".Array.data[0]".Length));
		}
		return null;
	}

	ReorderableList GetReorderableList(SerializedProperty property) {
		if(_list == null) {
			_list = new ReorderableList(property.serializedObject, property);
		}

		_list.drawHeaderCallback = (rect) => {
			_inList++;
			EditorGUI.LabelField(rect, property.displayName);
			_inList--;
		};

		_list.drawElementCallback = (rect, index, isActive, isFocused) => {
			_inList++;
			SerializedProperty p = property.GetArrayElementAtIndex(index);
			DrawElement(rect, p);
			_inList--;
		};

		_list.elementHeightCallback = (index) => {
			_inList++;
			SerializedProperty prop = property.GetArrayElementAtIndex(index);
			float height = EditorGUI.GetPropertyHeight(prop, prop.isExpanded) + 6; // add a little padding
			_inList--;
			return height;
		};

		return _list;
	}

	void DrawElement(Rect rect, SerializedProperty p) {
		GUIContent label = EditorGUI.BeginProperty(rect, new GUIContent(p.displayName), p);
		rect.yMin += 2; // a little padding
		if(p.hasVisibleChildren) {
			// This little indent trick were we move the indent back and push the rect forward is to allow foldouts to work with
			// the Reoderable list. It seems the collision detection for the foldout property is overlaying the re-orderable list
			// move control.
			EditorGUI.indentLevel--;
			rect.xMin += 15;
			EditorGUI.PropertyField(rect, p, label, true);
			EditorGUI.indentLevel++;
		} else {
			EditorGUI.PropertyField(rect, p, GUIContent.none, true);
		}
		EditorGUI.EndProperty();
	}
}