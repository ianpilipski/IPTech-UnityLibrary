using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool.Processors {
    [Tooltip("Sets the application version based on the specified command line argument.")]
    public class SetApplicationVersion : BuildProcessor {
        public string CommandLineArgument = "-appVersion";

        string origValue;

        public override void ModifyProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) {
            origValue = PlayerSettings.bundleVersion;
            if(args.TryGetValue(CommandLineArgument, out string val)) {
                Debug.Log($"Found command line argument {CommandLineArgument}. Setting application version to {val}");
                PlayerSettings.bundleVersion = val;
            }
        }

        public override void RestoreProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) {
            PlayerSettings.bundleVersion = origValue;
        }
    }
}
