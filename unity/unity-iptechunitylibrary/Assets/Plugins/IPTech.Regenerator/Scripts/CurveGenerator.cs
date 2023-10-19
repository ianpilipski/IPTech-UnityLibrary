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
using System.Collections.Generic;

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class CurveGenerator : MonoBehaviour, RefreshTarget
{
	public AnimationCurve Curve = AnimationCurve.Linear(0, 0, 1, 1);
	public int NumSteps = 5;

	[HideInInspector]
	private Mesh Mesh;
	
	[SerializeField, HideInInspector]
	private GenericGeneratorResult LastResult = null;

	public AnimationCurveContainer Container;
	public List<AnimationCurveContainer> containers;


	private GenericGeneratorInfo Info {
		get {
			GenericGeneratorInfo result = new GenericGeneratorInfo();
			result.AddAnimationCurve(Curve);
			result.AddInteger(NumSteps);

			return result;
		}
	}

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
		GenericGeneratorResult.CleanUp(ref LastResult);
	}
	
	public bool Refresh(bool forceRefresh)
	{
		if(GenericGeneratorResult.Refresh(this, ref LastResult, Info) || forceRefresh) {
			if(Mesh == null) {
				Mesh = new Mesh();
				Mesh.name = "CurveMesh (Generated)";
			}

			List<Vector3> verts = new List<Vector3>();
			List<Vector2> uv = new List<Vector2>();
			List<int> tris = new List<int>();

			if(Curve.length >= 2) {
				float minT = Curve[0].time;
				float maxT = Curve[Curve.length - 1].time;

				int steps = Mathf.Max(NumSteps, 1) + 1;

				for(int i = 0; i < steps; i++) {
					float t = minT + (i / (float)(steps - 1)) * (maxT - minT);

					float height = Mathf.Max(0, Curve.Evaluate(t));

					verts.Add(new Vector3(t, height, 0));
					verts.Add(new Vector3(t, 0, 0));

					uv.Add(new Vector2(t, height));
					uv.Add(new Vector2(t, 0));

					int vertCount = verts.Count;
					if(vertCount > 2) {
						tris.Add(vertCount - 2);
						tris.Add(vertCount - 3);
						tris.Add(vertCount - 4);
						tris.Add(vertCount - 2);
						tris.Add(vertCount - 1);
						tris.Add(vertCount - 3);
					}
				}

			}

			Mesh.Clear();
			Mesh.vertices = verts.ToArray();
			Mesh.triangles = tris.ToArray();
			Mesh.uv = uv.ToArray();
			Mesh.RecalculateBounds();
			Mesh.RecalculateNormals();
			
			GetComponent<MeshFilter>().sharedMesh = Mesh;
			
			Debug.LogWarning("Refreshing CurveGenerator mesh in " + gameObject.name);

			return true;
		}

		return false;
	}
}
