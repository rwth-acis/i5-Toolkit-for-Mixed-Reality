using i5.Toolkit.Core.Experimental.UnityAdapters;
using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class ModelImporterBehaviour : MonoBehaviour
    {
        [SerializeField] private BoxCollider targetVolume;

        public ModelImporter ModelImporter { get; private set; }

        private void Awake()
        {
            ModelImporter = new ModelImporter(new TransformAdapter(transform), new Bounds(targetVolume.center, targetVolume.size));
        }

        public async Task PresentModelAsync(GameObject newModel)
        {
            await ModelImporter.PresentModelAsync(newModel);
        }
    }
}