using IPTech.AgeVerification.Debugging;
using UnityEngine;
using UnityEngine.UIElements;

namespace IPTech.AgeVerification.iOS.Debugging
{
    public class IsEligibleResultUI : VisualElement
    {
        public event System.Action OnValuesChanged;
        
        public IsEligibleResultUI(IsEligibleResult mockIsEligible, bool isPreview = true)
        {
            var b = new MockPopupBuilder();
            this.Add(b.Root);

            if (!isPreview)
            {
                b.AddToggleProperty("Is Eligible for Age Features", mockIsEligible.IsEligible)
                    .RegisterValueChangedCallback(evt =>
                    {
                        mockIsEligible.IsEligible = evt.newValue;
                        OnValuesChanged?.Invoke();
                    });
            }
            else
            {
                b.AddLabelProperty("Is Eligible: " + mockIsEligible.IsEligible);
            }
        }
    }
}
