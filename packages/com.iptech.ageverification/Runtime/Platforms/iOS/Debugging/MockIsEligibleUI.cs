using IPTech.AgeVerification.Debugging;
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

            var b = new MockPopupBuilder();

            if (_isEditMode)
            {
                CreateHeader(b);
                CreateEditSection(b);
            }
            else
            {
                CreatePreviewSection(b);
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

            this.Add(b.Root);
        }

        private void CreateHeader(MockPopupBuilder b)
        {
            b.BeginSection($"Mock Is Eligible Configuration {(_isEditMode ? "(Edit Mode)" : "(Preview Mode)")}");
            b.EndSection();
        }

        private void CreateEditSection(MockPopupBuilder b)
        {
            b.BeginSection("Eligibility Settings");
            _isEligibleToggle = b.AddToggleProperty("Is Eligible for Age Features", false);
            b.EndSection();
        }

        private void CreatePreviewSection(MockPopupBuilder b)
        {
            _previewContainer = b.BeginSection("Generated Result");
            _previewLabel = b.AddLabelProperty("<json>"); 
            b.EndSection();
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
