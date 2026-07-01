using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.iOS.Debugging
{
    public class MockIsEligibleErrorUI : VisualElement
    {
        private MockIsEligibleError _mockError;
        private bool _isEditing;
        
        private EnumField _errorTypeField;
        private TextField _messageField;
        private Label _previewLabel;

        public MockIsEligibleErrorUI(MockIsEligibleError mockError, bool isEditing = true)
        {
            _mockError = mockError;
            _isEditing = isEditing;
            ComposeUI();
        }

        public MockIsEligibleErrorUI(CachedIsEligibleError error): this(new MockIsEligibleError(error), false)
        {
        }

        private void ComposeUI()
        {
            style.flexDirection = FlexDirection.Column;
            style.paddingTop = 10;
            style.paddingBottom = 10;
            
            if (_isEditing)
            {
                var titleLabel = new Label("Error Configuration");
                titleLabel.style.fontSize = 16;
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.marginBottom = 10;
                Add(titleLabel);
                
                // Error Type field
                _errorTypeField = new EnumField("Error Type", _mockError.Type);
                _errorTypeField.RegisterValueChangedCallback(evt =>
                {
                    _mockError.Type = (CachedIsEligibleError.ErrorType)evt.newValue;
                });
                Add(_errorTypeField);

                // Message field
                _messageField = new TextField("Error Message");
                _messageField.value = _mockError.Message;
                _messageField.RegisterValueChangedCallback(evt =>
                {
                    _mockError.Message = evt.newValue;
                });
                _messageField.style.marginBottom = 10;
                Add(_messageField);
            }
            else
            {
                // Preview section
                var previewContainer = new VisualElement();
                previewContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
                previewContainer.style.paddingTop = 10;
                previewContainer.style.paddingBottom = 10;
                previewContainer.style.paddingLeft = 10;
                previewContainer.style.paddingRight = 10;
                previewContainer.style.borderTopWidth = 1;
                previewContainer.style.borderBottomWidth = 1;
                previewContainer.style.borderLeftWidth = 1;
                previewContainer.style.borderRightWidth = 1;
                previewContainer.style.borderTopColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                previewContainer.style.borderBottomColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                previewContainer.style.borderLeftColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                previewContainer.style.borderRightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                previewContainer.style.borderTopLeftRadius = 4;
                previewContainer.style.borderTopRightRadius = 4;
                previewContainer.style.borderBottomLeftRadius = 4;
                previewContainer.style.borderBottomRightRadius = 4;
                Add(previewContainer);

                var previewTitleLabel = new Label("Exception");
                previewTitleLabel.style.fontSize = 14;
                previewTitleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                previewTitleLabel.style.marginBottom = 5;
                previewContainer.Add(previewTitleLabel);

                _previewLabel = new Label();
                _previewLabel.style.whiteSpace = WhiteSpace.Normal;
                _previewLabel.style.color = new Color(1f, 0.6f, 0.6f); // Light red for error
                previewContainer.Add(_previewLabel);

                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            if (_mockError == null || _previewLabel == null) return;

            try
            {
                var exception = _mockError.CreateException();
                _previewLabel.text = $"Exception Type: {exception.GetType().Name}\nMessage: {exception.Message}";
            }
            catch (System.Exception ex)
            {
                _previewLabel.text = $"Error creating preview: {ex.Message}";
            }
        }

        public void RefreshFromMockError()
        {
            if (_mockError == null) return;
            
            if (_isEditing)
            {
                _errorTypeField.value = _mockError.Type;
                _messageField.value = _mockError.Message;
            }
            UpdatePreview();
        }

        public Exception GetCurrentException()
        {
            return _mockError?.CreateException();
        }
    }
}