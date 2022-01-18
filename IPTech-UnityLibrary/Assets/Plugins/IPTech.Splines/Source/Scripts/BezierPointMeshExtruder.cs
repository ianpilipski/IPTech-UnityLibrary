using System.Collections;
using UnityEngine;
using System.Collections.Generic;
namespace IPTech.Splines {
    [RequireComponent(typeof(BezierPoint)), ExecuteInEditMode]
    public class BezierPointMeshExtruder : MonoBehaviour {

        public float overrideWidth = 1.0f;
        public int overrideResolution = 0;

        public Mesh mesh;
        public bool updateMeshFilter = true;
        public bool updateMeshCollider = true;

        private BezierPoint _bezierPoint;
        public BezierPoint bezierPoint {
            get {
                if (_bezierPoint == null) {
                    this._bezierPoint = GetComponent<BezierPoint>();
                }
                return this._bezierPoint;
            }
        }

        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        private BezierPoint[] pointArray = new BezierPoint[2];

        [SerializeField, HideInInspector]
        private BezierPointMeshExtruderComparitor lastResult;

        private BezierPointMeshExtruderComparitorInfo Info {
            get {
                BezierPointMeshExtruderComparitorInfo info = new BezierPointMeshExtruderComparitorInfo() {
                    ExtrusionWidth = GetExtrusionWidth(),
                    PointComparitorInfo = new BezierPointComparitorInfo() {
                        Point1 = bezierPoint,
                        Point2 = bezierPoint == null ? null : bezierPoint.NextPoint
                    }
                };
                return info;
            }
        }

		private void Start() {
			meshFilter = GetComponent<MeshFilter>();
			meshCollider = GetComponent<MeshCollider>();
		}

		private void Update() {
			Refresh(false);
		}

#if UNITY_EDITOR
		private void OnDrawGizmos() {
			//Allows realtime update to the mesh if editing the curve.
			Refresh(false);
		}
#endif

		private void OnDestroy() {
			BezierPointMeshExtruderComparitor.CleanUp(ref lastResult);
		}

		private bool Refresh(bool forceRefresh) {
			if (BezierPointMeshExtruderComparitor.Refresh(this, ref lastResult, Info) || forceRefresh) {
				UpdateExtrusionMesh();
				ConditionalUpdateMeshFilter();
				ConditionalUpdateMeshCollider();
				return true;
			}
			return false;
		}

		private void ConditionalUpdateMeshCollider() {
			if (this.meshCollider == null) {
				this.meshCollider = GetComponent<MeshCollider>();
				if (this.meshCollider == null) {
					this.meshCollider = gameObject.AddComponent<MeshCollider>();
				}
			}
			if (this.meshCollider.sharedMesh != this.mesh) {
				this.meshCollider.sharedMesh = this.mesh;
			}
		}
		private void ConditionalUpdateMeshFilter() {
			if (this.meshFilter == null) {
				meshFilter = GetComponent<MeshFilter>();
				if (meshFilter == null) {
					this.meshFilter = gameObject.AddComponent<MeshFilter>();
				}
			}
			if (this.meshFilter.sharedMesh != this.mesh) {
				this.meshFilter.sharedMesh = this.mesh;
			}
		}

		private int CalculateResolution() {
			if (this.overrideResolution == 0) {
				return Mathf.Max(bezierPoint.curve.resolution / bezierPoint.curve.pointCount, 1);
			}
			return this.overrideResolution;
		}

		public float GetExtrusionWidth() {
			if (this.overrideWidth == 0.0f) {
				return bezierPoint.curve.extrusionWidth;
			}
			return this.overrideWidth;
		}

        public void SetExtrusionWidth(float width, bool overrideValueOnly = false) {
            if(overrideValueOnly) {
                this.overrideWidth = width;
            } else {
                this.bezierPoint.curve.extrusionWidth = width;
            }
        }

		private void UpdateExtrusionMesh() {
			UpdatePointArray();
			if (this.mesh == null) {
				this.mesh = CreateRibbonMesh(pointArray, CalculateResolution(), GetExtrusionWidth());
				this.mesh.name = transform.parent.gameObject.name + ":" + gameObject.name;
			} else {
				UpdateRibbonMesh(this.mesh, pointArray, CalculateResolution(), GetExtrusionWidth());
			}
		}

		private void UpdatePointArray() {
			pointArray[0] = bezierPoint;
			pointArray[1] = bezierPoint.curve.GetNextPoint(bezierPoint);
		}

		public static Mesh CreateRibbonMesh(BezierPoint[] points, int resolution, float extrusionWidth) {
			Mesh mesh = new Mesh();
			UpdateRibbonMesh(mesh, points, resolution, extrusionWidth);
			return mesh;
		}

		public static void UpdateRibbonMesh(Mesh mesh, BezierPoint[] points, int resolution, float extrusionWidth) {
			if (points.Length > 1 && extrusionWidth > 0.0f) {
				List<Vector3> vertList = new List<Vector3>();
				for (int i = 0; i < points.Length - 1; i++) {
					CreateRibbonVerts(points[i], points[i + 1], resolution, vertList, extrusionWidth);
				}

				int[] indices = CreateIndexBuffer(vertList);
				mesh.Clear();
				mesh.vertices = vertList.ToArray();
				mesh.triangles = indices;
			} else {
				mesh.Clear();
			}
		}

		private static void CreateRibbonVerts(BezierPoint p1, BezierPoint p2, int resolution, List<Vector3> vertList, float extrusionWidth) {
			int limit = resolution + 1;
			float res = resolution;
			float width = extrusionWidth / 2;
			Vector3 currentPoint = Vector3.zero;
			for (int i = 0; i < limit; i++) {
				currentPoint = BezierCurve.CalculatePointOnCurve(p1, p2, i / res);
				Quaternion curveRot = BezierCurve.CalculateTheCurveRotationAtTime(p1, p2, i / res);
				Vector3 v1 = (curveRot * (Vector3.right * width)) + currentPoint - p1.transform.position;
				Vector3 v2 = (curveRot * (Vector3.right * -width)) + currentPoint - p1.transform.position;
				vertList.Add(v1);
				vertList.Add(v2);
			}
		}

		private static int[] CreateIndexBuffer(List<Vector3> vertList) {
			int numberofQuads = (vertList.Count / 2) - 1;
			int[] indexBuffer = new int[numberofQuads * 6];
			int bufferIndex = 0;
			for (int i = 0; i < numberofQuads; i++) {
				int ind1 = i * 2;
				indexBuffer[bufferIndex++] = ind1;
				indexBuffer[bufferIndex++] = ind1 + 1;
				indexBuffer[bufferIndex++] = ind1 + 2;
				indexBuffer[bufferIndex++] = ind1 + 1;
				indexBuffer[bufferIndex++] = ind1 + 3;
				indexBuffer[bufferIndex++] = ind1 + 2;
			}
			return indexBuffer;
		}
	}
}