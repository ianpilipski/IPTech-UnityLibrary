using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace IPTech.ExternalDependencyManager
{
    public static class ScopedRegistryUtility
    {
        // High-level wrapper to easily inject a registry
        public static bool AddRegistry(string name, string url, string[] scopes)
        {
            if(string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or whitespace", nameof(name));
            if(string.IsNullOrWhiteSpace(url)) throw new ArgumentException("URL cannot be null or whitespace", nameof(url));
            if(scopes == null) throw new ArgumentNullException("Scopes cannot be null", nameof(scopes));
            if(scopes.Length == 0) throw new ArgumentException("Scopes cannot be empty", nameof(scopes));

            string manifestPath = Path.Combine(Application.dataPath, "..", "Packages/manifest.json");

            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException("Could not find manifest.json!");
            }

            var added = false;

            // 1. Read raw JSON string from the project
            string rawJson = File.ReadAllText(manifestPath);

            JObject manifest = JObject.Parse(rawJson);

            // 2. Locate or create the scopedRegistries array safely
            if (manifest["scopedRegistries"] == null)
            {
                manifest["scopedRegistries"] = new JArray();
            }

            JArray registries = (JArray)manifest["scopedRegistries"];

            // 3. Prevent duplicate injection
            var existingReg = registries.FirstOrDefault(r => r["url"]?.ToString() == url);
            if (existingReg == null)
            {
                JObject newRegistry = new JObject
                {
                    ["name"] = name,
                    ["url"] = url,
                    ["scopes"] = new JArray(scopes)
                };
                registries.Add(newRegistry);

                added = true;
            }
            else
            {
                if(existingReg["scopes"] == null)
                {
                    existingReg["scopes"] = new JArray(scopes);
                    added = true;
                }
                else
                {
                    JArray scopesArray = (JArray)existingReg["scopes"];
                    foreach(var scope in scopes)
                    {
                        if (!scopesArray.Any(s => s.ToString() == scope))
                        {
                            scopesArray.Add(scope);
                            added = true;
                        }
                    }
                }
            }

            if (added)
            {
                // 4. Serialize the modified C# data back to pretty-printed JSON
                File.WriteAllText(manifestPath, manifest.ToString(Newtonsoft.Json.Formatting.Indented));
                
                // 5. Force Unity to reload and discover the new packages instantly
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                Debug.Log($"[IPTech][ExternalDependencyManager] Successfully updated scoped registry: '{name}'");
            }
            return added;
        }
        


    }
}
