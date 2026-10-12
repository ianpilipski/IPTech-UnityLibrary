using UnityEngine;

namespace IPTech.EditorTools.Tests {
    public class CleanerReferenceFixture : ScriptableObject {
        [SerializeField, HideInInspector] private string m_AssetGUID;
        public Object DirectReference;
        public TerrainData Terrain;

        public void SetAssetGuid(string guid) => m_AssetGUID = guid;
    }
}
