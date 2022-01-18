/*
Copyright (C) 2014 Brendan Vance

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR
ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class QuadGenerator : MonoBehaviour, RefreshTarget
{
	public QuadGeneratorInfo Info = new QuadGeneratorInfo();
	public int SomeInessentialProperty = 0;

	[HideInInspector]
	private Mesh QuadMesh;

	[SerializeField, HideInInspector]
	private QuadGeneratorResult LastResult = null;
	
	#if UNITY_EDITOR
	void Update () {
		if(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) {
			this.enabled = false;
		} else if(Refresh(false)) {
			UnityEditor.EditorUtility.SetDirty(this);
		}
	}
	#endif

	void OnDestroy()
	{
		QuadGeneratorResult.CleanUp(ref LastResult);
	}
	
	public bool Refresh(bool forceRefresh)
	{
		if(QuadGeneratorResult.Refresh(this, ref LastResult, Info) || forceRefresh) {
			if(QuadMesh == null) {
				QuadMesh = new Mesh();
				QuadMesh.name = "QuadMesh (Generated)";
			}
			
			float dim = Mathf.Sqrt(Mathf.Abs(Info.Length)) * 0.5f;
			
			Vector3[] verts = new Vector3[] {
				new Vector3(-dim, -dim, 0),
				new Vector3(-dim, dim, 0),
				new Vector3(dim, dim, 0),
				new Vector3(dim, -dim, 0)
			};
			
			if(Info.Length < 0) {
				Array.Reverse(verts);
			}
			
			Vector2[] uv = new Vector2[] {
				new Vector2(0, 0),
				new Vector2(1, 0),
				new Vector2(1, 1),
				new Vector2(0, 1)
			};
			
			int[] tris = new int[] {
				0, 1, 2, 2, 3, 0
			};
			
			QuadMesh.vertices = verts;
			QuadMesh.triangles = tris;
			QuadMesh.uv = uv;
			QuadMesh.RecalculateBounds();
			QuadMesh.RecalculateNormals();
			
			GetComponent<MeshFilter>().sharedMesh = QuadMesh;
			
			Debug.LogWarning("Refreshing QuadGenerator mesh in " + gameObject.name);

			return true;
		}

		return false;
	}
}
