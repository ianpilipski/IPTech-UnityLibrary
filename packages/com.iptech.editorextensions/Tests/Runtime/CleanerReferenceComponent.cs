using UnityEngine;

namespace IPTech.EditorTools.Tests {
    public class CleanerReferenceComponent : MonoBehaviour {
        [SerializeField, HideInInspector] private string m_AssetGUID;
        public void SetAssetGuid(string guid) => m_AssetGUID = guid;
    }
}
