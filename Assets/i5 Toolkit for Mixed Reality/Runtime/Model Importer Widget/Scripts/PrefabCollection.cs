using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    [CreateAssetMenu(fileName = "PrefabCollection", menuName = "i5 Toolkit/Prefab Collection")]
    public class PrefabCollection : ScriptableObject
    {
        [SerializeField] private ModelData[] modelData;

        public ModelData[] ModelData { get => modelData; }
    }
}