using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.HexMap {
	
	public static class HexMetrics {
		static Vector3[] _hexPoints;
		static Mesh _hexColumnMesh;

		public const float outerRadius = 1f;
		public static float innerRadius = outerRadius * Mathf.Cos(Mathf.Deg2Rad * 30.0f);

		public static Vector3[] hexPoints {
			get {
				if(_hexPoints == null) {
					_hexPoints = new Vector3[6];
					float deg = -30.0f;
					for(int i = 0; i < 6; i++) {
						_hexPoints[i] = Quaternion.AngleAxis(deg, Vector3.up) * Vector3.forward * outerRadius;
						deg += 60.0f;
					}
				}
				return _hexPoints;
			}
		}

		public static Mesh hexColumnMesh {
			get {
				if(_hexColumnMesh == null) {
					_hexColumnMesh = CreateHexColumnMesh();
				}
				return _hexColumnMesh;
			}
		}

		static Mesh CreateHexColumnMesh() {
			List<Vector3> verts = new List<Vector3>(12);
			List<int> indices = new List<int>();
			List<Color> colors = new List<Color>(12);

			Color color = Color.white;
			color.a = 0.25f;

			verts.Add(Vector3.zero);
			colors.Add(color);

			int i = 1;
			foreach(Vector3 p in hexPoints) {
				verts.Add(p);
				verts.Add(p + Vector3.up);

				colors.Add(color);
				colors.Add(color);

				if(i == 11) {
					indices.AddRange(new int[] { i , i - 10, 0 } );
					indices.AddRange(new int[] { i , i - 10, i + 1 });
					indices.AddRange(new int[] { i + 1, i - 9, 13 });
					indices.AddRange(new int[] { i + 1, i - 10, i - 9 }); 
				} else {
					indices.AddRange(new int[] { i, i + 2, 0 } );
					indices.AddRange(new int[] { i, i + 2, i + 1 });
					indices.AddRange(new int[] { i + 1, i + 3, 13 });
					indices.AddRange(new int[] { i + 1, i + 2, i + 3 }); 
				}
				i += 2;
			}

			verts.Add(Vector3.up);
			colors.Add(color);

			Mesh mesh = new Mesh();
			mesh.SetVertices(verts);
			mesh.SetTriangles(indices, 0);
			mesh.SetColors(colors);
			mesh.RecalculateNormals();

			mesh.UploadMeshData(true);
			return mesh;
		}
	}
}
