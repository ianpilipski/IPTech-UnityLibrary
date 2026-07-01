using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.Android.AgeSignals.Debugging
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
                var errorCodeField = new EnumField("Error Code", AgeSignalsException.KnownErrorCodes.NO_ERROR);
                
                var errorTypeField = new EnumField("Error Type", _mockError.Type);
                errorTypeField.RegisterValueChangedCallback(evt =>
                {
                    _mockError.Type = (CachedError.ErrorType)evt.newValue;
                    ToggleErrorCodeFieldVisibility(errorCodeField);
                });
                this.Add(errorTypeField);

                errorCodeField.value = AgeSignalsException.KnownErrorCodes.NO_ERROR;
                if(AgeSignalsException.TryGetKnownErrorCode(_mockError.ErrorCode, out var knownError))
                {
                    errorCodeField.value = knownError;
                }
                errorCodeField.RegisterValueChangedCallback(evt =>
                {
                    _mockError.ErrorCode = (int)(AgeSignalsException.KnownErrorCodes)evt.newValue;
                });
                this.Add(errorCodeField);

                var errorMessageField = new TextField("Error Message");
                errorMessageField.value = _mockError.Message;
                errorMessageField.RegisterValueChangedCallback(evt =>
                {
                    _mockError.Message = evt.newValue;
                });
                this.Add(errorMessageField);

                ToggleErrorCodeFieldVisibility(errorCodeField);
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

                if(_mockError.Type == CachedError.ErrorType.AgeSignalsException)
                {
                    var knownErrorStr = "Unknown";
                    if(_mockError.ErrorCode!=0 && AgeSignalsException.TryGetKnownErrorCode(_mockError.ErrorCode, out var knownError))
                    {
                       knownErrorStr = knownError.ToString();
                    }
                    var errorCodeLabel = new Label($"Error Code: {knownErrorStr} ({_mockError.ErrorCode})");
                    errorCodeLabel.style.marginLeft = 15;
                    resultContainer.Add(errorCodeLabel);
                }

                var errorMessageLabel = new Label($"Exception Message: {_mockError.Message}");
                errorMessageLabel.style.marginLeft = 15;
                resultContainer.Add(errorMessageLabel);
            }
        }

        private void ToggleErrorCodeFieldVisibility(VisualElement errorCodeField)
        {
            if(_mockError.Type == CachedError.ErrorType.AgeSignalsException)
            {
                errorCodeField.style.display = DisplayStyle.Flex;
            }
            else
            {
                errorCodeField.style.display = DisplayStyle.None;
            }
        }
    }
}