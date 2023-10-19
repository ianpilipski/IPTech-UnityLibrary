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
	public class TestDebugConsoleTelnetService
	{
		IDebugConsoleService mockDebugConsoleService;
		ITelnetServerFactory mockFactory;
		ITelnetServer mockServer;

		[SetUp]
		public void SetUp() {
			mockDebugConsoleService = Substitute.For<IDebugConsoleService>();
			mockFactory = Substitute.For<ITelnetServerFactory>();
			mockServer = Substitute.For<ITelnetServer>();

			mockFactory.Create(Arg.Any<IPAddress>(), Arg.Any<int>(), Arg.Any<int>()).Returns(mockServer);
		}

		[Test]
		public void Constructor_Creates_Class() {
			using(DebugConsoleTelnetService dcts = new DebugConsoleTelnetService(mockDebugConsoleService)) {
				Assert.NotNull(dcts);
			}
		}

		[Test]
		public void Constructor_WithFactory_Calls_Create_On_Factory() {
			using(new DebugConsoleTelnetService(mockDebugConsoleService, 2323, mockFactory)) {
				mockFactory.ReceivedWithAnyArgs(1).Create(null, 0, 0);
			}
		}

		[Test]
		public void Constructor_WithPort_Calls_Create_On_Factory_With_PortNumber() {
			using(new DebugConsoleTelnetService(mockDebugConsoleService, 1234, mockFactory)) {
				mockFactory.Received(1).Create(Arg.Any<IPAddress>(), 1234, Arg.Any<int>());
			}
		}

		[Test]
		public void Constructor_Calls_Start_On_TelnetServer() {
			using(new DebugConsoleTelnetService(mockDebugConsoleService, 2323, mockFactory)) {
				mockServer.Received(1).start();
			}
		}

		[Test]
		public void Start_Calls_LogMessage_On_DebugConsoleService() {
			using(DebugConsoleTelnetService dcts = new DebugConsoleTelnetService(mockDebugConsoleService, 2323, mockFactory)) {
				dcts.Start();	
				mockDebugConsoleService.Received(2).LogMessage(Arg.Any<string>(), Arg.Is<string>("telnet"));
			}
		}


		[Test]
		public void Start_Calls_Start_On_TelnetServer() {
			using(DebugConsoleTelnetService dcts = new DebugConsoleTelnetService(mockDebugConsoleService, 2323, mockFactory)) {
				dcts.Start();
				mockServer.Received(2).start();
			}
		}

		[Test]
		public void Stop_Calls_Stop_On_TelnetServer() {
			using(DebugConsoleTelnetService dcts = new DebugConsoleTelnetService(mockDebugConsoleService, 2323, mockFactory)) {
				dcts.Stop();
				mockServer.Received(1).stop();
			}
		}

		[Test]
		public void Start_Silently_Fails_If_TelnetServer_Throws() {
			mockServer.When( x => x.start() ).Do( _ => { throw new Exception(); } );
			using(DebugConsoleTelnetService dcts = new DebugConsoleTelnetService(mockDebugConsoleService, 2323, mockFactory)) {
				dcts.Start();
			}
		}
	}
}

#endif
