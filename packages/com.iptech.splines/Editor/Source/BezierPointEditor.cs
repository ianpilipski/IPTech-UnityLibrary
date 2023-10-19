using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
namespace IPTech.Splines
{
	[CustomEditor(typeof(BezierPoint))]
	[CanEditMultipleObjects]
	public class BezierPointEditor : Editor
	{

		BezierPoint point;

		private delegate void HandleFunction(BezierPoint p);
		private HandleFunction[] handlers = new HandleFunction[] { HandleConnected, HandleBroken, HandleAbsent };

		void OnEnable() {
			point = (BezierPoint)target;
		}

		public override void OnInspectorGUI() {
			DrawInspector(serializedObject);
		}

		public static void DrawInspector(SerializedObject forObject) {
			forObject.Update();

			SerializedProperty handleTypeProp = forObject.FindProperty("handleStyle");
			SerializedProperty handle1Prop = forObject.FindProperty("_handle1");
			SerializedProperty handle2Prop = forObject.FindProperty("_handle2");
			SerializedProperty hasCollisionProp = forObject.FindProperty("hasCollision");

			BezierPoint.HandleStyle handleStyle = (BezierPoint.HandleStyle)EditorGUILayout.EnumPopup("Handle Type", (BezierPoint.HandleStyle)handleTypeProp.intValue);

			if (handleStyle != (BezierPoint.HandleStyle)handleTypeProp.intValue) {
				handleTypeProp.intValue = (int)handleStyle;

				if (handleStyle == BezierPoint.HandleStyle.Connected) {
					if (handle1Prop.vector3Value != Vector3.zero) handle2Prop.vector3Value = -handle1Prop.vector3Value;
					else if (handle2Prop.vector3Value != Vector3.zero) handle1Prop.vector3Value = -handle2Prop.vector3Value;
					else {
						handle1Prop.vector3Value = new Vector3(0.1f, 0, 0);
						handle2Prop.vector3Value = new Vector3(-0.1f, 0, 0);
					}
				} else if (handleStyle == BezierPoint.HandleStyle.Broken) {
					if (handle1Prop.vector3Value == Vector3.zero && handle2Prop.vector3Value == Vector3.zero) {
						handle1Prop.vector3Value = new Vector3(0.1f, 0, 0);
						handle2Prop.vector3Value = new Vector3(-0.1f, 0, 0);
					}
				} else if (handleStyle == BezierPoint.HandleStyle.None) {
					handle1Prop.vector3Value = Vector3.zero;
					handle2Prop.vector3Value = Vector3.zero;
				}
			}

			if (handleStyle != BezierPoint.HandleStyle.None) {
				Vector3 newHandle1 = EditorGUILayout.Vector3Field("Handle 1", handle1Prop.vector3Value);
				Vector3 newHandle2 = EditorGUILayout.Vector3Field("Handle 2", handle2Prop.vector3Value);

				if (handleStyle == BezierPoint.HandleStyle.Connected) {
					if (newHandle1 != handle1Prop.vector3Value) {
						handle1Prop.vector3Value = newHandle1;
						handle2Prop.vector3Value = -newHandle1;
					} else if (newHandle2 != handle2Prop.vector3Value) {
						handle1Prop.vector3Value = -newHandle2;
						handle2Prop.vector3Value = newHandle2;
					}
				} else {
					handle1Prop.vector3Value = newHandle1;
					handle2Prop.vector3Value = newHandle2;
				}
			}

			hasCollisionProp.boolValue = EditorGUILayout.Toggle("Has Collision", hasCollisionProp.boolValue);

			if (GUI.changed) {
				forObject.ApplyModifiedProperties();
				foreach (BezierPoint p in forObject.targetObjects) {
                    EditorApplication.delayCall += () => {
                        p.EditorConditionalUpdateCollision();
                    };
                }
				EditorUtility.SetDirty(forObject.targetObject);
			}
		}

		void OnSceneGUI() {

			Handles.color = Color.green;
			Vector3 newPosition = Handles.FreeMoveHandle(point.position, point.transform.rotation, HandleUtility.GetHandleSize(point.position) * 0.2f, Vector3.zero, Handles.CubeHandleCap);
			if (point.position != newPosition) {
				point.position = newPosition;
				point.EditorConditionalUpdateCollision();
			}

			handlers[(int)point.handleStyle](point);

			Handles.color = Color.yellow;
			Handles.DrawLine(point.position, point.globalHandle1);
			Handles.DrawLine(point.position, point.globalHandle2);

			BezierCurveEditor.DrawOtherPoints(point.curve, point);
		}

		private static void HandleConnected(BezierPoint p) {
			Handles.color = Color.cyan;

			Vector3 newGlobal1 = Handles.FreeMoveHandle(p.globalHandle1, p.transform.rotation, HandleUtility.GetHandleSize(p.globalHandle1) * 0.15f, Vector3.zero, Handles.SphereHandleCap);

			if (newGlobal1 != p.globalHandle1) {
				Undo.RecordObject(p, "Move Handle");
				p.globalHandle1 = newGlobal1;
				p.globalHandle2 = -(newGlobal1 - p.position) + p.position;
				p.EditorConditionalUpdateCollision();
			}

			Vector3 newGlobal2 = Handles.FreeMoveHandle(p.globalHandle2, p.transform.rotation, HandleUtility.GetHandleSize(p.globalHandle2) * 0.15f, Vector3.zero, Handles.SphereHandleCap);

			if (newGlobal2 != p.globalHandle2) {
				Undo.RecordObject(p, "Move Handle");
				p.globalHandle1 = -(newGlobal2 - p.position) + p.position;
				p.globalHandle2 = newGlobal2;
				p.EditorConditionalUpdateCollision();
			}
		}

		private static void HandleBroken(BezierPoint p) {
			Handles.color = Color.cyan;

			Vector3 newGlobal1 = Handles.FreeMoveHandle(p.globalHandle1, Quaternion.identity, HandleUtility.GetHandleSize(p.globalHandle1) * 0.15f, Vector3.zero, Handles.SphereHandleCap);
			Vector3 newGlobal2 = Handles.FreeMoveHandle(p.globalHandle2, Quaternion.identity, HandleUtility.GetHandleSize(p.globalHandle2) * 0.15f, Vector3.zero, Handles.SphereHandleCap);

			if (newGlobal1 != p.globalHandle1) {
				Undo.RecordObject(p, "Move Handle");
				p.globalHandle1 = newGlobal1;
				p.EditorConditionalUpdateCollision();
			}

			if (newGlobal2 != p.globalHandle2) {
				Undo.RecordObject(p, "Move Handle");
				p.globalHandle2 = newGlobal2;
				p.EditorConditionalUpdateCollision();
			}
		}

		private static void HandleAbsent(BezierPoint p) {
			p.handle1 = Vector3.zero;
			p.handle2 = Vector3.zero;
		}
	}
}