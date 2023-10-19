using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace IPTech.HexMap {
	
	[ExecuteInEditMode]
	public class HexGrid : MonoBehaviour {
#if UNITY_EDITOR
		public Color GridColor = new Color(Color.green.r, Color.green.g, Color.green.b, 0.5f);
		public Color HighlightColor = Color.magenta;
#endif
	}
}
