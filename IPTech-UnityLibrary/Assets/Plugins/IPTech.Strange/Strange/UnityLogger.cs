using UnityEngine;
using IPTech.Strange.Api;

namespace IPTech.Strange
{
	public class UnityLogger : ILog
	{
		public void Log(string message) {
			Debug.Log(message);
		}
	}
}
