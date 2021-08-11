using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.ModelImporterWidget
{
    public class ModelListViewBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform parentContainer;
        [SerializeField] private GameObject listItemPrefab;
        [SerializeField] private ScrollingObjectCollection scrollingCollection;
        [SerializeField] private GridObjectCollection alignmentComponent;

        private List<ModelListItemBehaviour> listItems = new List<ModelListItemBehaviour>();
        private ModelData[] dataItems;

        public delegate void ModelListItemSelectedEventHandler(int selectedIndex);

        public event ModelListItemSelectedEventHandler ModelListItemSelected;

        public ModelData[] DataItems
        {
            get => dataItems;
            set
            {
                dataItems = value;
                PopulateView();
            }
        }

        private void PopulateView()
        {
            for (int i = 0; i < dataItems.Length; i++)
            {
                // if the list item does not exist already
                if (i >= listItems.Count)
                {
                    listItems.Add(GenerateListItem());
                }
                // else if the object existed but is destroyed
                else if (listItems[i] == null)
                {
                    listItems[i] = GenerateListItem();
                }

                // now we have an item and can use it
                listItems[i].SetUp(this, i);
                listItems[i].UpdateView();
            }

            // destroy all other listItems
            for (int i = listItems.Count - 1; i >= dataItems.Length; i--)
            {
                Destroy(listItems[i].gameObject);
                listItems.RemoveAt(i);
            }

            alignmentComponent.UpdateCollection();
            scrollingCollection.UpdateContent();
            if (scrollingCollection.FirstVisibleCellIndex > dataItems.Length)
            {
                scrollingCollection.MoveToIndex(dataItems.Length - 1);
            }
        }

        private ModelListItemBehaviour GenerateListItem()
        {
            return Instantiate(listItemPrefab, parentContainer).GetComponent<ModelListItemBehaviour>();
        }

        public void Select(int index)
        {
            ModelListItemSelected?.Invoke(index);
        }
    }
}