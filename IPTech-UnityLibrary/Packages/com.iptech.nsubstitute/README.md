# IPTech-NSubstitute
NSubstute for use as dependency in unity packages
## How to use
Add the following to your Packages/manifest.json file in your Unity project root. You may need to close and re-open unity for it to detect the change. After this should see the IPTech-NSubstitute package in your package editor. Now you can write code that reference NSubstitute without installing all of the UnityTestTools package.
```json
{
  "scopedRegistries": [
    {
      "name": "IPTech",
      "url": "https://registry.npmjs.com",
      "scopes": [
        "com.iptech"
      ]
    }
  ],
  "dependencies": {
    "com.iptech.nsubstitute": "1.0.2"
  }
}
```
