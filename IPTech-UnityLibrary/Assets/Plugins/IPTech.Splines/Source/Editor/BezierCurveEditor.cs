using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace IPTech.Splines
{
	[CustomEditor(typeof(BezierCurve))]
	public class BezierCurveEditor : Editor
	{
		BezierCurve curve;
		SerializedProperty resolutionProp;
		SerializedProperty closeProp;
		SerializedProperty pointsProp;
		SerializedProperty colorProp;
		//SerializedProperty hasCollisionProp;
		SerializedProperty extrusionWidthProp;

		/// <summary>
		///  	- the number of mid-points calculated for each pair of bezier points
		///  	- used for drawing the curve in the editor
		///  	- used for calculating the "length" variable
		/// </summary>
		private static int resolution = 30;

		private static bool showPoints = true;

		void OnEnable() {
			curve = (BezierCurve)target;

			resolutionProp = serializedObject.FindProperty("resolution");
			closeProp = serializedObject.FindProperty("_close");
			pointsProp = serializedObject.FindProperty("points");
			colorProp = serializedObject.FindProperty("drawColor");
			extrusionWidthProp = serializedObject.FindProperty("extrusionWidth");
		}

		private IList<BezierPoint> GetSelectedPoints() {
			List<BezierPoint> selectePoints = new List<BezierPoint>();
			for (int i = 0; i < curve.pointCount; i++) {
				BezierPoint point = curve[i];
				if (point.EditorSelected) {
					selectePoints.Add(point);
				}
			}
			return selectePoints;
		}


		private IList<BezierPoint> InsertPointAfterAllSelectedPoints() {
			List<BezierPoint> insertedPoints = new List<BezierPoint>();
			int count = curve.pointCount;
			for (int i = 0; i < count; i++) {
				BezierPoint point = curve[i];
				if (point.EditorSelected) {
					Undo.RecordObject(curve, "Insert Point");
					BezierPoint newPoint = curve.InsertPoint(curve[i], BezierCurve.EInsertType.AfterCurrentPoint);
					Undo.RegisterCreatedObjectUndo(newPoint.gameObject, "Insert Point");
					insertedPoints.Add(newPoint);
					count++;
				}
			}
			return insertedPoints;
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.PropertyField(resolutionProp);
			EditorGUILayout.PropertyField(closeProp);
			EditorGUILayout.PropertyField(colorProp);
			EditorGUILayout.PropertyField(extrusionWidthProp);

			showPoints = EditorGUILayout.Foldout(showPoints, "Points");

			if (showPoints) {
				int pointCount = pointsProp.arraySize;

				for (int i = 0; i < pointCount; i++) {
					if (DrawPointInspector(curve[i], i)) {
						pointCount--;
					}
				}

				if (GUILayout.Button("Add Point")) {
					Undo.RecordObject(curve, "Add Point");
					BezierPoint newPoint = curve.AddPoint();
					Undo.RegisterCreatedObjectUndo(newPoint.gameObject, "Add Point");
				}
			}

			if (serializedObject.ApplyModifiedProperties()) {
				EditorUtility.SetDirty(target);
			}
		}

		[DrawGizmo(GizmoType.Active | GizmoType.NotInSelectionHierarchy | GizmoType.Selected | GizmoType.Pickable)]
		private static void OnDrawGizmos(BezierCurve theCurve, GizmoType gizmoType) {
			Gizmos.color = theCurve.drawColor;
			BezierPoint[] points = theCurve.GetAnchorPoints();
			if (points.Length > 1) {
				for (int i = 0; i < points.Length - 1; i++) {
					DrawCurve(points[i], points[i + 1], resolution);
				}

				if (theCurve.close) {
					DrawCurve(points[points.Length - 1], points[0], resolution);
				}
			}
		}

		/// <summary>
		/// 	- Draws the curve in the Editor
		/// </summary>
		/// <param name='p1'>
		/// 	- The bezier point at the beginning of the curve
		/// </param>
		/// <param name='p2'>
		/// 	- The bezier point at the end of the curve
		/// </param>
		/// <param name='resolution'>
		/// 	- The number of segments along the curve to draw
		/// </param>
		public static void DrawCurve(BezierPoint p1, BezierPoint p2, int resolution) {
			int limit = resolution + 1;
			float _res = resolution;
			Vector3 lastPoint = p1.position;
			Vector3 currentPoint = Vector3.zero;

			for (int i = 1; i < limit; i++) {
				currentPoint = BezierCurve.CalculatePointOnCurve(p1, p2, i / _res);
				Gizmos.DrawLine(lastPoint, currentPoint);
				lastPoint = currentPoint;
			}
		}

		void OnSceneGUI() {
			for (int i = 0; i < curve.pointCount; i++) {
				DrawPointSceneGUI(curve[i]);
			}

			Event e = Event.current;
			if (e.isKey && e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.I) {
				e.Use();
				InsertPointAfterAllSelectedPoints();
			}
		}

		bool DrawPointInspector(BezierPoint point, int index) {
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button("X", GUILayout.Width(20))) {
				pointsProp.MoveArrayElement(curve.GetPointIndex(point), curve.pointCount - 1);
				pointsProp.arraySize--;
				serializedObject.ApplyModifiedProperties();
				Undo.DestroyObjectImmediate(point.gameObject);
				return true;
			}

			EditorGUILayout.ObjectField(point.gameObject, typeof(GameObject), true);

			if (index != 0 && GUILayout.Button(@"/\", GUILayout.Width(25))) {
				UnityEngine.Object other = pointsProp.GetArrayElementAtIndex(index - 1).objectReferenceValue;
				pointsProp.GetArrayElementAtIndex(index - 1).objectReferenceValue = point;
				pointsProp.GetArrayElementAtIndex(index).objectReferenceValue = other;
			}

			if (index != pointsProp.arraySize - 1 && GUILayout.Button(@"\/", GUILayout.Width(25))) {
				UnityEngine.Object other = pointsProp.GetArrayElementAtIndex(index + 1).objectReferenceValue;
				pointsProp.GetArrayElementAtIndex(index + 1).objectReferenceValue = point;
				pointsProp.GetArrayElementAtIndex(index).objectReferenceValue = other;
			}

			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel++;
			EditorGUI.indentLevel++;

			SerializedObject serObj = new SerializedObject(point);
			BezierPointEditor.DrawInspector(serObj);

			EditorGUI.indentLevel--;
			EditorGUI.indentLevel--;

			return false;
		}

		private static BezierPoint currentlyDrawnPoint = null;
		private static void ChangePointSelection(BezierPoint point, bool selected) {
			if (point.EditorSelected != selected) {
				Undo.RecordObject(point, "Point Selected");
				point.EditorSelected = selected;
			}
		}

		private static void SelectedPointCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType) {
			Event e = Event.current;
			if (controlID == GUIUtility.hotControl) {
				if (e.control) {
					ChangePointSelection(currentlyDrawnPoint, !currentlyDrawnPoint.EditorSelected);
				} else {
					if (!currentlyDrawnPoint.EditorSelected) {
						for (int i = 0; i < currentlyDrawnPoint.curve.pointCount; i++) {
							if (currentlyDrawnPoint.curve[i] != currentlyDrawnPoint) {
								ChangePointSelection(currentlyDrawnPoint.curve[i], false);
							}
						}
						ChangePointSelection(currentlyDrawnPoint, true);
					}
				}
			}

			if (currentlyDrawnPoint.EditorSelected) {
				//Handles.DotCap(controlID, position, rotation, size);
                Handles.DotHandleCap(controlID, position, rotation, size, eventType);
			} else {
                //Handles.RectangleCap(controlID, position, rotation, size);
                Handles.RectangleHandleCap(controlID, position, rotation, size, eventType);
            }
		}

		private static void DrawPointSceneGUI(BezierPoint point) {
			currentlyDrawnPoint = point;
			Handles.Label(point.position + new Vector3(0, HandleUtility.GetHandleSize(point.position) * 0.4f, 0), point.gameObject.name);

			Handles.color = Color.green;
			Vector3 newPosition = Handles.FreeMoveHandle(point.position, point.transform.rotation, HandleUtility.GetHandleSize(point.position) * 0.1f, Vector3.zero, SelectedPointCap);
            
			if (newPosition != point.position) {
				Vector3 deltaPosition = newPosition - point.position;
				for (int i = 0; i < point.curve.pointCount; i++) {
					BezierPoint p = point.curve[i];
					if (p.EditorSelected) {
						Undo.RecordObject(p.transform, "Move Point");
						p.transform.position = p.transform.position + deltaPosition;
						p.EditorConditionalUpdateCollision();
					}
				}
			}

			if (point.handleStyle != BezierPoint.HandleStyle.None) {
				Handles.color = Color.cyan;
				Vector3 newGlobal1 = Handles.FreeMoveHandle(point.globalHandle1, point.transform.rotation, HandleUtility.GetHandleSize(point.globalHandle1) * 0.075f, Vector3.zero, Handles.CircleHandleCap);
				if (point.globalHandle1 != newGlobal1) {
					Undo.RecordObject(point, "Move Handle");
					point.globalHandle1 = newGlobal1;
					if (point.handleStyle == BezierPoint.HandleStyle.Connected) point.globalHandle2 = -(newGlobal1 - point.position) + point.position;
					point.EditorConditionalUpdateCollision();
				}

				Vector3 newGlobal2 = Handles.FreeMoveHandle(point.globalHandle2, point.transform.rotation, HandleUtility.GetHandleSize(point.globalHandle2) * 0.075f, Vector3.zero, Handles.CircleHandleCap);
				if (point.globalHandle2 != newGlobal2) {
					Undo.RecordObject(point, "Move Handle");
					point.globalHandle2 = newGlobal2;
					if (point.handleStyle == BezierPoint.HandleStyle.Connected) point.globalHandle1 = -(newGlobal2 - point.position) + point.position;
					point.EditorConditionalUpdateCollision();
				}

				Handles.color = Color.yellow;
				Handles.DrawLine(point.position, point.globalHandle1);
				Handles.DrawLine(point.position, point.globalHandle2);
			}
		}

		public static void DrawOtherPoints(BezierCurve curve, BezierPoint caller) {
			foreach (BezierPoint p in curve.GetAnchorPoints()) {
				if (p != caller) DrawPointSceneGUI(p);
			}
		}

		[MenuItem("GameObject/Create Other/Bezier Curve")]
		public static void CreateCurve(MenuCommand command) {
			GameObject curveObject = new GameObject("BezierCurve");
			Undo.RecordObject(curveObject, "Undo Create Curve");
			BezierCurve curve = curveObject.AddComponent<BezierCurve>();

			BezierPoint p1 = curve.AddPointAt(Vector3.forward * 0.5f);
			p1.handleStyle = BezierPoint.HandleStyle.Connected;
			p1.handle1 = new Vector3(-0.28f, 0, 0);

			BezierPoint p2 = curve.AddPointAt(Vector3.right * 0.5f);
			p2.handleStyle = BezierPoint.HandleStyle.Connected;
			p2.handle1 = new Vector3(0, 0, 0.28f);

			BezierPoint p3 = curve.AddPointAt(-Vector3.forward * 0.5f);
			p3.handleStyle = BezierPoint.HandleStyle.Connected;
			p3.handle1 = new Vector3(0.28f, 0, 0);

			BezierPoint p4 = curve.AddPointAt(-Vector3.right * 0.5f);
			p4.handleStyle = BezierPoint.HandleStyle.Connected;
			p4.handle1 = new Vector3(0, 0, -0.28f);

			curve.close = true;
		}
	}
}