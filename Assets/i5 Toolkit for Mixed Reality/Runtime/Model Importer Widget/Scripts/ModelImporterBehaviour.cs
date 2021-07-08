using i5.Toolkit.Core.Utilities.UnityWrappers;
using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class ModelImporterBehaviour : MonoBehaviour
    {
        [SerializeField] private BoxCollider targetVolume;

        // TODO: temporary
        [SerializeField] ModelData[] modelDatas;
        [SerializeField] string modelId;

        public ModelImporter ModelImporter { get; private set; }

        private void Awake()
        {
            ModelImporter = new ModelImporter(new TransformAdapter(transform), new Bounds(targetVolume.center, targetVolume.size));
            ModelImporter.CurrentlySelectedProvider = new PrimitivesProvider(modelDatas);
        }

        public async Task ImportModelAsync()
        {
            await ModelImporter.ImportModelAsync(modelId);
        }
    }
}