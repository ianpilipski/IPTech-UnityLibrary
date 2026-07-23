using IPTech.AgeVerification;
using IPTech.Platform;
using UnityEngine;

public class EverythingReferenced : MonoBehaviour
{
    public void CreatePlatform()
    {
        var config = new IPTechPlatformConfig();
        config.ConfigureAgeVerification();
        
    }
}
