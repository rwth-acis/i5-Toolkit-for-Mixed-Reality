using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public interface IModelImportPostProcessor
    {
        void PostProcessGameObject(GameObject gameObject);
    }
}