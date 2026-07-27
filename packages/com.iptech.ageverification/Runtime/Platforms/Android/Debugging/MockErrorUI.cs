using IPTech.AgeVerification.Debugging;
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

            var b = new MockPopupBuilder();

            if(_isEditing) {
                var errorCodeField = b.AddEnumProperty("Error Code", AgeSignalsException.KnownErrorCodes.NO_ERROR);
                
                var errorTypeField = b.AddEnumProperty("Error Type", _mockError.Type);
                errorTypeField.RegisterValueChangedCallback(evt =>
                {
                    _mockError.Type = (CachedError.ErrorType)evt.newValue;
                    ToggleErrorCodeFieldVisibility(errorCodeField);
                });
                
                errorCodeField.value = AgeSignalsException.KnownErrorCodes.NO_ERROR;
                if(AgeSignalsException.TryGetKnownErrorCode(_mockError.ErrorCode, out var knownError))
                {
                    errorCodeField.value = knownError;
                }
                errorCodeField.RegisterValueChangedCallback(evt =>
                {
                    _mockError.ErrorCode = (int)(AgeSignalsException.KnownErrorCodes)evt.newValue;
                });
                var errorMessageField = b.AddTextFieldProperty("Error Message", _mockError.Message);
                errorMessageField.RegisterValueChangedCallback(evt =>
                {
                    _mockError.Message = evt.newValue;
                });
                
                ToggleErrorCodeFieldVisibility(errorCodeField);
            } 
            else
            {
                b.BeginSection("Generated Result:");
                b.AddLabelProperty($"Exception: {_mockError.Type}");
                if(_mockError.Type == CachedError.ErrorType.AgeSignalsException)
                {
                    var knownErrorStr = "Unknown";
                    if(_mockError.ErrorCode!=0 && AgeSignalsException.TryGetKnownErrorCode(_mockError.ErrorCode, out var knownError))
                    {
                       knownErrorStr = knownError.ToString();
                    }
                    b.AddLabelProperty($"Error Code: {knownErrorStr} ({_mockError.ErrorCode})");
                }
                b.AddLabelProperty($"Exception Message: {_mockError.Message}");
                b.EndSection();
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