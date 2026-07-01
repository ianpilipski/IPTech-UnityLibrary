using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using IPTech.AgeVerification.Android.AgeSignals.Debugging;

namespace IPTech.AgeVerification.Android.AgeSignals.Editor
{
    public class AgeSignalsSettingsProvider : SettingsProvider
    {
        private VisualElement _cachedResultPreviewContainer;
        
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

            
            // Header
            var header = new Label("Android Age Signals Settings");
            header.style.fontSize = 18;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 20;
            rootElement.Add(header);

            // Create scroll view for all content sections
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            rootElement.Add(scrollView);

            // Cached Result Section
            CreateCachedResultSection(scrollView);

            // Actions Section
            CreateActionsSection(scrollView);
        }

        private void CreateCachedResultSection(VisualElement root)
        {
            var section = CreateSection("Cached Age Signals Result", 
                "This shows the currently cached Age Signals result that will be returned in mock mode.");
            
            _cachedResultPreviewContainer = new VisualElement();
            _cachedResultPreviewContainer.style.marginBottom = 15;
            section.Add(_cachedResultPreviewContainer);

            // Button container for actions
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.marginTop = 10;

            var clearCachedResultButton = new Button(() => ClearCachedResult())
            {
                text = "Clear Cached Result"
            };
            clearCachedResultButton.style.flexGrow = 1;
            clearCachedResultButton.style.marginRight = 10;
            buttonContainer.Add(clearCachedResultButton);

            var refreshCachedResultButton = new Button(() => RefreshCachedResultDisplay())
            {
                text = "Refresh Cached Result"
            };
            refreshCachedResultButton.style.flexGrow = 1;
            buttonContainer.Add(refreshCachedResultButton);

            section.Add(buttonContainer);
            root.Add(section);

            // Initialize the cached result display
            RefreshCachedResultDisplay();
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

        private void ClearCachedResult()
        {
            AgeSignalsDebugSettings.CachedResult = null;
            RefreshCachedResultDisplay();
            Debug.Log("[AgeSignals] Cached result has been cleared.");
        }

        private void RefreshCachedResultDisplay()
        {
            if (_cachedResultPreviewContainer == null) return;

            // Clear existing content
            _cachedResultPreviewContainer.Clear();

            var cachedResult = AgeSignalsDebugSettings.CachedResult;

            if (cachedResult == null)
            {
                // No cached result - show message
                var noCacheLabel = new Label("No cached result available");
                noCacheLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                noCacheLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                noCacheLabel.style.marginBottom = 10;
                _cachedResultPreviewContainer.Add(noCacheLabel);
            }
            else
            {
                // Create and display the preview
                if (cachedResult.ResultKind == CachedResult.ResultType.AgeSignalsResult)
                {
                    var mockResult = new MockResult();
                    mockResult.PopulateFromResult(cachedResult.Result);
                    var mockResultUI = new MockResultUI(mockResult);
                    var previewElement = mockResultUI.GetPreviewElement();
                    _cachedResultPreviewContainer.Add(previewElement);
                }
                else if (cachedResult.ResultKind == CachedResult.ResultType.Exception)
                {
                    var mockErrorUI = new MockErrorUI(cachedResult.Error);
                    _cachedResultPreviewContainer.Add(mockErrorUI);
                }
            }
        }

        private void ResetToDefaults()
        {
            AgeSignalsDebugSettings.EnableMockMode = false;
            
            Debug.Log("[AgeSignals] Settings have been reset to default values.");
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