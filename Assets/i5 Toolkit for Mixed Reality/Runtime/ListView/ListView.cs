using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ListView<T> : MonoBehaviour
{
    private ScrollingObjectCollection scrollingObjectCollection;
    private ScrollingObjectCollectionExtension scrollEvents;
    private GridObjectCollection gridCollection;
    private ListItem<T>[] itemContainers;
    private int lastFirstVisible = 0;
    private DataSource<T> dataSource;

    private GameObject limiterObject;

    public int CellsPerTier { get => scrollingObjectCollection.CellsPerTier; }

    public int TiersPerPage { get => scrollingObjectCollection.TiersPerPage; }

    public DataSource<T> DataSource
    {
        get => dataSource;
        set
        {
            dataSource = value;
            ResetCollection();
        }
    }


    protected virtual void Awake()
    {
        scrollingObjectCollection = GetComponent<ScrollingObjectCollection>();
        scrollEvents = GetComponent<ScrollingObjectCollectionExtension>();
        gridCollection = GetComponentInChildren<GridObjectCollection>();
        itemContainers = GetComponentsInChildren<ListItem<T>>(true);

        limiterObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        limiterObject.transform.localScale = 0.01f * Vector3.one;
        limiterObject.transform.parent = gridCollection.transform.parent;
        limiterObject.transform.localPosition = new Vector3(0, -500, 0);
    }

    private void Start()
    {
        scrollEvents.OnScrollingUpdate.AddListener(OnScrollingUpdate);
        gridCollection.transform.localPosition = new Vector3(0, scrollingObjectCollection.CellHeight, 0);
        InitializeItems();
        gridCollection.UpdateCollection();
        scrollingObjectCollection.UpdateContent();
    }

    private void InitializeItems()
    {
        for (int i = 0; i < itemContainers.Length; i++)
        {
            int index = i - CellsPerTier;
            itemContainers[i].SetUp(this, index);
        }

        UpdateLimiter();
    }

    private void ResetCollection()
    {
        scrollingObjectCollection.MoveToIndex(0, false);
        InitializeItems();
    }

    private void UpdateLimiter()
    {
        if (dataSource != null)
        {
            // infinite list
            if (dataSource.Length < 0)
            {
                SetLimiter(scrollingObjectCollection.FirstHiddenCellIndex + 10 * scrollingObjectCollection.CellsPerTier);
            }
            // limited list
            else
            {
                SetLimiter(dataSource.Length);
            }
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

                for (int i = 0; i < CellsPerTier; i++)
                {
                    int dataIndex = firstVisibleIndex + (CellsPerTier * TiersPerPage) + i;
                    itemContainers[startIndex + i].transform.SetAsLastSibling();
                    itemContainers[startIndex + i].SetUp(this, firstVisibleIndex + (CellsPerTier * TiersPerPage) + i);
                }
                gridCollection.transform.localPosition -= new Vector3(0, scrollingObjectCollection.CellHeight, 0);
            }

            UpdateLimiter();
        }
        // scrolling up
        else
        {
            for (int firstVisibleIndex = lastFirstVisible - CellsPerTier; firstVisibleIndex >= scrollingObjectCollection.FirstVisibleCellIndex; firstVisibleIndex -= CellsPerTier)
            {

                int endIndex = (scrollingObjectCollection.FirstVisibleCellIndex + (CellsPerTier - 1)) % itemContainers.Length;

                for (int i = 0; i < CellsPerTier; i++)
                {
                    int dataIndex = scrollingObjectCollection.FirstVisibleCellIndex - i - 1;
                    itemContainers[endIndex - i].transform.SetAsFirstSibling();
                    itemContainers[endIndex - i].SetUp(this, dataIndex);
                }
                gridCollection.transform.localPosition += new Vector3(0, scrollingObjectCollection.CellHeight, 0);
            }
        }

        lastFirstVisible = scrollingObjectCollection.FirstVisibleCellIndex;

        gridCollection.UpdateCollection();
        scrollingObjectCollection.UpdateContent();
        Debug.Log(scrollingObjectCollection.GetType().GetProperty("MaxY", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(scrollingObjectCollection));
    }

    private void SetLimiter(int index)
    {
        int rows = (scrollingObjectCollection.FirstVisibleCellIndex + index) / scrollingObjectCollection.CellsPerTier;

        float depth = rows * scrollingObjectCollection.CellHeight;

        limiterObject.transform.localPosition = new Vector3(0, -depth, 0);
    }
}
