#if !(DEBUG || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using System.Collections;
using IPTech.DebugConsoleService;
using System;

public class InGameConsoleDepricatedTest : MonoBehaviour {

	public bool CallbackSucceeded;

	private DebugConsoleService dcs;

	// Use this for initialization
	void Start () {
		this.dcs = new DebugConsoleService();
		this.dcs.RegisterCommand("test", test, null);
	}

	private void test(string[] args, Action<string> result) {
		CallbackSucceeded = true;
		result("test worked");
	}
}

#endif
