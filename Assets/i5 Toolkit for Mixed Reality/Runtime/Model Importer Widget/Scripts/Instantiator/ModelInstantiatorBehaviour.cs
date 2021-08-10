using i5.Toolkit.Core.Experimental.UnityAdapters;
using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class ModelInstantiatorBehaviour : MonoBehaviour
    {
        [SerializeField] private BoxCollider targetVolume;

        public ModelInstantiator ModelInstantiator { get; private set; }

        private void Awake()
        {
            ModelInstantiator = new ModelInstantiator(new TransformAdapter(transform), new Bounds(targetVolume.center, targetVolume.size));
        }

        public async Task PresentModelAsync(GameObject newModel)
        {
            await ModelInstantiator.PresentModelAsync(newModel);
        }
    }
}