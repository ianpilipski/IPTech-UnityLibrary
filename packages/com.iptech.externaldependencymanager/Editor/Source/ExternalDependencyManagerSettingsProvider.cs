using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.ExternalDependencyManager
{
    public class ExternalDependencyManagerSettingsProvider : SettingsProvider
    {
        private Toggle _enableScanToggle;

        public ExternalDependencyManagerSettingsProvider() : base("Project/IPTech/ExternalDependencyManager", SettingsScope.Project, GetSearchKeywords())
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new ExternalDependencyManagerSettingsProvider();
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            rootElement.Clear();
            
            rootElement.AddToClassList("unity-inspector-element");
            
            rootElement.style.paddingTop = 20;
            rootElement.style.paddingBottom = 20;
            rootElement.style.paddingLeft = 20;
            rootElement.style.paddingRight = 20;

            var header = new Label("External Dependency Manager Settings");
            header.style.fontSize = 18;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 20;
            rootElement.Add(header);

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            rootElement.Add(scrollView);

            CreateBuildConfigurationSection(scrollView);
            CreateActionsSection(scrollView);
        }

        private void CreateBuildConfigurationSection(VisualElement root)
        {
            var section = CreateSection("Editor Settings", null);

            _enableScanToggle = new Toggle("Scan On Startup")
            {
                value = ExternalDependencyManagerSettings.instance.ScanOnStartup
            };
            section.Add(_enableScanToggle);

            var scanInfo = new HelpBox("", HelpBoxMessageType.Info);
            scanInfo.style.marginTop = 10;
            section.Add(scanInfo);

            UpdateScanInfo();

            root.Add(section);

            _enableScanToggle.RegisterValueChangedCallback(evt => {
                ExternalDependencyManagerSettings.instance.ScanOnStartup = _enableScanToggle.value;
                UpdateScanInfo();
            });

            void UpdateScanInfo()
            {
                if (_enableScanToggle.value)
                {
                    scanInfo.text = "Your editor project will be scanned for the EDM4U installation at startup.";
                }
                else
                {
                    scanInfo.text = "You will need to manually scan for the EDM4U installation.";
                }
            }
        }

        private void CreateActionsSection(VisualElement root)
        {
            var section = CreateSection("Actions", null);

            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.marginTop = 10;

            var resetButton = new Button(() => ResetToDefaults())
            {
                text = "Reset to Default Settings"
            };
            resetButton.style.flexGrow = 1;
            resetButton.style.marginRight = 10;
            buttonContainer.Add(resetButton);

            var scanButton = new Button(() => ManualScan())
            {
                text = "Scan for EDM4U Installation"
            };
            scanButton.style.flexGrow = 1;
            buttonContainer.Add(scanButton);

            section.Add(buttonContainer);
            root.Add(section);
        }

        private static VisualElement CreateSection(string title, string description)
        {
            var section = new VisualElement();
            section.style.marginBottom = 30;
            section.style.paddingTop = 15;
            section.style.paddingBottom = 15;
            section.style.paddingLeft = 15;
            section.style.paddingRight = 15;
            section.style.backgroundColor = new Color(0f, 0f, 0f, 0.1f);
            section.style.borderTopWidth = 1;
            section.style.borderBottomWidth = 1;
            section.style.borderLeftWidth = 1;
            section.style.borderRightWidth = 1;
            section.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            section.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            section.style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            section.style.borderRightColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            section.style.borderTopLeftRadius = 5;
            section.style.borderTopRightRadius = 5;
            section.style.borderBottomLeftRadius = 5;
            section.style.borderBottomRightRadius = 5;

            var header = new Label(title);
            header.style.fontSize = 14;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = description != null ? 10 : 15;
            section.Add(header);

            if (!string.IsNullOrEmpty(description))
            {
                var desc = new HelpBox(description, HelpBoxMessageType.Info);
                desc.style.marginBottom = 15;
                section.Add(desc);
            }

            return section;
        }
        
        public override void OnGUI(string searchContext)
        {
            // No longer used - we use OnSettingsActivate() instead
        }

        #region Utility Methods

        private void ManualScan()
        {
            ExternalDependencyManagerScanner.ScanForEDM4U();
        }

        private void ResetToDefaults()
        {
            _enableScanToggle.value = true;
        }

        private static IEnumerable<string> GetSearchKeywords()
        {
            return new[]
            {
                "IPTech",
                "External Dependency Manager",
                "Android",
                "Google",
                "EDM4U"
            };
        }

        #endregion
    }
}