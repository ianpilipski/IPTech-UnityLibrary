
using IPTech.AgeVerification.Debugging;
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
            var b = new MockPopupBuilder();

            if(_isEditing) {
                var errorType = b.AddEnumProperty("Error Type", _mockError.Type);
                errorType.RegisterValueChangedCallback(evt =>
                {
                    _mockError.Type = (CachedError.ErrorType)evt.newValue;
                });
                
                var errorMessage = b.AddTextFieldProperty("Error Message", _mockError.Message);
                errorMessage.RegisterValueChangedCallback(evt =>
                {
                    _mockError.Message = evt.newValue;
                });
            } 
            else
            {
                b.BeginSection("Generated Result:");
                b.AddLabelProperty($"Exception: {_mockError.Type}");
                b.AddLabelProperty($"Error Message: {_mockError.Message}");
                b.EndSection();
            }

            this.Add(b.Root);
        }
    }
}