using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IPTech.EditorExtensions {
	[CustomEditor(typeof(Note))]
	public class NoteEditor : Editor {

		void OnEnable() {
			Debug.Log("Enabled");
		}
		
		[DrawGizmo(GizmoType.Active | GizmoType.NonSelected)]
		static void OnDrawGizmos(Note noteObj, GizmoType gizmoType) {
			GUIContent content = new GUIContent(noteObj.Text);
			Rect rect = GetTextRect(noteObj, content);

			Handles.DrawSolidRectangleWithOutline(rect, Color.black, Color.yellow);
			Handles.color = Color.yellow;
			Handles.Label(noteObj.transform.position, noteObj.Text);
		}
		
		static Rect GetTextRect(Note noteObj, GUIContent content) {
			Vector2 size = GUI.skin.label.CalcSize(content);
			Rect rect = new Rect(0, 0, size.x, size.y);
			rect.position = noteObj.transform.position;
			return rect;
		}
	}
}
