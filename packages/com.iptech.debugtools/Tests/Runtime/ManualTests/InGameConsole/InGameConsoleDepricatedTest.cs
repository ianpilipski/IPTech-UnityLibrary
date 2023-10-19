#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using System.Collections;
using IPTech.DebugConsoleService;
using IPTechShared;
using System;

public class InGameConsoleDepricatedTest : MonoBehaviour {

	public bool CallbackSucceeded;

	private DebugConsoleService dcs;

	// Use this for initialization
	void Start () {
		this.dcs = new DebugConsoleService();
		DebugConsole.SetDebugConsoleService(this.dcs);

		this.dcs.RegisterCommand("test", test, null);
	}

	private void test(string[] args, Action<string> result) {
		CallbackSucceeded = true;
		result("test worked");
	}
}

#endif
