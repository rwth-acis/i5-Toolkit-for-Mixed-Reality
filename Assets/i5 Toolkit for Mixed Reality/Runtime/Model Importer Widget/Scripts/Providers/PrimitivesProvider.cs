using i5.Toolkit.Core.Utilities;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class PrimitivesProvider : IModelProvider
    {
        public ModelData[] AvailablePrimitives { get; private set; }

        public Shader Shader { get; set; } = Shader.Find("Mixed Reality Toolkit/Standard");

        public PrimitivesProvider(ModelData[] availablePrimitives)
        {
            AvailablePrimitives = availablePrimitives;
        }

        public Task<ModelData[]> ListAvailableModelsAsync(int page, int itemsPerPage)
        {
            return Task.FromResult(AvailablePrimitives);
        }

        public Task<GameObject> ProvideModelAsync(string modelId)
        {
            if (Enum.TryParse(modelId, out PrimitiveType primitiveType))
            {
                GameObject obj = GameObject.CreatePrimitive(primitiveType);
                obj.GetComponent<Renderer>().material.shader = Shader;
                return Task.FromResult(obj);
            }
            else
            {
                i5Debug.LogError($"Unable to find a model with the given ID {modelId}", this);
            }
            return Task.FromResult<GameObject>(null);
        }
    }
}
