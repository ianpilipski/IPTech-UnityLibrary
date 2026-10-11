#if DEBUG
#define ASSERTIONSON
#endif

using System;
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