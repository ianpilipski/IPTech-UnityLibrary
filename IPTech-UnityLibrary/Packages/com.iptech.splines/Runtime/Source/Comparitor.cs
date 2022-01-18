using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPTech.Splines
{
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
	using System.Linq;

	public class Comparitor<T> : ScriptableObject
	{

		public UnityEngine.Object Parent = null;

		/// <summary>
		/// Updates the information contained in this data structure, returning <c>true</c> if it has changed as
		/// a result of calling this method. Implementing subclasses should override this method
		/// </summary>
		/// <returns><c>true</c> if the information has changed, <c>false</c> otherwise.</returns>
		/// <param name="info">Info.</param>
		public virtual bool UpdateComparitor(T info) {
			return false;
		}

		/// <summary>
		/// Updates lastResult (creating and assigning it should it be null or should it belong to some
		/// other parent) using the information contained in newInfo, returning true if this information
		/// has changed as a result of calling this method.
		/// </summary>
		/// <param name="parent">The Unity Object (usually a MonoBehaviour) that 'owns' this result. GeneratorResults
		/// should never be shared between components, even in the case of Prefabs or duplicated GameObjects; this value
		/// ensures the same GeneratorResult is never used by two parents by instantiating a new result should the
		/// given one belong to some other parent.</param>
		/// <param name="lastResult">A reference to the GeneratorResult that should be updated, or where a new
		/// GeneratorResult may be assigned</param>
		/// <param name="newInfo">The cyrrebt information pertaining to this GeneratorResult</param>
		/// <typeparam name="C">The Type that inherits from GeneratorResult, and which is responsible
		/// for overriding the 'UpdateResult' method</typeparam>
		public static bool Refresh<C>(UnityEngine.Object parent, ref C lastResult, T newInfo) where C : Comparitor<T> {
			if (lastResult == null || lastResult.Parent != parent) {
				lastResult = ScriptableObject.CreateInstance<C>();
				lastResult.Parent = parent;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(lastResult);
#endif
			}

			return lastResult.UpdateComparitor(newInfo);
		}

		/// <summary>
		/// Destroys the specified GeneratorResult to prevent leaks.
		/// </summary>
		public static void CleanUp<C>(ref C result) where C : Comparitor<T> {
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) {
				ScriptableObject.DestroyImmediate(result);
				result = null;
				return;
			}
#endif

			ScriptableObject.Destroy(result);
			result = null;
		}
	}
}
