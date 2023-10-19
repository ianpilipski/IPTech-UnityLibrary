#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System;
using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using NSubstitute;
using IPTech.DebugConsoleService.Api;
using System.Net;

namespace IPTech.DebugConsoleService.TelnetConsole
{
	[TestFixture]
	public class TestTelnetServer
	{
		[Test]
		[TestCase("127.0.0.1", 2323, 0, TestName="127.0.0.1:2323")]
		[TestCase("127.0.0.1", 2323, 2048, TestName="127.0.0.1:2323 dataSize=2048")]
		[TestCase("192.168.0.1", 2324, 0, TestName="192.168.0.1:2323")]
		[TestCase("192.168.0.1", 2324, 2048, TestName="192.168.0.1:2323 dataSize=2048")]
		public void Constructor_WithArgs_ConstructsObject(string ipString, int port, int dataSize) {
			IPAddress ip = IPAddress.Parse(ipString);
			bool constructed = false;
			if(dataSize == 0) {
				using(TelnetServer ts = new TelnetServer(ip, port)) {
					constructed = ts != null;
				}
			} else {
				using(TelnetServer ts = new TelnetServer(ip, port, dataSize)) {
					constructed = ts != null;
				}
			}
			Assert.NotNull(constructed);
		}

		[Test]
		public void Start_Begins_Listening() {
			using(TelnetServer ts = new TelnetServer(IPAddress.Any, 2323)) {
				ts.start();
				Assert.IsTrue(ts.isListening);
			}
		}

		[Test]
		public void Start_Called_Multiple_Times_In_A_Row_Does_Not_Throw() {
			using(TelnetServer ts = new TelnetServer(IPAddress.Any, 2323)) {
				ts.start();
				ts.start();
				ts.start();
			}
		}

		[Test]
		public void Stop_Stops_Listening() {
			using(TelnetServer ts = new TelnetServer(IPAddress.Any, 2323)) {
				ts.start();
				Assert.IsTrue(ts.isListening);
				ts.stop();
				Assert.IsTrue(ts.isListening);
			}
		}
	
		[Test]
		public void IsListening_Is_False_Until_Start_Is_Called() {
			using(TelnetServer ts = new TelnetServer(IPAddress.Any, 2323)) {
				Assert.IsFalse(ts.isListening);
			}
		}

		[Test]
		public void Stop_Called_Multiple_Times_In_A_Row_Does_Not_Throw() {
			using(TelnetServer ts = new TelnetServer(IPAddress.Any, 2323)) {
				ts.stop();
				ts.stop();
				ts.stop();
			}
		}

		[Test]
		public void Dispose_Closes_And_Frees_Up_Socket() {
			using(TelnetServer ts = new TelnetServer(IPAddress.Any, 2323)) {
				ts.start();
				ts.Dispose();
				using(TelnetServer ts2 = new TelnetServer(IPAddress.Any, 2323)) {
					ts2.start();
				}
			}
		}
	}
}
#endif


