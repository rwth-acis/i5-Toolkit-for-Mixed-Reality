using i5.Toolkit.Core.Utilities;
using i5.Toolkit.Core.Utilities.UnityWrappers;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class ModelImporter
    {
        public IModelProvider CurrentlySelectedProvider { get; set; }

        public ITransformable TargetTransform { get; set; }

        public Bounds TargetBox { get; set; }

        public ModelImporter(ITransformable targetTransform, Bounds targetBox)
        {
            TargetTransform = targetTransform;
            TargetBox = targetBox;
        }

        public void ImportModel()
        {
            GameObject importedModel = CurrentlySelectedProvider.ProvideModel();

            Bounds overallBounds = ObjectBounds.GetComposedRendererBounds(importedModel);

            importedModel.transform.position = 
                TargetTransform.Position
                + TargetTransform.LocalScale.MultiplyComponentWise(TargetBox.center)
                - overallBounds.center;

            Vector3 realTargetBoxSize = TargetTransform.LocalScale.MultiplyComponentWise(TargetBox.size);

            Vector3 scalingFactors = realTargetBoxSize.DivideComponentWiseBy(overallBounds.size);
            float scalingFactor = scalingFactors.MinimumComponent();
            importedModel.transform.localScale *= scalingFactor;
        }


    }
}