using UnityEngine;
using System.Collections;
namespace IPTech.Splines
{
	[RequireComponent(typeof(MeshCollider), typeof(BezierPoint))]
	public class BezierPointCollider : MonoBehaviour
	{
		//[SerializeField]
		//private Mesh collisionMesh;
		//private MeshCollider meshCollider;
		private BezierPoint bezierPoint;

		[SerializeField]
		private BezierPointComparitor lastResult;

		private BezierPointComparitorInfo PointComparitorInfo {
			get {
				BezierPointComparitorInfo info = new BezierPointComparitorInfo() {
					Point1 = bezierPoint,
					Point2 = bezierPoint.curve.GetNextPoint(bezierPoint)
				};
				return info;
			}
		}

		// Use this for initialization
		void Start() {
			bezierPoint = GetComponent<BezierPoint>();
			//meshCollider = GetComponent<MeshCollider>();
		}

#if UNITY_EDITOR
		void Update() {
			if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) {
				this.enabled = false;
			} else if (Refresh(false)) {
				UnityEditor.EditorUtility.SetDirty(this);
			}
		}
#endif

		void OnDestroy() {
			BezierPointComparitor.CleanUp(ref lastResult);
		}

		public bool Refresh(bool forceRefresh) {
			if (BezierPointComparitor.Refresh(this, ref lastResult, PointComparitorInfo) || forceRefresh) {
				// Do collision update here

				return true;
			}
			return false;
		}
	}
}