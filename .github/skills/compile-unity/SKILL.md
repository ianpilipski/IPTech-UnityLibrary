---
name: compile-unity
description: Compile Unity C# scripts by forcing an editor asset refresh, then inspect the Unity console for compile errors and report the result.
---

This skill is for the Unity project in `unity/iptech-unitylibrary`.

## Workflow

1. Confirm the Unity project is available in the current workspace.
2. Clear the Unity editor console first using the console read/clear tool.
3. Use the Unity MCP integration to execute a Unity editor command that forces a synchronous asset refresh and triggers script compilation.
   - Example command: `AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);`
   - Use the Unity MCP tool `mcp_unity-mcp-ser_Unity_RunCommand` to run that editor command.
4. After compilation starts, use the Unity MCP console tool `mcp_unity-mcp-ser_Unity_GetConsoleLogs` to capture compile errors and warnings from the editor console.
5. If there are compile issues, list the first relevant errors and warn the user.
6. If there are no compile issues, confirm that script compilation succeeded.

## When to use this skill

- The user asks to compile or check Unity scripts.
- The user wants to verify whether Unity script compilation is currently clean.
- The user wants a concise result from the Unity console after a compile refresh.

## Steps

### 1. Trigger Unity script compilation

- Use the available Unity MCP integration to run an editor command.
- The command should force the editor to refresh assets and compile scripts immediately.
- If the editor is not already running, ask the user to open the Unity project first.

### 2. Read the Unity console

- Use the Unity console log tool to collect logs after the refresh.
- Extract compile errors and warnings.
- Include the first few lines of each error to help the user identify the problem.

### 3. Report results

- If `errorCount > 0`, respond with:
  - "Unity compile failed"
  - the number of errors and warnings
  - a short summary of the first compile errors
- If `errorCount == 0`, respond with:
  - "Unity compile succeeded"
  - the number of warnings, if any

## Notes

- Do not proceed if the project is not the current Unity workspace root.
- Keep the response focused on compile diagnostics only.
- If the console is empty after a refresh, say that the editor reported no compile issues.
