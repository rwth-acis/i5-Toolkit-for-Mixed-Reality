﻿using UnityEngine;


public class EnvironmentData : IListViewItemData
{
    public string EnvironmentName { get; private set; }
    public Sprite EnvironmentPreviewImage { get; private set; }
    public Material EnvironmentBackground { get; private set; }
    public GameObject EnvironmentPrefab { get; private set; }
    public string EnvironmentCredit { get; private set; }
    public string EnvironmentURL { get; private set; }

    public EnvironmentData(string name, Sprite previewImage, Material background, GameObject prefab, string credit, string url)
    {
        EnvironmentName = name;
        EnvironmentPreviewImage = previewImage;
        EnvironmentBackground = background;
        EnvironmentPrefab = prefab;
        EnvironmentCredit = credit;
        EnvironmentURL = url;
    }
}