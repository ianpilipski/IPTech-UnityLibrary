#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using System.Collections;
using IPTech.DebugConsoleService.InGameConsole;
using IPTech.DebugConsoleService;
using System;

public class InGameConsoleDev : MonoBehaviour {
    public GameObject inGameConsoleView;

    DebugConsoleService debugConsoleService;

    InGameDebugConsole service;

	// Use this for initialization
	void Start () {
        debugConsoleService = new DebugConsoleService();
        service = new InGameDebugConsole(debugConsoleService);
        service.SetInGameConsoleView(inGameConsoleView);

        SetupMockData();
        inGameConsoleView.GetComponent<InGameDebugConsoleView>().RequestUpdateButtons();
	}
	
    private void SetupMockData() {
        this.debugConsoleService.RegisterCommand("echo", CommandCallback, "Debug", "Debug command that just logs the argument passed to it.");
        for(int i=0;i<10;i++) {
            for(int j=0;j<20;j++) {
                this.debugConsoleService.RegisterAlias("Action " + i.ToString() + j.ToString(), "echo " + i + "." + j, "Action " + j, "Category " + i.ToString(), null);
            }
        }
    }

    private void CommandCallback(string[] args, Action<string> result) {
        result("finished action " + args[1]);
    }
}
#endif
