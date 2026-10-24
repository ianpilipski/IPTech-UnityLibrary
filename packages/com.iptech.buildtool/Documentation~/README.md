# IPTech.BuildTool

## Setting the application version

Add the `SetApplicationVersion` processor to a player build configuration's
`BuildProcessors` list and set `ApplicationVersion` to the desired version
(for example, `1.2.3`). New processors default to the current Player Settings
version.

The processor sets Unity's Player Settings **Version** (`PlayerSettings.bundleVersion`)
for the build and restores the previous value afterward. It does not change the
Android version code or iOS build number.
