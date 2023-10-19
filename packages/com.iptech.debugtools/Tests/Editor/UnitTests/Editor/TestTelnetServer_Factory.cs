#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Net;

namespace IPTech.DebugConsoleService.TelnetConsole
{
	public class TestTelnetServer_Factory {

		[Test]
		public void Create_Returns_TelnetServer_Object()
		{
			using(ITelnetServer ts = new TelnetServer.Factory().Create(IPAddress.Any, 2323)) {
				Assert.IsNotNull(ts);
				Assert.AreSame(typeof(TelnetServer), ts.GetType());
			}
		}
	}
}

#endif

