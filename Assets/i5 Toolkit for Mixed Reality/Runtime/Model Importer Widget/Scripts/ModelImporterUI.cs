using i5.Toolkit.Core.Utilities.UnityWrappers;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class ModelImporterUI : MonoBehaviour
    {
        [SerializeField] private BoxCollider targetVolume;

        private ModelImporter modelImporter;

        private void Awake()
        {
            modelImporter = new ModelImporter(new TransformAdapter(transform), new Bounds(targetVolume.center, targetVolume.size));
            modelImporter.CurrentlySelectedProvider = new PrimitivesProvider();
        }

        public void ImportModel()
        {
            modelImporter.ImportModel();
        }
    }
}