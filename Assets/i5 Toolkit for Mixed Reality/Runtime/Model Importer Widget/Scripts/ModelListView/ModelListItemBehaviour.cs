using i5.Toolkit.Core.Utilities;
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

        private GameObject previewInstance;

        public ModelData Data { get; private set; }

        public int Index { get; private set; }

        public ModelListViewBehaviour ParentListView { get; private set; }

        public void SetUp(ModelListViewBehaviour parent, int index, ModelData data)
        {
            ParentListView = parent;
            Data = data;
            Index = index;
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
                previewInstance = Instantiate(Data.model);
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

        public void OnSelect()
        {
            ParentListView.Select(Index);
        }
    }
}