using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace IPTech.Splines
{
	[CustomEditor(typeof(BezierPointMeshExtruder))]
	class BezierPointMeshExtruderEditor : Editor
	{
        Vector3 _sliderPos1;
        bool _isDragging;

        private void OnEnable() {
                
        }

        public override void OnInspectorGUI() {
			DrawInspector(this.serializedObject);
			DrawDefaultInspector();
		}

		public static void DrawInspector(SerializedObject forObject) {

		}

		protected void OnSceneGUI() {
			BezierPointMeshExtruder t = target as BezierPointMeshExtruder;

			if (t == null) {
				Debug.Log("Not MeshExtuder");
				return;
			}

			if (t.bezierPoint == null) {
				Debug.Log("No point!");
				return;
			}

			if (t.bezierPoint.NextPoint == null) {
				Debug.Log("No NextPoint!");
				return;
			}

            float extrusionWidth = t.GetExtrusionWidth();
			Vector3 startPoint = t.bezierPoint.transform.position;
			Vector3 endPoint = t.bezierPoint.NextPoint.transform.position;

			Vector3 midPoint = startPoint + ((endPoint - startPoint) * 0.5f);

			Vector3 right = (endPoint - startPoint).normalized;
			Vector3 forward = Vector3.Cross(Vector3.up, right).normalized;
            Vector3 up = Vector3.Cross(right, forward).normalized;

            //Quaternion rotation = Quaternion.LookRotation(forward, up);
            //extrusionWidth = Handles.Slider(midPoint, right);
            //DrawArrow(midPoint, rotation, extrusionWidth);
            //DrawArrow(midPoint, forward, right * -1, extrusionWidth);
            float size = HandleUtility.GetHandleSize(midPoint + (forward * extrusionWidth * 0.5f)) * 0.25f;

            Vector3 handleLoc = midPoint + (forward * extrusionWidth * 0.5f) + (forward * size);
            EditorGUI.BeginChangeCheck();
            Vector3 newHandleLoc = Handles.Slider(handleLoc, forward, size, FlatArrowCap, 0F);
            if(EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(t, "Modify Extrusion Width");
                float delta = Vector3.Dot(forward, (newHandleLoc - handleLoc));
                float newExtrusionWidth = extrusionWidth + delta;
                t.SetExtrusionWidth(newExtrusionWidth, true);
                //EditorUtility.SetDirty(t);
            }
            
		}

        static int hoveringFlatArrowCap;
        private void FlatArrowCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType) {
            Vector3[] arrowHead = CalculateArrowHead(position, rotation, size);
            float distanceToMouse = HandleUtility.DistanceToPolyLine(arrowHead);
            float distanceToCenter = (Event.current.mousePosition - HandleUtility.WorldToGUIPoint(position)).magnitude;
            float minDist = Mathf.Min(distanceToMouse, distanceToCenter);

            EventType filteredEventType = Event.current.GetTypeForControl(controlID);
            switch (filteredEventType) {
                case EventType.Layout:
                    HandleUtility.AddControl(controlID, minDist);
                   if(GUIUtility.hotControl == 0) {
                        if (minDist < 10F) {
                            if (hoveringFlatArrowCap != controlID) {
                                hoveringFlatArrowCap = controlID;
                                HandleUtility.Repaint();
                            }
                        } else if(hoveringFlatArrowCap == controlID) {
                            hoveringFlatArrowCap = 0;
                            HandleUtility.Repaint();                            
                        }
                    }
                    break;
                case EventType.Repaint:
                    Handles.color = Color.red;
                    if (GUIUtility.hotControl == controlID || (GUIUtility.hotControl==0 && minDist<10F)) {
                        Handles.DrawAAConvexPolygon(arrowHead);
                    } else {
                        Handles.DrawAAPolyLine(arrowHead);
                    }                    
                    break;
            }
        }

        private static Vector3[] CalculateArrowHead(Vector3 position, Quaternion rotation, float size) {
            Vector3[] arrowHead = new Vector3[4];

            float width = size;
            float height = size * 0.6f;

            Vector3 right = rotation * Vector3.forward;
            Vector3 forward = rotation * Vector3.right;
            Vector3 arrowTip = position + right * height;

            arrowHead[0] = arrowTip;
            arrowHead[1] = arrowTip - right * height + forward * width;
            arrowHead[2] = arrowTip - right * height - forward * width;
            arrowHead[3] = arrowTip;
            return arrowHead;
        }
    }

}