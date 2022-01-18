#region UsingStatements

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace IPTech.Splines
{
	/// <summary>
	/// 	- Class for describing and drawing Bezier Curves
	/// 	- Efficiently handles approximate length calculation through 'dirty' system
	/// 	- Has static functions for getting points on curves constructed by Vector3 parameters (GetPoint, GetCubicPoint, GetQuadraticPoint, and GetLinearPoint)
	/// </summary>
	[ExecuteInEditMode]
	[Serializable]
	public class BezierCurve : MonoBehaviour
	{

		#region EditorVariable
#if UNITY_EDITOR
		public bool EditorSelected { get; set; }
#endif
		#endregion

		public enum EInsertType
		{
			BeforeCurrentPoint,
			AfterCurrentPoint,
		}

		#region PublicVariables

		/// <summary>
		///  	- the number of mid-points calculated for each pair of bezier points
		///  	- used for drawing the curve in the editor
		///  	- used for calculating the "length" variable
		/// </summary>
		public int resolution = 30;

		public float extrusionWidth = 1.0f;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="BezierCurve"/> is dirty.
		/// </summary>
		/// <value>
		/// <c>true</c> if dirty; otherwise, <c>false</c>.
		/// </value>
		public bool dirty { get; private set; }

		/// <summary>
		/// 	- color this curve will be drawn with in the editor
		///		- set in the editor
		/// </summary>
		public Color drawColor = Color.white;


		#endregion

		#region PublicProperties

		/// <summary>
		///		- set in the editor
		/// 	- used to determine if the curve should be drawn as "closed" in the editor
		/// 	- used to determine if the curve's length should include the curve between the first and the last points in "points" array
		/// 	- setting this value will cause the curve to become dirty
		/// </summary>
		[SerializeField]
		private bool _close;
		public bool close {
			get { return _close; }
			set {
				if (_close == value) return;
				_close = value;
				dirty = true;
			}
		}

		/// <summary>
		///		- set internally
		///		- gets point corresponding to "index" in "points" array
		///		- does not allow direct set
		/// </summary>
		/// <param name='index'>
		/// 	- the index
		/// </param>
		public BezierPoint this[int index] {
			get { return points[index]; }
		}

		/// <summary>
		/// 	- number of points stored in 'points' variable
		///		- set internally
		///		- does not include "handles"
		/// </summary>
		/// <value>
		/// 	- The point count
		/// </value>
		public int pointCount {
			get { return points.Length; }
		}

		/// <summary>
		/// 	- The approximate length of the curve
		/// 	- recalculates if the curve is "dirty"
		/// </summary>
		private float _length;
		public float length {
			get {
				if (dirty) {
					_length = 0;
					for (int i = 0; i < points.Length - 1; i++) {
						_length += ApproximateLength(points[i], points[i + 1], resolution);
					}

					if (close) _length += ApproximateLength(points[points.Length - 1], points[0], resolution);

					dirty = false;
				}

				return _length;
			}
		}

		#endregion

		#region PrivateVariables

		/// <summary> 
		/// 	- Array of point objects that make up this curve
		///		- Populated through editor
		/// </summary>
		[SerializeField]
		private BezierPoint[] points = new BezierPoint[0];

		#endregion

		#region UnityFunctions

		void Awake() {
			dirty = true;
		}


		#endregion

		#region PrivateFunctions



		#endregion

		#region PublicFunctions

		public BezierPoint AddPoint() {
			return AddPointAt(transform.position);
		}

		/// <summary>
		/// 	- Adds the given point to the end of the curve ("points" array)
		/// </summary>
		/// <param name='point'>
		/// 	- The point to add.
		/// </param>
		public BezierPoint AddPoint(BezierPoint point) {
			List<BezierPoint> tempArray = new List<BezierPoint>(points);
			tempArray.Add(point);
			points = tempArray.ToArray();
			dirty = true;
			return point;
		}

		/// <summary>
		/// 	- Adds a point at position
		/// </summary>
		/// <returns>
		/// 	- The point object
		/// </returns>
		/// <param name='position'>
		/// 	- Where to add the point
		/// </param>
		public BezierPoint AddPointAt(Vector3 position) {
			GameObject pointObject = new GameObject("Point " + pointCount);

			pointObject.transform.parent = transform;
			pointObject.transform.position = position;

			BezierPoint newPoint = pointObject.AddComponent<BezierPoint>();
			newPoint.curve = this;

			return newPoint;
		}

		public BezierPoint InsertPoint(BezierPoint point, EInsertType insertType) {
			BezierPoint[] pointArray = new BezierPoint[2]
			{
			point, point
			};

			int index = GetPointIndex(point);
			int startIndex = pointCount + 1;
			if (insertType == EInsertType.BeforeCurrentPoint) {
				if (index == 0) {
					pointArray[0] = points[pointCount - 1];
				} else {
					startIndex = index - 1;
					pointArray[0] = points[index - 1];
				}
			} else {
				if (index == pointCount - 1) {
					pointArray[1] = points[0];
				} else {
					startIndex = index + 1;
					pointArray[1] = points[index + 1];
				}
			}

			Vector3 pos = CalculatePointOnCurve(pointArray[0], pointArray[1], 0.5f);

			BezierPoint newPoint = AddPointAt(pos);

			BezierPoint swap = points[pointCount - 1];
			for (int i = startIndex; i < pointCount; i++) {
				BezierPoint tmp = points[i];
				points[i] = swap;
				swap = tmp;
			}

			return newPoint;
		}

		/// <summary>
		/// 	- Removes the given point from the curve ("points" array)
		/// </summary>
		/// <param name='point'>
		/// 	- The point to remove
		/// </param>
		public void RemovePoint(BezierPoint point) {
			List<BezierPoint> tempArray = new List<BezierPoint>(points);
			tempArray.Remove(point);
			points = tempArray.ToArray();
			dirty = false;
		}

		/// <summary>
		/// 	- Gets a copy of the bezier point array used to define this curve
		/// </summary>
		/// <returns>
		/// 	- The cloned array of points
		/// </returns>
		public BezierPoint[] GetAnchorPoints() {
			return (BezierPoint[])points.Clone();
		}

		/// <summary>
		/// 	- Gets the point at 't' percent along this curve
		/// </summary>
		/// <returns>
		/// 	- Returns the point at 't' percent
		/// </returns>
		/// <param name='t'>
		/// 	- Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
		/// </param>
		public Vector3 CalculatePointOnCurveAt(float t) {
			BezierPoint p1, p2;
			float timeBetweenPoints;
			CalculatePointsAndPctBetweenThemAt(t, out p1, out p2, out timeBetweenPoints);
			return CalculatePointOnCurve(p1, p2, timeBetweenPoints);
		}

		public Vector3 CalculatePointOnCurveAt(float t, out Vector3 tangent) {
			BezierPoint p1, p2;
			float timeBetweenPoints;
			CalculatePointsAndPctBetweenThemAt(t, out p1, out p2, out timeBetweenPoints);

			tangent = CalculateTangentOnCurve(p1, p2, timeBetweenPoints);
			return CalculatePointOnCurve(p1, p2, timeBetweenPoints);
		}

		void CalculatePointsAndPctBetweenThemAt(float t, out BezierPoint p1, out BezierPoint p2, out float timeBetweenPoints) { 
			//if (t <= 0) return points[0].position;
			//else if (t >= 1) return points[points.Length - 1].position;

			float totalPercent = 0;
			float curvePercent = 0;
			float curveLength = length;

			p1 = null;
			p2 = null;

			for (int i = 0; i < points.Length - 1; i++) {
				curvePercent = ApproximateLength(points[i], points[i + 1], this.resolution) / curveLength;
				if (totalPercent + curvePercent > t) {
					p1 = points[i];
					p2 = points[i + 1];
					break;
				} else totalPercent += curvePercent;
			}

			if (close && p1 == null) {
				p1 = points[points.Length - 1];
				p2 = points[0];
			} else if(p1 == null) {
				//return points[points.Length - 1].transform.position;
				p1 = points[points.Length - 2];
				p2 = points[points.Length - 1];
				totalPercent -= ApproximateLength(p1, p2, this.resolution) / curveLength;
			}

			t -= totalPercent;

			timeBetweenPoints = t / curvePercent;
		}

		/// <summary>
		/// 	- Get the index of the given point in this curve
		/// </summary>
		/// <returns>
		/// 	- The index, or -1 if the point is not found
		/// </returns>
		/// <param name='point'>
		/// 	- Point to search for
		/// </param>
		public int GetPointIndex(BezierPoint point) {
			int result = -1;
			for (int i = 0; i < points.Length; i++) {
				if (points[i] == point) {
					result = i;
					break;
				}
			}

			return result;
		}

		public BezierPoint GetNextPoint(BezierPoint startPoint) {
			int nextIndex = GetPointIndex(startPoint) + 1;
			if (nextIndex == points.Length) {
				if (!close) {
					return null;
				}
				nextIndex = 0;
			}

			return points[nextIndex];
		}

		/// <summary>
		/// 	- Sets this curve to 'dirty'
		/// 	- Forces the curve to recalculate its length
		/// </summary>
		public void SetDirty() {
			dirty = true;
		}

		#endregion

		#region PublicStaticFunctions

		/// <summary>
		/// 	- Gets the point 't' percent along a curve
		/// 	- Automatically calculates for the number of relevant points
		/// </summary>
		/// <returns>
		/// 	- The point 't' percent along the curve
		/// </returns>
		/// <param name='p1'>
		/// 	- The bezier point at the beginning of the curve
		/// </param>
		/// <param name='p2'>
		/// 	- The bezier point at the end of the curve
		/// </param>
		/// <param name='t'>
		/// 	- Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
		/// </param>
		public static Vector3 CalculatePointOnCurve(BezierPoint p1, BezierPoint p2, float t) {
			if (p1.handle2 != Vector3.zero) {
				if (p2.handle1 != Vector3.zero) return CalculateCubicCurvePoint(p1.position, p1.globalHandle2, p2.globalHandle1, p2.position, t);
				else return CalculateQuadraticCurvePoint(p1.position, p1.globalHandle2, p2.position, t);
			} else {
				if (p2.handle1 != Vector3.zero) return CalculateQuadraticCurvePoint(p1.position, p2.globalHandle1, p2.position, t);
				else return CalculateLinearPoint(p1.position, p2.position, t);
			}
		}

		public static Vector3 CalculateTangentOnCurve(BezierPoint p1, BezierPoint p2, float t) {
			if (p1.handle2 != Vector3.zero) {
				if (p2.handle1 != Vector3.zero) return CalculateCubicCurveTangent(p1.position, p1.globalHandle2, p2.globalHandle1, p2.position, t);
				else return CalculateQuadraticCurveTangent(p1.position, p1.globalHandle2, p2.position, t);
			} else {
				if (p2.handle1 != Vector3.zero) return CalculateQuadraticCurveTangent(p1.position, p2.globalHandle1, p2.position, t);
				else return p2.position - p1.position;
			}
		}

		/// <summary>
		/// 	- Gets the point 't' percent along a third-order curve
		/// </summary>
		/// <returns>
		/// 	- The point 't' percent along the curve
		/// </returns>
		/// <param name='p1'>
		/// 	- The point at the beginning of the curve
		/// </param>
		/// <param name='p2'>
		/// 	- The second point along the curve
		/// </param>
		/// <param name='p3'>
		/// 	- The third point along the curve
		/// </param>
		/// <param name='p4'>
		/// 	- The point at the end of the curve
		/// </param>
		/// <param name='t'>
		/// 	- Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
		/// </param>
		public static Vector3 CalculateCubicCurvePoint(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t) {
			//t = Mathf.Clamp01(t);

			Vector3 part1 = Mathf.Pow(1 - t, 3) * p1;
			Vector3 part2 = 3 * Mathf.Pow(1 - t, 2) * t * p2;
			Vector3 part3 = 3 * (1 - t) * Mathf.Pow(t, 2) * p3;
			Vector3 part4 = Mathf.Pow(t, 3) * p4;

			return part1 + part2 + part3 + part4;
		}

		public static Vector3 CalculateCubicCurveTangent(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float t) {
			t = Mathf.Clamp01(t);

			Vector3 part1 = -3 * Mathf.Pow(1 - t, 2) * p1;
			Vector3 part2 = 3 * (t - 1) * ((3 * t) - 1) * p2;
			Vector3 part3 = -3 * t * (3 * t - 2) * p3;
			Vector3 part4 = 3 * Mathf.Pow(t, 2) * p4;

			return part1 + part2 + part3 + part4;
		}

		/// <summary>
		/// 	- Gets the point 't' percent along a second-order curve
		/// </summary>
		/// <returns>
		/// 	- The point 't' percent along the curve
		/// </returns>
		/// <param name='p1'>
		/// 	- The point at the beginning of the curve
		/// </param>
		/// <param name='p2'>
		/// 	- The second point along the curve
		/// </param>
		/// <param name='p3'>
		/// 	- The point at the end of the curve
		/// </param>
		/// <param name='t'>
		/// 	- Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
		/// </param>
		public static Vector3 CalculateQuadraticCurvePoint(Vector3 p1, Vector3 p2, Vector3 p3, float t) {
			t = Mathf.Clamp01(t);

			Vector3 part1 = Mathf.Pow(1 - t, 2) * p1;
			Vector3 part2 = 2 * (1 - t) * t * p2;
			Vector3 part3 = Mathf.Pow(t, 2) * p3;

			return part1 + part2 + part3;
		}

		public static Vector3 CalculateQuadraticCurveTangent(Vector3 p1, Vector3 p2, Vector3 p3, float t) {
			t = Mathf.Clamp01(t);

			Vector3 part1 = -2 * (1 - t) * p1;
			Vector3 part2 = 2 * (1 - 2 * t) * p2;
			Vector3 part3 = 2 * t * p3;

			return part1 + part2 + part3;
		}

		/// <summary>
		/// 	- Gets point 't' percent along a linear "curve" (line)
		/// 	- This is exactly equivalent to Vector3.Lerp
		/// </summary>
		/// <returns>
		///		- The point 't' percent along the curve
		/// </returns>
		/// <param name='p1'>
		/// 	- The point at the beginning of the line
		/// </param>
		/// <param name='p2'>
		/// 	- The point at the end of the line
		/// </param>
		/// <param name='t'>
		/// 	- Value between 0 and 1 representing the percent along the line (0 = 0%, 1 = 100%)
		/// </param>
		public static Vector3 CalculateLinearPoint(Vector3 p1, Vector3 p2, float t) {
			return p1 + ((p2 - p1) * t);
		}

		/// <summary>
		/// 	- Gets point 't' percent along n-order curve
		/// </summary>
		/// <returns>
		/// 	- The point 't' percent along the curve
		/// </returns>
		/// <param name='t'>
		/// 	- Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%)
		/// </param>
		/// <param name='points'>
		/// 	- The points used to define the curve
		/// </param>
		public static Vector3 CalculatePoint(float t, params Vector3[] points) {
			t = Mathf.Clamp01(t);

			int order = points.Length - 1;
			Vector3 point = Vector3.zero;
			Vector3 vectorToAdd;

			for (int i = 0; i < points.Length; i++) {
				vectorToAdd = points[points.Length - i - 1] * (BinomialCoefficient(i, order) * Mathf.Pow(t, order - i) * Mathf.Pow((1 - t), i));
				point += vectorToAdd;
			}

			return point;
		}

		/// <summary>
		/// 	- Approximates the length
		/// </summary>
		/// <returns>
		/// 	- The approximate length
		/// </returns>
		/// <param name='p1'>
		/// 	- The bezier point at the start of the curve
		/// </param>
		/// <param name='p2'>
		/// 	- The bezier point at the end of the curve
		/// </param>
		/// <param name='resolution'>
		/// 	- The number of points along the curve used to create measurable segments
		/// </param>
		public static float ApproximateLength(BezierPoint p1, BezierPoint p2, int resolution = 10) {
			float _res = resolution;
			float total = 0;
			Vector3 lastPosition = p1.position;
			Vector3 currentPosition;

			for (int i = 0; i < resolution + 1; i++) {
				currentPosition = CalculatePointOnCurve(p1, p2, i / _res);
				total += (currentPosition - lastPosition).magnitude;
				lastPosition = currentPosition;
			}

			return total;
		}

		public static Bounds CalculateBounds(BezierPoint p1, BezierPoint p2, int resolution = 10) {
			int limit = resolution + 1;
			float _res = resolution;
			Vector3 currentPoint = p1.position;

			Bounds bounds = new Bounds();

			for (int i = 0; i < limit; i++) {
				currentPoint = CalculatePointOnCurve(p1, p2, i / _res);
				currentPoint = p1.transform.InverseTransformPoint(currentPoint);
				bounds.Encapsulate(currentPoint);
			}
			bounds.center = bounds.center + p1.position;
			return bounds;
		}

		public static Quaternion CalculateTheCurveRotationAtTime(BezierPoint p1, BezierPoint p2, float t) {
			Quaternion rot = Quaternion.Lerp(p1.transform.rotation, p2.transform.rotation, t);
			Vector3 right = Vector3.right;
			Vector3 tangent = CalculateTangentOnCurve(p1, p2, t);
			Vector3 up = Vector3.Cross(right, tangent).normalized;
			rot.SetLookRotation(tangent, up);
			return rot;
		}

		#endregion

		#region UtilityFunctions

		private static int BinomialCoefficient(int i, int n) {
			return Factoral(n) / (Factoral(i) * Factoral(n - i));
		}

		private static int Factoral(int i) {
			if (i == 0) return 1;

			int total = 1;

			while (i - 1 >= 0) {
				total *= i;
				i--;
			}

			return total;
		}

		#endregion

		public Vector3 CalculatePointAtDistance(float distance)
		{
			if(close)
			{
				if(distance < 0) while(distance < 0) { distance += length; }
				else if(distance > length) while(distance > length) { distance -= length; }
			}

			float t = distance / length;
			return CalculatePointOnCurveAt(t);
		}
		
	}
}