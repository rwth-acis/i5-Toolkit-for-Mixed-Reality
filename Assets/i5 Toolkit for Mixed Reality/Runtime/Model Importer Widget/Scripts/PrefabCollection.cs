using System;
using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    [CreateAssetMenu(fileName = "PrefabCollection", menuName = "i5 Toolkit/Prefab Collection")]
    public class PrefabCollection : ScriptableObject, IModelProvider
    {
        [SerializeField] private ModelData[] modelData;

        public ModelData[] ModelData { get => modelData; }

        public Task<ModelData[]> ListAvailableModelsAsync(int startIndex, int count = -1)
        {
            return Task.FromResult(modelData);
        }

        public Task<GameObject> ProvideModelAsync(string modelId)
        {
            int index = Array.IndexOf(modelData, Array.Find(modelData, item => item.id == modelId));
            return Task.FromResult(Instantiate(modelData[index].model));
        }
    }
}