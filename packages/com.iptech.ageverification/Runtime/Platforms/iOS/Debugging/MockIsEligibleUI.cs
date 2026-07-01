using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.iOS.Debugging
{
    public class MockIsEligibleUI : VisualElement
    {
        public event System.Action OnValuesChanged;
        
        private MockIsEligible _mockIsEligible;
        private bool _isEditMode;
        private Toggle _isEligibleToggle;
        private Label _previewLabel;
        private VisualElement _previewContainer;

        public MockIsEligibleUI(MockIsEligible mockIsEligible, bool isEditMode)
        {
            _mockIsEligible = mockIsEligible;
            _isEditMode = isEditMode;

            // Add some styling
            style.paddingTop = 10;
            style.paddingBottom = 10;
            style.paddingLeft = 5;
            style.paddingRight = 5;

            if (_isEditMode)
            {
                CreateHeader();
                CreateEditSection();
            }
            else
            {
                CreatePreviewSection();
            }

            // Initialize values from MockEligible
            InitializeValues();

            if (_isEditMode)
            {
                // Register callbacks for edit mode
                RegisterCallbacks();
            }

            // Update preview initially
            if (!_isEditMode)
            {
                UpdatePreview();
            }
        }

        private void CreateHeader()
        {
            var header = new Label($"Mock Is Eligible Configuration {(_isEditMode ? "(Edit Mode)" : "(Preview Mode)")}");
            header.style.fontSize = 16;
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.marginBottom = 10;
            Add(header);
        }

        private void CreateEditSection()
        {
            var editContainer = new VisualElement();
            editContainer.style.marginBottom = 15;

            var editLabel = new Label("Eligibility Settings");
            editLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            editLabel.style.marginBottom = 5;
            editContainer.Add(editLabel);

            _isEligibleToggle = new Toggle("Is Eligible for Age Features");
            _isEligibleToggle.style.marginLeft = 15;
            editContainer.Add(_isEligibleToggle);

            Add(editContainer);
        }

        private void CreatePreviewSection()
        {
            _previewContainer = new VisualElement();
            _previewContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.1f);
            _previewContainer.style.borderTopWidth = 1;
            _previewContainer.style.borderBottomWidth = 1;
            _previewContainer.style.borderLeftWidth = 1;
            _previewContainer.style.borderRightWidth = 1;
            _previewContainer.style.borderTopColor = Color.gray;
            _previewContainer.style.borderBottomColor = Color.gray;
            _previewContainer.style.borderLeftColor = Color.gray;
            _previewContainer.style.borderRightColor = Color.gray;
            _previewContainer.style.paddingTop = 10;
            _previewContainer.style.paddingBottom = 10;
            _previewContainer.style.paddingLeft = 10;
            _previewContainer.style.paddingRight = 10;

            var previewHeaderLabel = new Label("Generated Result:");
            previewHeaderLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            previewHeaderLabel.style.marginBottom = 5;
            _previewContainer.Add(previewHeaderLabel);

            _previewLabel = new Label();
            _previewLabel.style.marginLeft = 15;
            _previewContainer.Add(_previewLabel);

            Add(_previewContainer);
        }

        private void InitializeValues()
        {
            if (_mockIsEligible == null) return;

            if (_isEditMode && _isEligibleToggle != null)
            {
                _isEligibleToggle.value = _mockIsEligible.IsEligible;
            }
        }

        private void RegisterCallbacks()
        {
            if (_isEligibleToggle != null)
            {
                _isEligibleToggle.RegisterValueChangedCallback(evt => 
                {
                    if (_mockIsEligible != null) 
                    {
                        _mockIsEligible.IsEligible = evt.newValue;
                    }
                    OnValuesChanged?.Invoke();
                });
            }
        }

        private void UpdatePreview()
        {
            if (_mockIsEligible == null || _previewLabel == null) return;

            _previewLabel.text = $"Is Eligible: {_mockIsEligible.IsEligible}";
        }

        public void RefreshFromMockIsEligible()
        {
            if (_mockIsEligible == null) return;
            
            InitializeValues();
            UpdatePreview();
        }

        public bool GetCurrentValue()
        {
            return _mockIsEligible?.IsEligible ?? false;
        }
    }
}
