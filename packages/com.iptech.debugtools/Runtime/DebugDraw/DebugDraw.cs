using UnityEngine;
using System.Collections.Generic;
using System;

namespace IPTech.DebugTools
{
    public interface IDebugDraw {
        void Sphere(Vector3 center, float radius);
        void GizmosDelegate(Action onDrawGizmos);
        void Text(Vector3 center, string text);
        void Line(Vector3 startPoint, Vector3 endPoint);
        void WireCube(Rect rect);
        void WireCube(Rect rect, Matrix4x4 matrix);
        void WireCube(Vector3 center, Vector3 size);
        void WireCube(Vector3 center, Vector3 size, Matrix4x4 matrix);
    }

	[ExecuteInEditMode]
	public class DebugDraw : MonoBehaviour, IDebugDraw
	{
        
		public interface IDebugDrawer
		{
			void OnDrawGizmos(GameObject gameObject);
		}

		private IList<IDebugDrawer> DebugDrawList;
		private IList<int> RemoveList;

		public DebugDraw() {
			DebugDrawList = new List<IDebugDrawer>();
			RemoveList = new List<int>();
		}

		private void Update() {
			for (int i = DebugDrawList.Count - 1; i >= 0; i--) {
				if (RemoveList.Contains(i)) {
					DebugDrawList.RemoveAt(i);
				}
			}
			RemoveList.Clear();
		}

		private void OnDrawGizmos() {
			for (int i = DebugDrawList.Count - 1; i >= 0; i--) {
				IDebugDrawer dd = DebugDrawList[i];
				dd.OnDrawGizmos(this.gameObject);
				if (!RemoveList.Contains(i)) {
					RemoveList.Add(i);
				}
			}
		}

		private void Queue(IDebugDrawer debugDrawObject) {
			DebugDrawList.Add(debugDrawObject);
		}

		public void Sphere(Vector3 center, float radius) {
			Queue(new DDSphere(center, radius));
		}

        public void GizmosDelegate(Action onDrawGizmos) {
			Queue(new DDDelegate(onDrawGizmos));
		}

		public void Text(Vector3 center, string text) {
			Queue(new DDText(text, center));
		}

		public void Line(Vector3 startPoint, Vector3 endPoint) {
			Queue(new DDLine(startPoint, endPoint));
		}

		public void WireCube(Rect rect) {
			WireCube(rect, Matrix4x4.identity);
		}

		public void WireCube(Rect rect, Matrix4x4 matrix) {
			Queue(new DDCube(rect.center, rect.size, matrix, true));
		}

		public void WireCube(Vector3 center, Vector3 size) {
			WireCube(center, size, Matrix4x4.identity);
		}

		public void WireCube(Vector3 center, Vector3 size, Matrix4x4 matrix) {
			Queue(new DDCube(center, size, matrix, true));
		}

		private class DDDelegate : IDebugDrawer
		{
			private Action onDrawGizmos;

			public DDDelegate(Action onDrawGizmos) {
				this.onDrawGizmos = onDrawGizmos;
			}

			public void OnDrawGizmos(GameObject gameObject) {
				onDrawGizmos();
			}
		}

		private class DDSphere : IDebugDrawer
		{
			private Vector3 center;
			private float radius;

			public DDSphere(Vector3 center, float radius) {
				this.center = center;
				this.radius = radius;
			}

			public void OnDrawGizmos(GameObject gameObject) {
				UnityEngine.Gizmos.DrawSphere(center, radius);
			}
		}

		private class DDText : IDebugDrawer
		{
			private string text;
			private Vector3 worldPosition;

			public DDText(string text, Vector3 worldPosition) {
				this.text = text;
				this.worldPosition = worldPosition;
			}

			public void OnDrawGizmos(GameObject gameObject) {
#if UNITY_EDITOR
				UnityEditor.Handles.Label(worldPosition, text);
#endif
				//Camera cam = Camera.current;
				//Vector3 screenPos = cam.WorldToScreenPoint(this.worldPosition);
				//GUI.Label(new Rect(screenPos.x, screenPos.y, 100, 10), text);
			}
		}

		private class DDLine : IDebugDrawer
		{
			private Vector3 startPoint;
			private Vector3 endPoint;

			public DDLine(Vector3 startPoint, Vector3 endPoint) {
				this.startPoint = startPoint;
				this.endPoint = endPoint;
			}

			public void OnDrawGizmos(GameObject gameObject) {
				UnityEngine.Gizmos.DrawLine(startPoint, endPoint);
			}
		}

		private class DDCube : IDebugDrawer
		{
			private Vector3 center;
			private Vector3 size;
			private Matrix4x4 matrix;
			private bool wire;

			public DDCube(Vector3 center, Vector3 size, Matrix4x4 matrix, bool wire) {
				this.center = center;
				this.size = size;
				this.matrix = matrix;
				this.wire = wire;
			}

			public void OnDrawGizmos(GameObject gameObject) {
				Matrix4x4 origMatrix = Gizmos.matrix;
				Gizmos.matrix = this.matrix;
				if (wire) {
					Gizmos.DrawWireCube(this.center, this.size);
				} else {
					Gizmos.DrawCube(this.center, this.size);
				}
				Gizmos.matrix = origMatrix;
			}
		}
	}
}