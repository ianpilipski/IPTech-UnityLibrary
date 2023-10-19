#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using IPTech.DebugConsoleService.Api;

namespace IPTech.DebugConsoleService
{
	[TestFixture]
	public class TestDebugConsoleService {


        [Test]
        public void LogMessage_Fires_MessageLogged_Event_With_Proper_Variables() {
            DebugConsoleService serviceUnderTest = new DebugConsoleService();
            string eventMessage = null;
            string eventCategory=null;
            serviceUnderTest.MessageLogged += (arg1, arg2) => {
                eventMessage = arg1;
                eventCategory = arg2;
            };

            serviceUnderTest.LogMessage("myMessage", "myCategory");
            Assert.AreEqual("myMessage", eventMessage);
            Assert.AreEqual("myCategory", eventCategory);
        }

		[Test]
		public void RegisterCommand_Adds_Command_To_Command_List() {
			DebugConsoleService serviceUnderTest = new DebugConsoleService();
			serviceUnderTest.RegisterCommand("myCommand" , (args, res) =>  { res("done"); }, "test");	
			Assert.IsTrue(serviceUnderTest.GetDebugCommands().Any( ci => ci.Command=="myCommand" ));
		}

		[Test]
		public void UnRegisterCommand_Removes_Command_From_Command_List() {
			DebugConsoleService serviceUnderTest = new DebugConsoleService();
			serviceUnderTest.RegisterCommand("myCommand" , (args, res) =>  { res("done"); }, null);	
			serviceUnderTest.UnregisterCommand("myCommand");
			Assert.IsFalse(serviceUnderTest.GetDebugCommands().Any( ci => ci.Command == "myCommand"));
		}

		[Test]
		public void ExecDebugCommand_Calls_The_Registered_Command() {
			DebugConsoleService serviceUnderTest = new DebugConsoleService();
			bool called = false;
			bool argsCorrect = false;
			serviceUnderTest.RegisterCommand("test", (args, res) => {
				called = true;
				argsCorrect = args[0]=="one" && args[1]=="two" && args[2]=="three";
				res("done");
			}, null);

			serviceUnderTest.ExecDebugCommand("test", new string[] { "one", "two", "three" }, null);

			Assert.IsTrue(called);
			Assert.IsTrue(argsCorrect);
		}

		[Test]
		public void ExecDebugCommand_Does_Not_Hold_Refernce_And_Fails_When_Callback_Has_Been_Deleted() {
			DebugConsoleService service = new DebugConsoleService();
			string retOne = null;
			RegisterTestCommand(service);
			service.ExecDebugCommand("testCommand", null, s => retOne=s );
			Assert.AreEqual("hello world", retOne);
			GC.Collect();
			string retVal = null;
			service.ExecDebugCommand("testCommand", null, s => retVal=s);
			Assert.AreEqual(DebugConsoleService.WEAK_REF_NO_LONGER_AVAIL, retVal);
		}

		[Test]
		public void GetDebugCommands_Returns_Sorted_List() {
			DebugConsoleService service = new DebugConsoleService(false);
			string a = "abcdefghijklmnopqrstuvwxyz";
			for(int i=a.Length-1;i>=0;i--) {
				for(int j=a.Length-1;j>=0;j--) {
					service.RegisterCommand(a[i].ToString() + a[j].ToString(), new TestClass().MyCommand, a[j].ToString(), null);
				}
			}

			int k = 0;
			IList<ICommandInfo> list = service.GetDebugCommands();
			for(int i = 0; i < a.Length; i++) {
				for(int j = 0; j < a.Length; j++) {
					Assert.AreEqual(a[i].ToString(), list[k].Category);
					Assert.AreEqual(a[j].ToString() + a[i].ToString(), list[k].Command);
					k++;
				}
			}
		}

		// Put registratioin in separate function so the stack values don't hold a reference to the object
		// This allows GC to collect the TestClass as it has no reference after leaving this function.
		private void RegisterTestCommand(DebugConsoleService service) {
			service.RegisterCommand("testCommand", new TestClass().MyCommand, null);
		}


		private class TestClass {
			public void MyCommand(string[] args, Action<string> result) {
				result("hello world");
			}
		}

	}
}

#endif
