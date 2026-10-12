# IPTech.EditorExtensions

## Project Cleaner

Targets and is validated with **Unity 6000.6.5f1**. The package's minimum
Unity version is now 6000.6.5f1; older editor support is not retained.

Open **Window > IPTech > Project > Cleaner** (or **IPTech > Project > Cleaner ...**).
The UI Toolkit window does not scan automatically.
Its main/preview layout is loaded from `ProjectCleanerEditor.uxml` beside
`ProjectCleanerEditor.cs`; presentation is defined in `ProjectCleanerEditor.uss`.
The code is split so the UI can be tested without opening a window:

- `ProjectCleanerSession` holds scan results, selection, filters, sorting, and
  deletion bookkeeping (no UI).
- `ProjectCleanerView` loads the UXML/USS into any root `VisualElement`, queries
  named controls, and binds them to the session. `ProjectCleanerPreview` owns the
  preview list, messages, and page transitions.
- `IProjectCleanerServices` wraps editor side effects (dialogs, file pickers,
  scans, deletion); `EditorProjectCleanerServices` is the real implementation.
- `ProjectCleanerEditor` is a thin `EditorWindow` that creates a view for its
  `rootVisualElement` and disposes it when the window closes.

### Workflow

1. **Scan** while the editor is idle and outside Play Mode. Progress is staged;
   Cancel stops between work units. Incomplete scans cannot authorize deletion.
2. **Review** candidates, scripts requiring review, protected assets, and referenced
   assets using Show, Scope, search, and Sort. Select a row to inspect incoming
   references, outgoing dependencies, protection reasons, and detector coverage.
   In narrow panes, scroll the results horizontally to reach the remaining columns.
   Click any column header to sort; click again to reverse direction. Size and
   Referrers sort numerically, and Pick sorts by checked state. The Sort dropdown
   stays synchronized with the active header sort.
   Ping/Open navigates to the corresponding project asset. The Project window
   context menu **IPTech > Find References** opens the same analysis.
3. **Keep** assets or folders that must survive cleanup. Kept folders protect
   descendants and their dependencies. Manage all Keep decisions in the details
   pane, including entries that are not cleanup candidates. Remove a parent Keep
   if a child remains kept after removing its own exact Keep.
4. **Select** individual eligible rows or all visible eligible rows. Hidden rows
   are not added by Select Visible. Scripts require explicit review acknowledgement.
5. **Preview selected** shows selected paths, counts, sizes, and the backup location,
   even when deletion is blocked. The main UI slides left
   and the preview slides in from the right. **Cancel** slides back to the main UI
   without clearing your selection or changing the splitter position. The hidden
   main page retains its layout during preview. Project changes disable deletion
   in the preview.
6. **Back up and delete** rechecks current dependencies and settings, then verifies
   files and `.meta` files before deleting. Unsaved assets block deletion, not preview.
   Any failed backup blocks the whole batch. Unity deletion is **not Undo**.
   Disabling backup is an advanced opt-out with an explicit destructive confirmation.
7. Inspect per-item results, then scan again. Successfully deleted assets are removed
   from the results immediately; failed deletions remain visible. Project mutations
   invalidate the remaining scan results.

Revalidation refreshes imports and provides cancellable progress. Adding a new
script to a selection clears its previous review acknowledgement.

The default backup location is `<project>/Backups/ProjectCleaner`. Each operation
creates a unique session containing asset-relative files, metadata, and
`manifest.json`. Backups must be outside Assets and every registered package root.
Empty-folder backups include descendant folder metadata.

Use **Restore backup...** to select a session folder. Restore verifies content,
project identity, package location, and GUIDs. Its preview blocks collisions;
existing assets are never overwritten. Restore package assets only after explicitly
enabling editable-package cleanup.

### Scope and protections

- Dependency sources include Assets and registered packages, even read-only packages.
  Serialized ProjectSettings GUIDs protect graphics/quality pipelines, cursors,
  splash assets, and other settings-only references.
- Deletion defaults to Assets. Local/embedded package deletion is explicit opt-in;
  registry/Git/cache packages and root folders cannot be deleted.
- Resources, StreamingAssets, Editor, Editor Default Resources, Tests, ThirdParty,
  data/configuration files, assembly definitions, assemblies, shader includes,
  sprite atlases, UXML/USS, build-list scenes, player icons, editor/code entry points,
  registered Addressables, Unity Build Profiles, and their dependencies are
  protected conservatively.
- Additional protected folders are stored as asset paths in the settings foldout.
- Keep and protection settings are project-local at
  `ProjectSettings/IPTech/ProjectCleanerSettings.asset`. Existing kept paths
  migrate to GUIDs when resolvable, so Keep survives moves. Unresolved path entries
  are retained for management rather than discarded.
- Symbolic-link cleanup paths are rejected.

### Detection and limitations

One direct forward/reverse graph is built from `AssetDatabase.GetDependencies`,
with self-edges removed and evidence deduplicated. Supplementary detectors inspect
serialized Addressables `m_AssetGUID` fields and TerrainData links in text assets.
Binary serialized assets inspect subassets, prefab components, and preview-scene
components, including hidden serialized properties. No serialization settings or
user scenes are changed. Missing components or detector failures block cleanup
rather than silently returning negative evidence.

Addressables registration detection uses its optional editor API through reflection,
including folder entries. The package does **not** depend on Dependencies Hunter
or require Addressables to be installed.

C# declaration/identifier analysis is **lexical evidence, not semantic liveness**.
It includes code in packages and tests, supports multiple declared types and
filename/type mismatches, and ignores ordinary comments and strings. Editor entry
point detection is conservative. All unreferenced scripts remain review-required.
Reflection, generated paths, custom runtime registries, preprocessor behavior,
remote catalogs, and string-based loading can still evade detection.

Before deletion, save or discard unsaved scenes and native assets. The cleaner
reports blocking asset paths rather than saving your work automatically. Imported
shader/compiler dirty flags are not mistaken for unsaved native assets.

Zero incoming references means **no detected direct referrers**, not proof of
runtime non-use. Cycles and dependencies of otherwise unused assets are not
automatically identified as dead. Atlas references remain evidence, not a special
reason to erase reference counts. File sizes describe source files, not final
player-build savings.

**Export CSV** writes the current filtered results with path, GUID, type, source
bytes, status, reason, and referrers. Export is a report, not a deletion manifest.

Dependency-detection techniques are adapted from Dependencies Hunter 1.3.0;
see [MIT attribution](../Editor/Source/Tools/ProjectCleaner/ThirdPartyNotices.txt).

### Tests

Enable this package's tests in the consumer project's package manifest:

```json
"testables": ["com.iptech.editorextensions"]
```

Run the EditMode fixtures `IPTech.EditorTools.Tests.ProjectCleaner*` in Unity's
Test Runner. `ProjectCleanerSessionTests` cover the model directly;
`ProjectCleanerViewTests` build the real UXML in an off-screen panel with the
UI Test Framework (`com.unity.ui.test-framework`) and a fake
`IProjectCleanerServices`, so no editor windows are opened. The test assembly
therefore references `Unity.UI.TestFramework.Runtime`/`.Editor`; the consumer
project must include that package. Tests use unique Assets/Temp fixture paths and remove their own
assets/backups. Optional Addressables registration tests add and remove only
temporary in-memory entries, preserving existing dirty flags and never saving
the user's groups/settings.

Tests access private scanner behavior through its internal, editor-only nested
`SpyImp` via `scanner.Spy`, not reflection. Reflection in the optional Addressables
tests is limited to third-party APIs so Addressables remains an optional dependency.
## Project Cleaner settings and tests

Project Cleaner takes an `IProjectCleanerSettings` dependency. The editor window
injects `ProjectCleanerSettings.instance` into `ProjectCleanerSession`; the view,
scanner, reclassification, deletion revalidation, and restore use that same
dependency. Public scan and cleanup helpers require an explicit settings argument.
The window rebinds settings after domain reload while retaining serialized UI choices.

Tests use NSubstitute settings rather than reading or writing the project's
settings asset. NSubstitute is referenced only by the test assembly and must be
available in the project running the tests; it is not a package dependency.
