using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;


namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class MovablePostProcessor : IModelImportPostProcessor
    {
        public void PostProcessGameObject(GameObject gameObject)
        {
            gameObject.AddComponent<ObjectManipulator>();
            gameObject.AddComponent<NearInteractionGrabbable>();
        }
    }
}