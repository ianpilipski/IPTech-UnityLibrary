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
using System.Linq;
using System.Collections.Generic;

public class GenericGeneratorResult : Comparitor<GenericGeneratorInfo> {

	public GenericGeneratorInfo GenericInfo = null;

	public override bool UpdateComparitor(GenericGeneratorInfo info)
	{
		if(!info.Equals(GenericInfo)) {
			GenericInfo = info.Clone();
#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(this);
#endif

			return true;
		}

		return false;
	}
}

[System.Serializable]
public class AnimationCurveContainer {
	public AnimationCurve Curve = null;

	public AnimationCurveContainer(AnimationCurve curve) {
		this.Curve = curve;
	}

	public override bool Equals(object container) {
		if(container == null || container.GetType() != typeof(AnimationCurveContainer)) return false;

		AnimationCurveContainer other = (AnimationCurveContainer)container;


		if((other.Curve == null) != (Curve == null)) return false;

		if(Curve == null) return true;

		if(!Enumerable.SequenceEqual(Curve.keys, other.Curve.keys)) {
			return false;
		}

		return true;
	}

	public override int GetHashCode()
	{
		return Curve != null ? Curve.GetHashCode() : 0;
	}
}

[System.Serializable]
public class GenericGeneratorInfo {
	[SerializeField]
	private List<float> Floats = new List<float>();
	
	[SerializeField]
	private List<int> Integers = new List<int>();
	
	[SerializeField]
	private List<AnimationCurveContainer> Curves = new List<AnimationCurveContainer>();
	
	public void AddFloat(float f) {
		Floats.Add(f);
	}
	
	public void AddInteger(int f) {
		Integers.Add(f);
	}
	
	public void AddAnimationCurve(AnimationCurve c) {
		AnimationCurve copy = new AnimationCurve(c.keys);
		Curves.Add(new AnimationCurveContainer(copy));
	}
	
	public bool Equals(GenericGeneratorInfo info) {
		if(info == null) return false;

		if(!Enumerable.SequenceEqual(Floats, info.Floats)) {
			return false;
		}
		
		if(!Enumerable.SequenceEqual(Integers, info.Integers)) {
			return false;
		}

		if(!Enumerable.SequenceEqual(Curves, info.Curves)) {
			return false;
		}

		return true;
	}

	public GenericGeneratorInfo Clone()
	{
		GenericGeneratorInfo info = new GenericGeneratorInfo();

		foreach(float f in Floats) {
			info.AddFloat(f);
		}

		foreach(int i in Integers) {
			info.AddInteger(i);
		}

		foreach(AnimationCurveContainer c in Curves) {
			info.AddAnimationCurve(c.Curve);
		}

		return info;
	}
}