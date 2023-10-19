#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using System;
using UnityEngine;
using System.Collections;
using IPTech.DebugConsoleService;
using System.Text;
using IPTech.DebugConsoleService.TelnetConsole;

public class TelnetConsoleTest : MonoBehaviour {

	DebugConsoleService dcs;
	DebugConsoleTelnetService dct;

	void Start() {
		dcs = new DebugConsoleService();
		dcs.RegisterCommand("myTestCommand", myTestCommand, "test");

		dct = new DebugConsoleTelnetService(dcs);
	}

	private void myTestCommand(string[] args, Action<string> result) {
		GameObject[] all = GameObject.FindObjectsOfType<GameObject>();

		StringBuilder sb = new StringBuilder();
		sb.AppendLine("** myTestCommand : All Game Objects **");
		foreach(GameObject go in all) {
			sb.AppendLine(go.name);
		}

		result(sb.ToString());
	}

	private void OnDestroy() {
		dct.Stop();
	}
}
#endif
