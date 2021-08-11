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
    private ScrollingObjectCollectionEvents scrollEvents;
    private GridObjectCollection gridCollection;
    private ListItem[] listItems;
    private int lastStartIndex = 1;

    // Start is called before the first frame update
    void Start()
    {
        scrollingObjectCollection = GetComponent<ScrollingObjectCollection>();
        scrollEvents = GetComponent<ScrollingObjectCollectionEvents>();
        scrollEvents.OnScrollingUpdate.AddListener(OnScrollingUpdate);
        gridCollection = GetComponentInChildren<GridObjectCollection>();
        listItems = GetComponentsInChildren<ListItem>(true);
        InitializeItems();
    }

    private void InitializeItems()
    {
        for (int i = 0; i < listItems.Length; i++)
        {
            listItems[i].SetUp(this, i);
        }
    }

    private void OnScrollingUpdate()
    {
        Debug.Log("First visible: " + scrollingObjectCollection.FirstVisibleCellIndex);

        int startIndex = (scrollingObjectCollection.FirstVisibleCellIndex - 9) % listItems.Length;
        if (startIndex < 0 || startIndex == lastStartIndex)
        {
            return;
        }
        // scrolling down
        else if (startIndex > lastStartIndex)
        {
            for (int i = 0; i < 3; i++)
            {
                listItems[startIndex + i].transform.SetAsLastSibling();
                listItems[startIndex + i].SetUp(this, scrollingObjectCollection.FirstVisibleCellIndex + i);
            }
        }
        // scrolling up
        else if (startIndex < lastStartIndex)
        {
            for (int i = 3; i >= 0; i--)
            {
                listItems[startIndex - i].transform.SetAsFirstSibling();
                listItems[startIndex - i].SetUp(this, scrollingObjectCollection.FirstVisibleCellIndex - i);
            }
        }

        gridCollection.UpdateCollection();
        gridCollection.transform.localPosition -= new Vector3(0, scrollingObjectCollection.CellHeight, 0);
        scrollingObjectCollection.UpdateContent();
        lastStartIndex = startIndex;
    }
}
