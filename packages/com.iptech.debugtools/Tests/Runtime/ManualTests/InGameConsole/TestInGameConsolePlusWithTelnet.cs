#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED
using UnityEngine;
using System.Collections;
using IPTech.DebugConsoleService;
using IPTech.DebugConsoleService.InGameConsole;
using System;
using IPTech.DebugConsoleService.TelnetConsole;
using System.Collections.Generic;

public class TestInGameConsolePlusWithTelnet : MonoBehaviour {

	DebugConsoleService dcs;
	IList<object> holdRefs;
	
	// Use this for initialization
	void Start () {
		dcs = new DebugConsoleService();

		dcs.RegisterCommand("myCommand", MyCommand, "test");
		dcs.RegisterAlias("myCommand.doit", "myCommand doit", "Do it!", "test");

        InGameDebugConsole.CreateDefault(dcs);

		holdRefs = new List<object>();
		holdRefs.Add(new DebugConsoleTelnetService(dcs));
	}
	
	private void MyCommand(string[] args, Action<string> result) {
        if(args.Length>1) {
            result("did my thing!");
            return;
        }
		result("did my command\nwith a new line!");
	}
}
#endif
