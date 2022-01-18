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

public class QuadGeneratorResult : Comparitor<QuadGeneratorInfo>
{

	[SerializeField]
	private QuadGeneratorInfo Result;

	public override bool UpdateComparitor(QuadGeneratorInfo info)
	{
		if(!info.Equals(Result)) {
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
public class QuadGeneratorInfo
{
	public float Length = 1;
	
	public bool Equals(QuadGeneratorInfo info)
	{
		if(info == null)
			return false;
		
		return info.Length == Length;
	}
	
	public QuadGeneratorInfo Clone()
	{
		return (QuadGeneratorInfo)this.MemberwiseClone();
	}
}
