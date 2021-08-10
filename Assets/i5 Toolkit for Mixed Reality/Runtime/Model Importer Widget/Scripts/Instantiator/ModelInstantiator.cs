using i5.Toolkit.Core.Experimental.UnityAdapters;
using i5.Toolkit.Core.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class ModelInstantiator
    {
        public ITransformable TargetTransform { get; set; }

        public Bounds TargetBox { get; set; }

        public GameObject LastImportedObject { get; private set; }

        public IInstantiationEffect InstantiationEffect { get; set; }

        public List<IModelImportPostProcessor> PostProcessors { get; set; }

        public bool IsBusy { get; private set; }

        public ModelInstantiator(ITransformable targetTransform, Bounds targetBox)
        {
            TargetTransform = targetTransform;
            TargetBox = targetBox;
            PostProcessors = new List<IModelImportPostProcessor>()
            {
                new MovablePostProcessor()
            };
        }

        public async Task PresentModelAsync(GameObject newModel)
        {
            if (IsBusy)
            {
                i5Debug.LogWarning("Tried to create new object while the previous operation was still ongoing. Aborted second import operation.", this);
                return;
            }

            IsBusy = true;

            if (LastImportedObject != null && !LastImportedObject.transform.hasChanged)
            {
                GameObject.Destroy(LastImportedObject);
            }

            Vector3 center = TargetTransform.Position + TargetTransform.LocalScale.MultiplyComponentWise(TargetBox.center);
            Vector3 size = TargetTransform.LocalScale.MultiplyComponentWise(TargetBox.size);
            BoxVolume targetVolume = new BoxVolume(center, size);

            GameObjectUtils.PlaceInBox(newModel, targetVolume);

            if (InstantiationEffect != null)
            {
                await InstantiationEffect.PlayInstantiationEffectAsync(newModel);
            }

            foreach(IModelImportPostProcessor postProcessor in PostProcessors)
            {
                postProcessor.PostProcessGameObject(newModel);
            }

            LastImportedObject = newModel;
            LastImportedObject.transform.hasChanged = false;

            IsBusy = false;
        }
    }
}