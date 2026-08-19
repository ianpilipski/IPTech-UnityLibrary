using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using IPTech.AgeVerification.Android.AgeSignals.Debugging;
using System.Xml.Linq;
using System.IO;

namespace IPTech.AgeVerification.Android.AgeSignals.Editor
{
    public class AgeSignalsSettingsProvider : SettingsProvider
    {
        private VisualElement _cachedAgeSignalsAccessResultPreviewContainer;
        private VisualElement _cachedAgeSignalsAccessResultPreviewButtons;
        private VisualElement _cachedAgeSignalsResultPreviewContainer;
        private VisualElement _cachedAgeSignalsResultPreviewButtons;
        
        public AgeSignalsSettingsProvider() : base("Project/IPTech/AgeVerification/Android Age Signals", SettingsScope.Project, GetSearchKeywords())
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateAgeSignalsSettingsProvider()
        {
            return new AgeSignalsSettingsProvider();
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            rootElement.Clear();
            
            rootElement.AddToClassList("unity-inspector-element");
            
            rootElement.style.paddingTop = 20;
            rootElement.style.paddingBottom = 20;
            rootElement.style.paddingLeft = 20;
            rootElement.style.paddingRight = 20;

            var header = new Label("Android Age Signals Settings");
            header.style.fontSize = 18;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 20;
            rootElement.Add(header);

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            rootElement.Add(scrollView);

            CreateSdkInfoSection(scrollView);
            CreateCachedAgeSignalsAccessResultSection(scrollView);
            CreateCachedAgeSignalsResultSection(scrollView);
            CreateActionsSection(scrollView);

            RefreshAllContent();
            AgeSignalsDebugSettings.ValuesChanged += RefreshAllContent;
        }

        public override void OnDeactivate()
        {
            AgeSignalsDebugSettings.ValuesChanged -= RefreshAllContent;
        }

        private void RefreshAllContent()
        {
            RefreshCachedAgeSignalsAccessResultContent();
            RefreshCachedAgeSignalsResultContent();
        }

        private void CreateSdkInfoSection(VisualElement root)
        {
            var section = CreateSection("Android AgeSignals Info", null);
            var sdkVersion = ExtractAndroidAgeSignalsSdkVersionFromDependenciesXML();
            var sdkVersionLabel = new Label($"Sdk Version: {sdkVersion}");
            section.Add(sdkVersionLabel);
            root.Add(section);
        }

        private string ExtractAndroidAgeSignalsSdkVersionFromDependenciesXML()
        {
            var xmlFilePath = "Packages/com.iptech.ageverification/Editor/Android/PlayAgeSignalsDependencies.xml";
            if (File.Exists(xmlFilePath))
            {
                var doc = XDocument.Load(xmlFilePath);
                var sdkVersion = doc.Root?.Element("androidPackages")?.Element("androidPackage")?.Attribute("spec")?.Value;
                if (!string.IsNullOrEmpty(sdkVersion))
                {
                    // Do something with the SDK version
                    return sdkVersion;
                }
            }
            return "Version not found";
        }

        private void CreateCachedAgeSignalsAccessResultSection(VisualElement root)
        {
            var section = CreateSection("Cached Age Signals Access Result", 
                "This shows the currently cached Age Signals access result that will be returned in mock mode.");

            _cachedAgeSignalsAccessResultPreviewContainer = new VisualElement();
            _cachedAgeSignalsAccessResultPreviewContainer.style.marginBottom = 15;
            section.Add(_cachedAgeSignalsAccessResultPreviewContainer);

            _cachedAgeSignalsAccessResultPreviewButtons = new VisualElement();
            _cachedAgeSignalsAccessResultPreviewButtons.style.flexDirection = FlexDirection.Row;
            _cachedAgeSignalsAccessResultPreviewButtons.style.marginTop = 10;

            var clearButton = new Button(() => ClearCachedAgeSignalsAccessResult())
            {
                text = "Clear Cached Result"
            };
            clearButton.style.flexGrow = 1;
            clearButton.style.marginRight = 10;
            _cachedAgeSignalsAccessResultPreviewButtons.Add(clearButton);

            section.Add(_cachedAgeSignalsAccessResultPreviewButtons);

            root.Add(section);
        }

        private void CreateCachedAgeSignalsResultSection(VisualElement root)
        {
            var section = CreateSection("Cached Age Signals Result", 
                "This shows the currently cached Age Signals result that will be returned in mock mode.");
            
            _cachedAgeSignalsResultPreviewContainer = new VisualElement();
            _cachedAgeSignalsResultPreviewContainer.style.marginBottom = 15;
            section.Add(_cachedAgeSignalsResultPreviewContainer);

            _cachedAgeSignalsResultPreviewButtons = new VisualElement();
            _cachedAgeSignalsResultPreviewButtons.style.flexDirection = FlexDirection.Row;
            _cachedAgeSignalsResultPreviewButtons.style.marginTop = 10;

            var clearButton = new Button(() => ClearCachedAgeSignalsResult())
            {
                text = "Clear Cached Result"
            };
            clearButton.style.flexGrow = 1;
            clearButton.style.marginRight = 10;
            _cachedAgeSignalsResultPreviewButtons.Add(clearButton);

            section.Add(_cachedAgeSignalsResultPreviewButtons);
            root.Add(section);
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

            var docsButton = new Button(() => Application.OpenURL("https://developer.android.com/google/play/age-signals/use-age-signals-api"))
            {
                text = "Open Age Signals Documentation"
            };
            docsButton.style.flexGrow = 1;
            buttonContainer.Add(docsButton);

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

        private void ClearCachedAgeSignalsResult()
        {
            AgeSignalsDebugSettings.CachedAgeSignalsResult = null;
            RefreshCachedAgeSignalsResultContent();
        }

        private void ClearCachedAgeSignalsAccessResult()
        {
            AgeSignalsDebugSettings.CachedAgeSignalsAccessResult = null;
            RefreshCachedAgeSignalsAccessResultContent();
        }

        private void RefreshCachedAgeSignalsAccessResultContent()
        {
            if (_cachedAgeSignalsAccessResultPreviewContainer == null) return;

            // Clear existing content
            _cachedAgeSignalsAccessResultPreviewContainer.Clear();

            var cachedResult = AgeSignalsDebugSettings.CachedAgeSignalsAccessResult;

            if (cachedResult == null)
            {
                var noCacheLabel = new Label("No cached result available");
                noCacheLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                noCacheLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                noCacheLabel.style.marginBottom = 10;
                _cachedAgeSignalsAccessResultPreviewContainer.Add(noCacheLabel);
                _cachedAgeSignalsAccessResultPreviewButtons.style.display = DisplayStyle.None;
            }
            else
            {
                if (cachedResult.ResultKind == CachedAgeSignalsAccessResult.ResultType.AgeSignalsAccessResult)
                {
                    _cachedAgeSignalsAccessResultPreviewContainer.Add(new AgeSignalsAccessResultUI(cachedResult.Result, isPreview: true));
                }
                else if (cachedResult.ResultKind == CachedAgeSignalsAccessResult.ResultType.Exception)
                {
                    _cachedAgeSignalsAccessResultPreviewContainer.Add(new CachedErrorUI(cachedResult.Error, isPreview: true));
                }
                _cachedAgeSignalsAccessResultPreviewButtons.style.display = DisplayStyle.Flex;
            }
        }

        private void RefreshCachedAgeSignalsResultContent()
        {
            if (_cachedAgeSignalsResultPreviewContainer == null) return;

            // Clear existing content
            _cachedAgeSignalsResultPreviewContainer.Clear();

            var cachedResult = AgeSignalsDebugSettings.CachedAgeSignalsResult;

            if (cachedResult == null)
            {
                var noCacheLabel = new Label("No cached result available");
                noCacheLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                noCacheLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                noCacheLabel.style.marginBottom = 10;
                _cachedAgeSignalsResultPreviewContainer.Add(noCacheLabel);
                _cachedAgeSignalsResultPreviewButtons.style.display = DisplayStyle.None;
            }
            else
            {
                if (cachedResult.ResultKind == CachedAgeSignalsResult.ResultType.AgeSignalsResult)
                {
                    _cachedAgeSignalsResultPreviewContainer.Add(new AgeSignalsResultUI(cachedResult.Result, isPreview: true));
                }
                else if (cachedResult.ResultKind == CachedAgeSignalsResult.ResultType.Exception)
                {
                    _cachedAgeSignalsResultPreviewContainer.Add(new CachedErrorUI(cachedResult.Error, isPreview: true));
                }
                _cachedAgeSignalsResultPreviewButtons.style.display = DisplayStyle.Flex;
            }
        }

        private void ResetToDefaults()
        {
            AgeSignalsDebugSettings.EnableMockMode = false;
            AgeSignalsDebugSettings.CachedAgeSignalsResult = null;
            AgeSignalsDebugSettings.CachedAgeSignalsAccessResult = null;
        }

        private static IEnumerable<string> GetSearchKeywords()
        {
            return new[]
            {
                "IPTech",
                "Age Signals",
                "Android",
                "Google",
                "Mock",
                "Debug",
                "Age Verification",
                "User Status",
                "Privacy",
                "Play Console",
                "Cached",
                "Cache",
                "Result",
                "Preview",
                "Clear"
            };
        }

        #endregion
    }
}