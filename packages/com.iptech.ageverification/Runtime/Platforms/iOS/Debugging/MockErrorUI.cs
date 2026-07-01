
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.iOS.Debugging
{
    public class MockErrorUI : VisualElement
    {
        private MockError _mockError;
        private bool _isEditing;

        public MockErrorUI(CachedError error): this(new MockError(error), false)
        {
        }

        public MockErrorUI(MockError mockError, bool isEditing = true)
        {
            _mockError = mockError;
            _isEditing = isEditing;
            ComposeUI();
        }

        private void ComposeUI()
        {
            this.Clear();

            if(_isEditing) {
                var errorTypeField = new EnumField("Error Type", _mockError.Type);
                errorTypeField.RegisterValueChangedCallback(evt =>
                {
                    _mockError.Type = (CachedError.ErrorType)evt.newValue;
                });
                this.Add(errorTypeField);

                var errorMessageField = new TextField("Error Message");
                errorMessageField.value = _mockError.Message;
                errorMessageField.RegisterValueChangedCallback(evt =>
                {
                    _mockError.Message = evt.newValue;
                });
                this.Add(errorMessageField);
            } 
            else
            {
                var previewContainer = new VisualElement();
                previewContainer.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.1f);
                previewContainer.style.borderTopWidth = 1;
                previewContainer.style.borderBottomWidth = 1;
                previewContainer.style.borderLeftWidth = 1;
                previewContainer.style.borderRightWidth = 1;
                previewContainer.style.borderTopColor = Color.gray;
                previewContainer.style.borderBottomColor = Color.gray;
                previewContainer.style.borderLeftColor = Color.gray;
                previewContainer.style.borderRightColor = Color.gray;
                previewContainer.style.paddingTop = 10;
                previewContainer.style.paddingBottom = 10;
                previewContainer.style.paddingLeft = 10;
                previewContainer.style.paddingRight = 10;
                this.Add(previewContainer);

                var resultContainer = new VisualElement();
                previewContainer.Add(resultContainer);

                var resultLabel = new Label("Generated Result:");
                resultLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                resultLabel.style.marginBottom = 5;
                resultContainer.Add(resultLabel);

                var errorTypeLabel = new Label($"Exception Type: {_mockError.Type}");
                errorTypeLabel.style.marginLeft = 15;
                resultContainer.Add(errorTypeLabel);

                var errorMessageLabel = new Label($"Exception Message: {_mockError.Message}");
                errorMessageLabel.style.marginLeft = 15;
                resultContainer.Add(errorMessageLabel);
            }
        }
    }
}