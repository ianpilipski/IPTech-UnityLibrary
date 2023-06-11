using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.HexMap {

	public static class HexTile {

		public static void DrawHexGizmo(Matrix4x4 matrix, Color color, float columnHeight = 0) {
			
			Color origColor = Gizmos.color;
			Matrix4x4 origMatrix = Gizmos.matrix;

			Gizmos.color = color;
			Gizmos.matrix = matrix;

			DrawHex();

			if(Mathf.Abs(columnHeight) > 0F) {
				Vector3 scaleUpVect = new Vector3(1, columnHeight, 1);
				Matrix4x4 heightOffset = Matrix4x4.Scale(scaleUpVect);
				Gizmos.matrix = matrix * heightOffset;
				Color c = Gizmos.color;
				c.a = 0.35f;
				Gizmos.color = c;
				Gizmos.DrawMesh(HexMetrics.hexColumnMesh);
			}

			Gizmos.matrix = origMatrix;
			Gizmos.color = origColor;
		}

		static void DrawHex() {
			Vector3 prevPoint = HexMetrics.hexPoints[HexMetrics.hexPoints.Length - 1];
			for(int i = 0; i < HexMetrics.hexPoints.Length; i++) {
				Gizmos.DrawLine(prevPoint, HexMetrics.hexPoints[i]);
				prevPoint = HexMetrics.hexPoints[i];
			}
		}
	}
}
