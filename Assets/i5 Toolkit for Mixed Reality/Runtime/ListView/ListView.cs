using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ListView : MonoBehaviour
{
    private ScrollingObjectCollection scrollingObjectCollection;
    private ScrollingObjectCollectionExtension scrollEvents;
    private GridObjectCollection gridCollection;
    private ListItem[] itemContainers;
    private int lastFirstVisible = 0;

    // Start is called before the first frame update
    void Start()
    {
        scrollingObjectCollection = GetComponent<ScrollingObjectCollection>();
        scrollEvents = GetComponent<ScrollingObjectCollectionExtension>();
        scrollEvents.OnScrollingUpdate.AddListener(OnScrollingUpdate);
        gridCollection = GetComponentInChildren<GridObjectCollection>();
        itemContainers = GetComponentsInChildren<ListItem>(true);
        InitializeItems();
    }

    private void InitializeItems()
    {
        for (int i = 0; i < itemContainers.Length; i++)
        {
            itemContainers[i].SetUp(this, i - 3);
        }
    }

    private void OnScrollingUpdate()
    {
        if (scrollingObjectCollection.FirstVisibleCellIndex == lastFirstVisible)
        {
            return;
        }
        // scrolling down
        else if (scrollingObjectCollection.FirstVisibleCellIndex > lastFirstVisible)
        {
            for (int firstVisibleIndex = lastFirstVisible + 3; firstVisibleIndex <= scrollingObjectCollection.FirstVisibleCellIndex; firstVisibleIndex += 3)
            {
                int startIndex = (firstVisibleIndex - 3) % itemContainers.Length;
                if (startIndex < 0)
                {
                    return;
                }

                Debug.Log("Scrolling down");

                for (int i = 0; i < 3; i++)
                {
                    itemContainers[startIndex + i].transform.SetAsLastSibling();
                    itemContainers[startIndex + i].SetUp(this, firstVisibleIndex + 9 + i);
                }
                gridCollection.transform.localPosition -= new Vector3(0, scrollingObjectCollection.CellHeight, 0);
            }
        }
        // scrolling up
        else
        {
            for (int firstVisibleIndex = lastFirstVisible - 3; firstVisibleIndex >= scrollingObjectCollection.FirstVisibleCellIndex; firstVisibleIndex -= 3)
            {

                int endIndex = (scrollingObjectCollection.FirstVisibleCellIndex + 2) % itemContainers.Length;

                Debug.Log("Scrolling up");

                for (int i = 0; i < 3; i++)
                {
                    itemContainers[endIndex - i].transform.SetAsFirstSibling();
                    itemContainers[endIndex - i].SetUp(this, scrollingObjectCollection.FirstVisibleCellIndex - i - 1);
                }
                gridCollection.transform.localPosition += new Vector3(0, scrollingObjectCollection.CellHeight, 0);
            }
        }

        lastFirstVisible = scrollingObjectCollection.FirstVisibleCellIndex;

        gridCollection.UpdateCollection();
        scrollingObjectCollection.UpdateContent();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("First visible: " + scrollingObjectCollection.FirstVisibleCellIndex);
        }
    }
}
