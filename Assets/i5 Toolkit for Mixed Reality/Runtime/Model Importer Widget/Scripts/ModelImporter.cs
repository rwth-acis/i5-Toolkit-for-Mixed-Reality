using i5.Toolkit.Core.Utilities;
using i5.Toolkit.Core.Utilities.UnityWrappers;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class ModelImporter
    {
        public IModelProvider CurrentlySelectedProvider { get; set; }

        public ITransformable TargetTransform { get; set; }

        public Bounds TargetBox { get; set; }

        public GameObject LastImportedObject { get; private set; }

        public IInstantiationEffect InstantiationEffect { get; set; }

        public List<IModelImportPostProcessor> PostProcessors { get; set; }

        public ModelImporter(ITransformable targetTransform, Bounds targetBox)
        {
            TargetTransform = targetTransform;
            TargetBox = targetBox;
            PostProcessors = new List<IModelImportPostProcessor>()
            {
                new MovablePostProcessor()
            };
        }

        public async Task ImportModelAsync(string modelId)
        {
            if (LastImportedObject != null && !LastImportedObject.transform.hasChanged)
            {
                GameObject.Destroy(LastImportedObject);
            }

            GameObject importedModel = await CurrentlySelectedProvider.ProvideModelAsync(modelId);

            Bounds overallBounds = ObjectBounds.GetComposedRendererBounds(importedModel);

            importedModel.transform.position = 
                TargetTransform.Position
                + TargetTransform.LocalScale.MultiplyComponentWise(TargetBox.center)
                - overallBounds.center;
            importedModel.transform.rotation = TargetTransform.Rotation;

            Vector3 realTargetBoxSize = TargetTransform.LocalScale.MultiplyComponentWise(TargetBox.size);

            Vector3 scalingFactors = realTargetBoxSize.DivideComponentWiseBy(overallBounds.size);
            float scalingFactor = scalingFactors.MinimumComponent();
            importedModel.transform.localScale *= scalingFactor;

            if (InstantiationEffect != null)
            {
                await InstantiationEffect.PlayInstantiationEffectAsync(importedModel);
            }

            foreach(IModelImportPostProcessor postProcessor in PostProcessors)
            {
                postProcessor.PostProcessGameObject(importedModel);
            }

            LastImportedObject = importedModel;
            LastImportedObject.transform.hasChanged = false;
        }
    }
}