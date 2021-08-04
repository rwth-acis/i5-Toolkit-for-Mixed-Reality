using i5.Toolkit.Core.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class ModelListItemBehaviour : MonoBehaviour
    {
        [SerializeField] private BoxCollider modelTargetBox;
        [SerializeField] private Transform modelPreviewParent;
        [SerializeField] private SpriteRenderer thumbnail;
        [SerializeField] private TextMeshPro nameLabel;

        private ModelData data;
        private GameObject previewInstance;

        public ModelData Data
        {
            get => data;
            set
            {
                data = value;
                UpdateView();
            }
        }

        public void UpdateView()
        {
            if (Data.model != null)
            {
                UpdateInstance();
            }
            //else
            //{
            //    thumbnail.sprite = Data.thumbnail;
            //}
            //nameLabel.text = data.name;
        }

        private void UpdateInstance()
        {
            if (previewInstance != null)
            {
                Destroy(previewInstance);
            }
            else
            {
                previewInstance = Instantiate(data.model);
                // calculate the box volume in which the object should be placed
                Vector3 size = modelTargetBox.size.MultiplyComponentWise(modelTargetBox.transform.localScale);
                Vector3 center = modelTargetBox.transform.position + modelTargetBox.center.MultiplyComponentWise(modelTargetBox.transform.localScale);
                BoxVolume targetVolume = new BoxVolume(center, size, modelTargetBox.transform.rotation);
                GameObjectUtils.PlaceInBox(previewInstance, targetVolume);
                // make the object slightly smaller to avoid clipping artifacts
                previewInstance.transform.localScale *= 0.99f;
                // make it a child of the parent object
                previewInstance.transform.parent = modelPreviewParent;
            }
        }
    }
}