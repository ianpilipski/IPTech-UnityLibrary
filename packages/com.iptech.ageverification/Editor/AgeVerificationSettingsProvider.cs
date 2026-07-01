using System.Collections.Generic;
using IPTech.AgeVerification.iOS.Debugging;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.iOS.Editor
{
    public class AgeVerificationSettingsProvider : SettingsProvider
    {
        private Toggle _postProcessToggle;
        private CachedResult _mockCachedResult;
        private VisualElement _cachedResultPreviewContainer;
        private CachedResult _lastCachedResult;
        private VisualElement _cachedIsEligiblePreviewContainer;
        
        public AgeVerificationSettingsProvider() : base("Project/IPTech/AgeVerification/iOS Age Range", SettingsScope.Project, GetSearchKeywords())
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateAgeRangeSettingsProvider()
        {
            return new AgeVerificationSettingsProvider();
        }

        private bool PostProcessEnabled
        {
            get => !AgeRangeSettings.instance.DisablePostProcessor;
            set => AgeRangeSettings.instance.DisablePostProcessor = !value;
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
            var header = new Label("iOS Age Range Settings");
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

            // Cached IsEligible Section
            CreateCachedIsEligibleSection(scrollView);

            // Build Configuration Section  
            CreateBuildConfigurationSection(scrollView);

            // Actions Section
            CreateActionsSection(scrollView);
        }

        private void CreateCachedResultSection(VisualElement root)
        {
            var section = CreateSection("Cached Age Range Result", 
                "This shows the currently cached Age Range result that will be returned in mock mode.");
            
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

            RefreshCachedResultDisplay();
        }

        private void CreateCachedIsEligibleSection(VisualElement root)
        {
            var section = CreateSection("Cached Age Features Eligibility", 
                "This shows the currently cached eligibility result that will be returned in mock mode.");
            
            _cachedIsEligiblePreviewContainer = new VisualElement();
            _cachedIsEligiblePreviewContainer.style.marginBottom = 15;
            section.Add(_cachedIsEligiblePreviewContainer);

            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.marginTop = 10;

            var clearCachedIsEligibleButton = new Button(() => ClearCachedIsEligibleResult())
            {
                text = "Clear Cached Eligibility"
            };
            clearCachedIsEligibleButton.style.flexGrow = 1;
            clearCachedIsEligibleButton.style.marginRight = 10;
            buttonContainer.Add(clearCachedIsEligibleButton);

            var refreshCachedIsEligibleButton = new Button(() => RefreshCachedIsEligibleDisplay())
            {
                text = "Refresh Cached Eligibility"
            };
            refreshCachedIsEligibleButton.style.flexGrow = 1;
            buttonContainer.Add(refreshCachedIsEligibleButton);

            section.Add(buttonContainer);
            root.Add(section);

            RefreshCachedIsEligibleDisplay();
        }

        private void CreateBuildConfigurationSection(VisualElement root)
        {
            var section = CreateSection("Build Configuration",
                "These settings control the build-time behavior of the Age Range module.");

            _postProcessToggle = new Toggle("Enable Post Process Build");
            _postProcessToggle.value = PostProcessEnabled;
            _postProcessToggle.RegisterValueChangedCallback(evt => PostProcessEnabled = evt.newValue);
            section.Add(_postProcessToggle);

            var postProcessInfo = new HelpBox("", HelpBoxMessageType.Info);
            postProcessInfo.style.marginTop = 10;
            section.Add(postProcessInfo);

            void UpdatePostProcessInfo()
            {
                if (_postProcessToggle.value)
                {
                    postProcessInfo.text = "The Age Range capability will be automatically added to your iOS builds.";
                    postProcessInfo.messageType = HelpBoxMessageType.Info;
                }
                else
                {
                    postProcessInfo.text = "You will need to manually add the Age Range capability to your iOS project.";
                    postProcessInfo.messageType = HelpBoxMessageType.Warning;
                }
            }

            _postProcessToggle.RegisterValueChangedCallback(evt => UpdatePostProcessInfo());
            UpdatePostProcessInfo();

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

            var docsButton = new Button(() => Application.OpenURL("https://developer.apple.com/documentation/declaredagerange/"))
            {
                text = "Open Age Range Documentation"
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
            // use visual elements
        }

        public override void OnDeactivate()
        {
            if (_mockCachedResult != null)
            {
                _mockCachedResult = null;
            }
        }

        #region Utility Methods

        private void ClearCachedResult()
        {
            AgeRangeDebugSettings.CachedResult = null;
            Debug.Log("Cached result has been cleared.");
            RefreshCachedResultDisplay();
        }

        private void ClearCachedIsEligibleResult()
        {
            AgeRangeDebugSettings.CachedIsEligibleForAgeFeaturesResult = null;
            Debug.Log("Cached IsEligible result has been cleared.");
            RefreshCachedIsEligibleDisplay();
        }

        private void RefreshCachedResultDisplay()
        {
            if (_cachedResultPreviewContainer == null) return;

            _cachedResultPreviewContainer.Clear();

            var cachedResult = AgeRangeDebugSettings.CachedResult;

            if (cachedResult == null)
            {
                var noCacheLabel = new Label("No cached result available");
                noCacheLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                noCacheLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                noCacheLabel.style.marginBottom = 10;
                _cachedResultPreviewContainer.Add(noCacheLabel);
            }
            else
            {
                if (_lastCachedResult != cachedResult)
                {
                    _lastCachedResult = cachedResult;
                }

                if (_lastCachedResult != null)
                {
                    if(_lastCachedResult.ResultKind == CachedResult.ResultType.AgeRangeResult)
                    {
                        var mockResult = new MockResult();
                        mockResult.PopulateFromResult(_lastCachedResult.Result);
                        var mockResultUI = new MockResultUI(mockResult);
                        var previewElement = mockResultUI.GetPreviewElement();
                        _cachedResultPreviewContainer.Add(previewElement);
                    }
                    else
                    {
                        var mockErrorUI = new MockErrorUI(_lastCachedResult.Error);
                        _cachedResultPreviewContainer.Add(mockErrorUI);
                    }
                }
            }
        }

        private void RefreshCachedIsEligibleDisplay()
        {
            if (_cachedIsEligiblePreviewContainer == null) return;

            _cachedIsEligiblePreviewContainer.Clear();

            var cachedIsEligible = AgeRangeDebugSettings.CachedIsEligibleForAgeFeaturesResult;

            if (cachedIsEligible == null)
            {
                var noCacheLabel = new Label("No cached eligibility result available");
                noCacheLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                noCacheLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                noCacheLabel.style.marginBottom = 10;
                _cachedIsEligiblePreviewContainer.Add(noCacheLabel);
            }
            else
            {
                // Create and display the preview
                if(cachedIsEligible.ResultKind == CachedIsEligibleResult.ResultType.IsEligibleResult)
                {
                    var mockIsEligible = new MockIsEligible();
                    mockIsEligible.PopulateFromResult(cachedIsEligible);
                    var previewElement = new MockIsEligibleUI(mockIsEligible, false);
                    _cachedIsEligiblePreviewContainer.Add(previewElement);
                }
                else
                {
                    var mockErrorUI = new MockIsEligibleErrorUI(cachedIsEligible.Error);
                    _cachedIsEligiblePreviewContainer.Add(mockErrorUI);
                }
            }
        }

        private void ResetToDefaults()
        {
            AgeRangeApi.EnableMockMode = false;
            AgeRangeDebugSettings.CachedResult = null;
            AgeRangeDebugSettings.CachedIsEligibleForAgeFeaturesResult = null;
            
            Debug.Log("Settings have been reset to default values.");

            // Refresh the UI with default values
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (_postProcessToggle != null)
                _postProcessToggle.value = PostProcessEnabled;

            RefreshCachedResultDisplay();
            RefreshCachedIsEligibleDisplay();
        }

        private static IEnumerable<string> GetSearchKeywords()
        {
            return new[]
            {
                "IPTech",
                "Age Range",
                "iOS",
                "Apple",
                "Mock",
                "Build",
                "Post Process",
                "Debug",
                "Capability",
                "Age Declaration",
                "Age Verification",
                "Cached",
                "Cache",
                "Result",
                "Preview",
                "Clear",
                "Eligible",
                "Eligibility",
                "Age Features",
                "IsEligible"
            };
        }

        #endregion
    }
}