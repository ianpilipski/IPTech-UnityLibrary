using UnityEngine;
using System.Collections;

namespace IPTech.Splines
{
	public class CurveCollider : MonoBehaviour
	{

		public Vector3 center {
			get {
				Collider col = gameObject.GetComponent<Collider>();
				if (col != null) {
					return col.bounds.center;
				}

				return transform.position;
			}
		}

		public float radius {
			get {
				Collider col = gameObject.GetComponent<Collider>();
				if (col != null) {
					if (col is SphereCollider) {
						return col.bounds.size.x / 2;
					}
					return col.bounds.extents.magnitude;
				}

				return 1.0f;
			}
		}

		public void OnCurveCollision(BezierPoint point, BezierPoint.CollisionResult collisionResult) {
			Debug.Log("ON CURVE COLLISION");
		}
	}
}