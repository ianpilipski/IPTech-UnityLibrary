#region UsingStatements

using UnityEngine;
using System;
using System.Collections;

using IPTech.DebugTools;
#endregion

namespace IPTech.Splines
{
	/// <summary>
	/// 	- Helper class for storing and manipulating Bezier Point data
	/// 	- Ensures that handles are in correct relation to one another
	/// 	- Handles adding/removing self from curve point lists
	/// 	- Calls SetDirty() on curve when edited 
	/// </summary>
	[Serializable]
	public class BezierPoint : MonoBehaviour
	{
        #region PrivateVariables
        [SerializeField]
        private BoxCollider boxCollider;

        /// <summary>
        /// 	- Used to determine if this point has moved since the last frame
        /// </summary>
        private Vector3 lastPosition;

        private IDebugDraw debugDraw;

        #endregion

        #region EditorVariable
#if UNITY_EDITOR
        public bool EditorSelected = false;
#endif
        #endregion

        #region PublicProperties

        /// <summary>
        /// 	- Enumeration describing the relationship between a point's handles
        /// 	- Connected : The point's handles are mirrored across the point
        /// 	- Broken : Each handle moves independently of the other
        /// 	- None : This point has no handles (both handles are located ON the point)
        /// </summary>
        public enum HandleStyle {
            Connected,
            Broken,
            None,
        }

        public BezierPoint NextPoint {
			get {
				if (this._curve != null) return this._curve.GetNextPoint(this);
				return null;
			}
		}

		/// <summary>
		///		- Curve this point belongs to
		/// 	- Changing this value will automatically remove this point from the current curve and add it to the new one
		/// </summary>
		[SerializeField]
		private BezierCurve _curve;
		public BezierCurve curve {
			get { return _curve; }
			set {
				if (_curve) _curve.RemovePoint(this);
				_curve = value;
				_curve.AddPoint(this);
			}
		}

		public HandleStyle handleStyle;

		public Vector3 position {
			get { return transform.position; }
			set { transform.position = value; }
		}

		public Vector3 localPosition {
			get { return transform.localPosition; }
			set { transform.localPosition = value; }
		}

		[SerializeField]
		private Vector3 _handle1;
		public Vector3 handle1 {
			get { return _handle1; }
			set {
				if (_handle1 == value) return;
				_handle1 = value;
				if (handleStyle == HandleStyle.None) handleStyle = HandleStyle.Broken;
				else if (handleStyle == HandleStyle.Connected) _handle2 = -value;
				_curve.SetDirty();
			}
		}

		public Vector3 globalHandle1 {
			get { return transform.TransformPoint(handle1); }
			set { handle1 = transform.InverseTransformPoint(value); }
		}

		[SerializeField]
		private Vector3 _handle2;
		public Vector3 handle2 {
			get { return _handle2; }
			set {
				if (_handle2 == value) return;
				_handle2 = value;
				if (handleStyle == HandleStyle.None) handleStyle = HandleStyle.Broken;
				else if (handleStyle == HandleStyle.Connected) _handle1 = -value;
				_curve.SetDirty();
			}
		}

		public Vector3 globalHandle2 {
			get { return transform.TransformPoint(handle2); }
			set { handle2 = transform.InverseTransformPoint(value); }
		}

		public bool hasCollision;
		#endregion

		#region Public Functions
		public struct CollisionResult
		{
			public float time;
			public Vector3 point;
			public Vector3 normal;
		}

		private float DistanceToRaySqr(Vector3 P, Vector3 origin, Vector3 ray) {
			Vector3 v = ray;
			Vector3 w = P - origin;
			float c1 = Vector3.Dot(w, v);
			if (c1 <= 0)
				return w.sqrMagnitude;

			float c2 = Vector3.Dot(v, v);
			if (c2 <= c1)
				return (P - (origin + ray)).sqrMagnitude;

			float b = c1 / c2;
			Vector3 Pb = origin + (b * v);

			return (P - Pb).sqrMagnitude;
		}

		public bool CollidesWith(Vector3 center, float radius, out CollisionResult collisionResult, int resolution = 10) {
			collisionResult = new CollisionResult();
			if (hasCollision) {
				Bounds localBounds = new Bounds(boxCollider.center, boxCollider.size);
				localBounds.Expand(radius * 2);
				Vector3 localCenter = transform.InverseTransformPoint(center);
                debugDraw.WireCube(localBounds.center, localBounds.size, transform.localToWorldMatrix);
				if (localBounds.Contains(localCenter)) {
                    debugDraw.Sphere(center, radius);
					BezierPoint point2 = curve.GetNextPoint(this);
					if (point2 != null) {
						int limit = resolution + 1;
						float res = (float)resolution;
						float sqrRadius = radius * radius;
						Vector3 prev = BezierCurve.CalculatePointOnCurve(this, point2, 0.0f);
						Vector3 minPoint = Vector3.zero;
						Vector3 minRay = Vector3.zero;
						float minTime = 0.0f;
						bool foundCollision = false;
						float minSqrMagnitude = sqrRadius;

						for (int i = 1; i < limit; i++) {
							float t = i / res;
							Vector3 p = BezierCurve.CalculatePointOnCurve(this, point2, t);
							Vector3 ray = p - prev;
							//float sqrDistance = Vector3.Cross(ray, center - prev).sqrMagnitude;
							float sqrDistance = DistanceToRaySqr(center, prev, ray);
							if (sqrDistance <= minSqrMagnitude) {
								foundCollision = true;
								minSqrMagnitude = sqrDistance;
								minPoint = prev;
								minRay = ray;
								minTime = t;
							}

							prev = p;
						}

						if (foundCollision) {
							Vector3 rayNormalized = minRay.normalized;
							float dot = Vector3.Dot((center - minPoint), rayNormalized);
							float scale = dot / minRay.magnitude;
                            debugDraw.Line(minPoint, center);
                            debugDraw.Line(minPoint, minPoint + minRay);
                            debugDraw.Text(minPoint, scale.ToString());
							collisionResult.point = minPoint + (minRay * scale);
							collisionResult.time = minTime + (1 / res) * scale;
							collisionResult.normal = Vector3.Normalize(center - collisionResult.point);
                            debugDraw.Sphere(collisionResult.point, 0.01f);
							return true;
						}
					}
				}
			}
			return false;
		}

		#endregion

		#region Private Functions
#if UNITY_EDITOR
		public void EditorConditionalUpdateCollision() {
			ConditionalUpdateCollision();
		}
#endif

		private BoxCollider CreateCollisionCollider() {
#if UNITY_EDITOR
			return UnityEditor.Undo.AddComponent<BoxCollider>(gameObject);
#else
		return gameObject.AddComponent<BoxCollider>();
#endif
		}

		private void DisableCollisioinCollider() {
			if (boxCollider != null) {
#if UNITY_EDITOR
				if (Application.isPlaying) {
					boxCollider.enabled = false;
				} else {
					UnityEditor.Undo.DestroyObjectImmediate(boxCollider);
				}
#else
			boxCollider.enabled = false;
#endif
			}
		}

		private void AddCollision() {
			Bounds collisionBounds = new Bounds();
			BezierPoint nextPoint = curve.GetNextPoint(this);
			if (nextPoint != null) {
				collisionBounds = BezierCurve.CalculateBounds(this, nextPoint);
				if (boxCollider == null) {
					boxCollider = CreateCollisionCollider();
					boxCollider.isTrigger = true;
				}
				boxCollider.center = (collisionBounds.center - transform.position);
				boxCollider.size = collisionBounds.extents * 2;
				boxCollider.enabled = true;
				return;
			} else {
				Debug.LogWarning("EndPoint with collision has no next point.");
			}
		}

		private void ConditionalUpdateCollision() {
			if (hasCollision) {
				AddCollision();
			} else {
				DisableCollisioinCollider();
			}
		}
		#endregion

		#region MonoBehaviourFunctions

		void Awake() {
            debugDraw = DebugDrawInst.Instance;
			ConditionalUpdateCollision();
		}

		void Update() {
			if (!_curve.dirty && transform.position != lastPosition) {
				_curve.SetDirty();
				lastPosition = transform.position;
				ConditionalUpdateCollision();
			}
		}

		void OnTriggerEnter(Collider other) {
			ConditionalEvaluateCollision(other);
		}

		void OnTriggerStay(Collider other) {
			ConditionalEvaluateCollision(other);
		}

		private void ConditionalEvaluateCollision(Collider other) {
			if (enabled) {
				CurveCollider curveCollider = other.gameObject.GetComponent<CurveCollider>();
				if (curveCollider != null) {
					CollisionResult collisionResult;
					if (CollidesWith(curveCollider.center, curveCollider.radius, out collisionResult)) {
						curveCollider.OnCurveCollision(this, collisionResult);
					}
				}
			}
		}

		#endregion
	}
}