using UnityEngine;
using System;
using System.Collections;
namespace IPTech.Splines
{
	public class BezierPointComparitor : Comparitor<BezierPointComparitorInfo>
	{
		[SerializeField]
		private BezierPointComparitorInfo Result;

		public override bool UpdateComparitor(BezierPointComparitorInfo info) {
			if (!info.Equals(Result)) {
				Result = info.Clone();
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(this);
#endif
				return true;
			}

			return false;
		}
	}

	[Serializable]
	public class BezierPointComparitorInfo
	{
		[Serializable]
		private struct PointData
		{
			public Vector3 position;
			public Vector3 handle1;
			public Vector3 handle2;

			public PointData(BezierPoint bezierPoint) {
				if (bezierPoint != null) {
					this.position = bezierPoint.transform.localPosition;
					this.handle1 = bezierPoint.handle1;
					this.handle2 = bezierPoint.handle2;
				} else {
					this.position = this.handle1 = this.handle2 = Vector3.zero;
				}
			}

			public override bool Equals(object obj) {
				if (obj is PointData) {
					PointData p = (PointData)obj;
					return this.position.Equals(p.position) &&
						this.handle1.Equals(p.handle1) &&
						this.handle2.Equals(p.handle2);
				}

				return false;
			}

			public override int GetHashCode() {
				return this.position.GetHashCode() ^ this.handle1.GetHashCode() ^ this.handle2.GetHashCode();
			}
		}

		private PointData point1;
		private PointData point2;

		public BezierPoint Point1 {
			set {
				this.point1 = new PointData(value);
			}
		}

		public BezierPoint Point2 {
			set {
				this.point2 = new PointData(value);
			}
		}

		public bool Equals(BezierPointComparitorInfo info) {
			if (info == null) {
				return false;
			}

			return this.point1.Equals(info.point1) && this.point2.Equals(info.point2);
		}

		public BezierPointComparitorInfo Clone() {
			return (BezierPointComparitorInfo)this.MemberwiseClone();
		}
	}
}