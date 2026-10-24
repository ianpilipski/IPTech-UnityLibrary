using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace IPTech.BuildTool.Processors {
    public class SetProductName : BuildProcessor {
        public string ProductName;

        string orignalValue;

        private void OnEnable() {
            if(string.IsNullOrEmpty(ProductName)) {
                ProductName = Application.productName;
            }
        }

        public override void ModifyProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) {
            orignalValue = PlayerSettings.productName;
            PlayerSettings.productName = ProductName;
        }

        public override void RestoreProject(BuildTarget buildTarget, IReadOnlyDictionary<string,string> args) {
            PlayerSettings.productName = orignalValue;
        }
    }
}
