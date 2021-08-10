using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public interface IModelProvider
    {
        Task<ModelData[]> ListAvailableModelsAsync(int startIndex, int count = -1);

        Task<GameObject> ProvideModelAsync(string modelId);
    }
}