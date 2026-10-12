---
uid: trigger-and-update-ui
---

# Trigger and update UI

The [`PanelSimulator`](xref:UnityEngine.UIElements.TestFramework.PanelSimulator) API provides methods that you can use to ensure the UI is up-to-date and to manage the simulated time during test execution.

## Update the UI

To ensure your UI reflects the latest state, call the [`simulate.FrameUpdate()`](xref:UnityEngine.UIElements.TestFramework.PanelSimulator.FrameUpdate) method. This executes the UI Toolkit update loop, which processes data bindings, layout, and other UI changes.

`simulate.FrameUpdate()` only executes the UI Toolkit update loop. This lets you test UI in isolation from the Editor update loop and the game loop, which is faster and more deterministic than using `yield return`.

For example, when testing data binding updates, you must call `simulate.FrameUpdate()` before validating the UI state:

[!code-cs[](../../Samples~/DocCodeSamples.Tests/Runtime/FrameUpdateExample.cs#FrameUpdate_UpdatesUI)]

For UI that depends on the Editor update loop or the game loop, you can still use `yield return` or other [Unity Test Framework](xref:um-test-framework-intro) coroutine patterns. This isn't the intended workflow of the UI Test Framework, and results might be less deterministic. These coroutines don't advance the simulated time tracked by the panel (refer to [Manage time progression](#manage-time-progression)), so you need to manage that yourself using [`IncrementCurrentTime`](xref:UnityEngine.UIElements.TestFramework.PanelSimulator.IncrementCurrentTime(System.Double)).

> [!NOTE]
> Each PanelSimulator type might implement `FrameUpdate` differently. Refer to the specific Scripting API documentation for details about available options.

## Manage time progression

By default, calling `simulate.FrameUpdate()` or any [UI interaction simulation method](xref:UnityEngine.UIElements.TestFramework.PanelSimulator#methods) advances the simulated time tracked by the panel. Each call to `simulate.FrameUpdate()` increments the time by 200 ms.

You can customize this behavior so that time doesn't advance when performing UI updates or interactions. This is useful for testing scheduled actions or animations that depend on time progression.

The following example demonstrates how to control the simulated time to verify the execution of a scheduled item:

[!code-cs[](../../Samples~/DocCodeSamples.Tests/Runtime/FrameUpdateExample.cs#FrameUpdate_ToControlTime)]

## Additional resources

- [PanelSimulator types](xref:introduction-to-test-fixtures#simulator-types)
- [Simulate UI interactions](xref:simulate-ui-interaction-landing)
- [Create multi-window tests](xref:create-multi-window-tests)
