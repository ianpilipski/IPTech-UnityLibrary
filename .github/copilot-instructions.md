# Repository guidance

## Build and test

- This is a Unity Package Manager (UPM) package monorepo. The root `package.json` has no build, test, or lint scripts; the package projects and CI define the supported validation paths.
- Use `unity-cli` for Unity compilation and test operations; do not use the VS Code build task for Unity compilation. To compile scripts, target the library consumer project:

  ```sh
  unity recompile --project-path unity/iptech-unitylibrary
  ```

  `unity recompile` requires a running Editor with the Pipeline package. Use the latest installed `unity` CLI; if no Editor is reachable, compilation is unverified rather than successful.

- Build the configured player through the package's `IPTech.BuildTool.Builder.Build` entry point. The build config selects the output and other build settings:

  ```sh
  unity build unity/iptech-unitylibrary --target Android \
    --execute-method IPTech.BuildTool.Builder.Build --args "-buildConfig DebugAndroid" \
    --allow-dirty-build
  unity build unity/iptech-unitylibrary --target iOS \
    --execute-method IPTech.BuildTool.Builder.Build --args "-buildConfig DebugiOS" \
    --allow-dirty-build
  ```

  `--allow-dirty-build` is for intentional builds of uncommitted work; otherwise the CLI's dirty-worktree guard applies. CI also builds `unity/iptech-unityservices`. Jenkins uses the root Gradle wrapper for both projects, supplying `BUILD_NUMBER` and `UNITY_EDITOR_VERSION`; use these tasks to reproduce its exact build steps:

  ```sh
  ./gradlew buildDebugAndroid -PBUILD_NUMBER=1 -PUNITY_EDITOR_VERSION=6000.6.5f1
  ./gradlew buildDebugiOS -PBUILD_NUMBER=1 -PUNITY_EDITOR_VERSION=6000.6.5f1
  ./gradlew -b build-unityservices.gradle buildDebugAndroid -PBUILD_NUMBER=1 -PUNITY_EDITOR_VERSION=6000.6.5f1
  ./gradlew -b build-unityservices.gradle buildDebugiOS -PBUILD_NUMBER=1 -PUNITY_EDITOR_VERSION=6000.6.5f1
  ```

  Match `UNITY_EDITOR_VERSION` to the target project's `ProjectSettings/ProjectVersion.txt` and supply the actual build number.

- CI runs Unity EditMode tests in `unity/iptech-unitylibrary` and `unity/iptech-unityservices` for iOS and Android. To run one EditMode test locally, filter by its fully qualified test name:

  ```sh
  unity test unity/iptech-unitylibrary --mode EditMode \
    --filter "CoroutineRunnerTests.StartMethod.CountIsIncremented"
  ```

  Replace `--filter` with another test name as needed. Use `unity/iptech-unityservices` as the project path when testing that consumer project.

## Architecture

- Reusable packages live under `packages/com.iptech.*`; each is a UPM package with its own `package.json`, version, Unity compatibility, and package dependencies. Consumers can install packages from this Git repository using a `?path=/packages/<package>` URL.
- The Unity projects under `unity/` consume packages for integration, compilation, and tests. `unity/iptech-unitylibrary` links local packages through relative `file:` dependencies in `Packages/manifest.json`; the CI test matrix also validates `unity/iptech-unityservices`.
- Within a package, `Runtime/` contains player/runtime code and `Editor/` contains Unity Editor-only code. Assembly Definition (`.asmdef`) files establish the assembly boundaries; keep Editor-only APIs out of runtime assemblies.
- Tests are package-owned assemblies under `Tests/` and reference the package assembly they exercise. Test assemblies may need to be included in the consumer project's `testables` list in `Packages/manifest.json`; CI currently runs EditMode tests.

## Package and Unity asset conventions

- Keep the UPM package identifier in `package.json` aligned with the package directory name (`com.iptech.*`). Declare inter-package dependencies in that package's `dependencies` object and ensure the consuming project's manifest resolves the package.
- Preserve Unity `.meta` files and their GUIDs when moving or editing assets. New Unity assets need matching `.meta` files so existing assembly references and serialized assets remain stable.
- Keep package boundaries explicit in `.asmdef` references. Tests should reference the assembly under test and use Unity's test assemblies; do not add test-only dependencies to runtime assemblies.
- Package-facing usage and API details belong with that package's README or `Documentation~/` content.
