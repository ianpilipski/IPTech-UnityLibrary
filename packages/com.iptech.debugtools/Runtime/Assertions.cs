#if DEVELOPMENT_BUILD
#define ASSERTIONSON
#endif

using UnityEngine;
using System;
using System.Collections;
using System.Diagnostics;

namespace IPTech
{
	public class Debugging {
		[Conditional("ASSERTIONSON")]
		public static void Assert(bool condition) {
			if(!condition) {
				throw new Exception();
			}
		}
	}
}