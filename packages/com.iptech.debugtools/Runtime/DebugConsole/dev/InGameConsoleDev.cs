#if !(UNITY_EDITOR || DEVELOPMENT_BUILD || QA_BUILD)
#define CONTAINERDEBUGSERVICE_DISABLED
#endif

#if !CONTAINERDEBUGSERVICE_DISABLED

using UnityEngine;
using System.Collections;
using IPTech.DebugConsoleService.InGameConsole;
using IPTech.DebugConsoleService;
using System;
using UnityEngine.UIElements;

public class InGameConsoleDev : MonoBehaviour {
    public GameObject inGameConsoleView;
    public UIToolkitInGameConsole uIToolkitInGameConsole;

    public enum EUseType {
        GUI,
        Toolkit,
        Default
    }

    public EUseType typeToUse;

    DebugConsoleService debugConsoleService;

    InGameDebugConsole service;

	// Use this for initialization
	void Start () {
        debugConsoleService = new DebugConsoleService();
        

        if(typeToUse == EUseType.GUI) {
            service = new InGameDebugConsole(debugConsoleService);
            service.RegisterDebugPanel("DebugPanel", "TestPanel", DebugPanelFactory);

            SetupMockData();

            uIToolkitInGameConsole.gameObject.SetActive(false);
            inGameConsoleView.SetActive(true);
            var view = inGameConsoleView.GetComponentInChildren<IInGameDebugConsoleView>(true);
            service.SetInGameConsoleView(view);
            inGameConsoleView.GetComponent<InGameDebugConsoleView>().RequestUpdateButtons();
        } else if(typeToUse == EUseType.Toolkit) {
            service = new InGameDebugConsole(debugConsoleService);
            service.RegisterDebugPanel("DebugPanel", "TestPanel", DebugPanelFactory);

            SetupMockData();

            uIToolkitInGameConsole.gameObject.SetActive(true);
            inGameConsoleView.SetActive(false);
            service.SetInGameConsoleView(uIToolkitInGameConsole);
        } else {
            uIToolkitInGameConsole.gameObject.SetActive(false);
            inGameConsoleView.SetActive(false);
            service = InGameDebugConsole.CreateDefault(debugConsoleService);

            service.RegisterDebugPanel("DebugPanel", "TestPanel", DebugPanelFactory);

            SetupMockData();
        }
    }

    private VisualElement DebugPanelFactory() {
        var root = new VisualElement();
        var b = new Button();
        b.text = "my button";
        root.Add(b);

        var s = new Slider();
        root.Add(s);
        return root;
    }

    private void SetupMockData() {
        this.debugConsoleService.RegisterCommand("echo", CommandCallback, "Debug", "Debug command that just logs the argument passed to it.");
        for(int i=0;i<10;i++) {
            for(int j=0;j<20;j++) {
                this.debugConsoleService.RegisterAlias($"Action {i}{j}", $"echo {i}.{j}", $"Action {j}", $"Category {i}", null);
            }
        }
    }

    private void CommandCallback(string[] args, Action<string> result) {
        result("finished action " + args[1]);
    }

    public void UIButtonClicked() {
        service.Notify("UIButton Clicked");
    }
}
#endif
