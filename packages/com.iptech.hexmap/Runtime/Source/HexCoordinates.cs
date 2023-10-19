using UnityEngine;

namespace IPTech.HexMap
{
	[System.Serializable]
	public class HexCoordinates
	{
		public int X { get; private set; }
		public int Y { get; private set; }
		public int Z {
			get {
				return -X - Y;
			}
		}

		public HexCoordinates(int x, int y) {
			X = x;
			Y = y;
		}

		public static HexCoordinates FromOffsetCoordinates (int col, int row) {
			return new HexCoordinates(col, row - col / 2);
		}

		public void ToOffsetCoordinates(out int col, out int row) {
			col = X;
			row = Y + (X / 2);
		}

		public static HexCoordinates FromPosition (Vector3 position) {
			float y = position.z / (HexMetrics.innerRadius * 2f);
			float z = -y;

			float offset = position.x / (HexMetrics.outerRadius * 3f);
			y -= offset;
			z -= offset;

			int iY = Mathf.RoundToInt(y);
			int iZ = Mathf.RoundToInt(z);
			int iX = Mathf.RoundToInt(-y -z);

			if (iX + iY + iZ != 0) {
				float dY = Mathf.Abs(y - iY);
				float dZ = Mathf.Abs(z - iZ);
				float dX = Mathf.Abs(-y -z - iX);

				if (dX > dY && dX > dZ) {
					iX = -iY - iZ;
				}
				else if (dY > dZ) {
					iY = -iX - iZ;
				}
			}

			return new HexCoordinates(iX, iY);
		}

		public Vector3 ToPosition() {
			int col;
			int row;
			ToOffsetCoordinates(out col, out row);
			return ToPosition(col, row);
		}

		public static Vector3 ToPosition(int col, int row) {
			float colOffset = (col * (HexMetrics.outerRadius * 1.5f));
			float rowOffset = (row + col * 0.5f - col / 2) * (HexMetrics.innerRadius * 2.0f);
			return new Vector3(colOffset, 0, rowOffset);
		}

		public override string ToString() {
			return string.Format("{0}, {1}, {2}", X, Y, Z);
		}
	}
}
