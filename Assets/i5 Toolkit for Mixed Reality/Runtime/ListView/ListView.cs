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

    public int CellsPerTier { get => scrollingObjectCollection.CellsPerTier; }

    public int TiersPerPage { get => scrollingObjectCollection.TiersPerPage; }


    private void Awake()
    {
        scrollingObjectCollection = GetComponent<ScrollingObjectCollection>();
        scrollEvents = GetComponent<ScrollingObjectCollectionExtension>();
        gridCollection = GetComponentInChildren<GridObjectCollection>();
        itemContainers = GetComponentsInChildren<ListItem>(true);
    }

    private void Start()
    {
        scrollEvents.OnScrollingUpdate.AddListener(OnScrollingUpdate);
        gridCollection.transform.localPosition = new Vector3(0, scrollingObjectCollection.CellHeight, 0);
        InitializeItems();
    }

    private void InitializeItems()
    {
        for (int i = 0; i < itemContainers.Length; i++)
        {
            itemContainers[i].SetUp(this, i - CellsPerTier);
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
            for (int firstVisibleIndex = lastFirstVisible + CellsPerTier; firstVisibleIndex <= scrollingObjectCollection.FirstVisibleCellIndex; firstVisibleIndex += CellsPerTier)
            {
                int startIndex = (firstVisibleIndex - CellsPerTier) % itemContainers.Length;
                if (startIndex < 0)
                {
                    return;
                }

                Debug.Log("Scrolling down");

                for (int i = 0; i < CellsPerTier; i++)
                {
                    itemContainers[startIndex + i].transform.SetAsLastSibling();
                    itemContainers[startIndex + i].SetUp(this, firstVisibleIndex + (CellsPerTier * TiersPerPage) + i);
                }
                gridCollection.transform.localPosition -= new Vector3(0, scrollingObjectCollection.CellHeight, 0);
            }
        }
        // scrolling up
        else
        {
            for (int firstVisibleIndex = lastFirstVisible - CellsPerTier; firstVisibleIndex >= scrollingObjectCollection.FirstVisibleCellIndex; firstVisibleIndex -= CellsPerTier)
            {

                int endIndex = (scrollingObjectCollection.FirstVisibleCellIndex + (CellsPerTier - 1)) % itemContainers.Length;

                Debug.Log("Scrolling up");

                for (int i = 0; i < CellsPerTier; i++)
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
}
