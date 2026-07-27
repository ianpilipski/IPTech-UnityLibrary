
using UnityEngine;
using IPTech.ConsentScreen;

public class ConsentScreenHandler : IPTech.ConsentScreen.ConsentScreenHandler
{
    public override ConsentInfo GetCurrentConsentInfo() 
    {
        return new ConsentInfo();
    }

    public override void SetConsentInfo(ConsentInfo info)
    {
        
    }
}
