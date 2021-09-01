﻿using i5.Toolkit.Core.Utilities;
using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main controller of the list view
/// Takes items of an DataType and transforms them to GameObject instances.
/// The ItemType is a class which can apply the DataType to the GameObject's representation, e.g. fill the text meshes
/// </summary>
/// <typeparam name="DataType">The DataType of the list</typeparam>
/// <typeparam name="ItemType">The class which converts a DataType to a GameObject representation</typeparam>
public class ListViewController<DataType, ItemType> : MonoBehaviour, IListViewController
    where DataType : IListViewItemData
    where ItemType : ListViewItem<DataType>
{
    [Header("Display Options")]
    [SerializeField] protected GameObject displayPreviewPrefab;
    [SerializeField] protected Vector3 displayOffset;
    [Range(1, 5)] public int entriesPerPage;
    [SerializeField] protected List<DataType> items = new List<DataType>();

    [Header("Navigation Buttons")]
    [SerializeField] public Interactable pageUpButton;
    [SerializeField] public Interactable pageDownButton;

    public event EventHandler<ListViewItemSelectedArgs> ItemSelected;

    protected List<ItemType> instances;

    public List<DataType> Items
    {
        get => items;
        set
        {
            items = value;
            RemoveInstances();
            CreateInstances();
            SelectedItemIndex = 0;
        }
    }

    public GameObject ItemPrefab
    {
        get => displayPreviewPrefab;
    }

    public int SelectedItemIndex { get; set; }

    public DataType SeletedItem { get { return items[SelectedItemIndex]; } }

    private void Awake()
    {
        instances = new List<ItemType>();
        if (displayPreviewPrefab == null)
        {
            SpecialDebugMessages.LogMissingReferenceError(this, nameof(displayPreviewPrefab));
        }
        else
        {
            if (displayPreviewPrefab.GetComponent<ItemType>() == null)
            {
                SpecialDebugMessages.LogComponentNotFoundError(this, nameof(ItemType), displayPreviewPrefab);
            }
        }
        if (Items == null)
        {
            Items = new List<DataType>();
        }
    }

    protected virtual void CreateInstances()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            ItemType instanceAdapter = Instantiate(displayPreviewPrefab, transform).GetComponent<ItemType>();
            if (instanceAdapter == null)
            {
                SpecialDebugMessages.LogComponentNotFoundError(this, nameof(ItemType), displayPreviewPrefab);
            }
            else
            {
                instanceAdapter.Setup(items[i], i, this);
            }
        }
    }

    protected virtual void RemoveInstances()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void OnItemSelected(int index)
    {
        SelectedItemIndex = index;
        EventHandler<ListViewItemSelectedArgs> handler = ItemSelected;
        if (handler != null)
        {
            ListViewItemSelectedArgs args = new ListViewItemSelectedArgs();
            args.SelectedItemIndex = index;
            handler(this, args);
        }
    }

    public void Clear()
    {
        Items = new List<DataType>();
    }


    /// <summary>
    /// Aligns the child objects of this transform
    /// </summary>
    private void Update()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).localPosition = displayOffset * i;
        }
    }

    /// <summary>
    /// Updates the alignment in edit mode if the offset is changed
    /// </summary>
    private void OnValidate()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).localPosition = displayOffset * i;
        }
    }
}

public class ListViewController : ListViewController<ListViewItemInspectorData, ListViewItem>
{
}

public class ListViewItemSelectedArgs : EventArgs
{
    public int SelectedItemIndex { get; set; }
}