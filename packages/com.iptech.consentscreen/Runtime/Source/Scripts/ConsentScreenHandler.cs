using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IPTech.ConsentScreen {
    public abstract class ConsentScreenHandler : MonoBehaviour
    {
        public abstract ConsentInfo GetCurrentConsentInfo();
        public abstract void SetConsentInfo(ConsentInfo info);
    }
}
