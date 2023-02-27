using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IPTech.BuildTool {
    public class ConfigModifierProductName : ConfigModifier {
        public string ProductName;

        string orignalValue;

        private void OnEnable() {
            if(string.IsNullOrEmpty(ProductName)) {
                ProductName = Application.productName;
            }
        }

        public override void ModifyProject() {
            orignalValue = PlayerSettings.productName;
            PlayerSettings.productName = ProductName;
        }

        public override void RestoreProject() {
            PlayerSettings.productName = orignalValue;
        }
    }
}
