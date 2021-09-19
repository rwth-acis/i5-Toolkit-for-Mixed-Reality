using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using i5.Toolkit.Core.Utilities;
using UnityEngine.UI;

/// <summary>
/// Controls the menu which allows a user to select existing rooms (or navigate to the menu where a new room can be created)
/// </summary>
public class VirtualEnvironmentsManager : MonoBehaviour
{

    public EnvironmentLoadingInformation[] serverEnvironmentsFromInspector;
    public EnvironmentLoadingInformation[] localEnvironmentsFromInspector;

    public Material defaultSkybox;
    public GameObject defaultModel;
    public Sprite defaultPreviewImage;
    public string defaultCredits;

    private bool coroutinesFinished = false;

    public string serverLoadingBaseURL;
    public string localLoadingBasePath;

    public List<EnvironmentData> environments = new List<EnvironmentData>();


   
    /// <summary>
    /// TODO
    /// </summary>
    private void Awake()
    {
        if (UnityEngine.XR.WSA.HolographicSettings.IsDisplayOpaque)
            //TODO add this, if default environment is added from inspector 
            //environments.Add(new EnvironmentData("Default", defaultPreviewImage, defaultSkybox, defaultModel, defaultCredits, "");
            StartCoroutine(GetAssetBundleObjectsFromServer());
            StartCoroutine(GetAssetBundleObjectsFromLocal());
    }


    /// <summary>
    /// TODO
    /// </summary>
    IEnumerator GetAssetBundleObjectsFromServer()
    {
        for (int arrayIndex = 0; arrayIndex < serverEnvironmentsFromInspector.Length; arrayIndex++)
        {
            string name = null;
            Material loadedSkybox = null;
            GameObject loadedModel = null;
            Sprite loadedPreviewImage = null;
            string loadedCredits = null;

            if (arrayIndex != 0)
            {
                string url = serverLoadingBaseURL + serverEnvironmentsFromInspector[arrayIndex].LoadingPath;
                var request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(url, 0);

                AsyncOperation sentRequest = request.SendWebRequest();
                while (!sentRequest.isDone)
                { }

                if (UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request) != null)
                {
                    AssetBundle bundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
                    if (bundle != null)
                    {
                        name = serverEnvironmentsFromInspector[arrayIndex].Name;
                        loadedSkybox = bundle.LoadAllAssets<Material>()[0];
                        if (bundle.LoadAllAssets<GameObject>().Length != 0)
                            loadedModel = bundle.LoadAllAssets<GameObject>()[0];
                        loadedPreviewImage = bundle.LoadAllAssets<Sprite>()[0];
                        loadedCredits = bundle.LoadAllAssets<TextAsset>()[0].text;
                    }
                }
            }

            if (loadedPreviewImage != null && loadedSkybox != null)
            {
                environments.Add(new EnvironmentData(name, loadedPreviewImage, loadedSkybox, loadedModel, loadedCredits, serverEnvironmentsFromInspector[arrayIndex].LoadingPath));
            }
        }
        yield return null;
    }

    /// <summary>
    /// TODO
    /// </summary>
    IEnumerator GetAssetBundleObjectsFromLocal()
    {
        for (int arrayIndex = 0; arrayIndex < localEnvironmentsFromInspector.Length; arrayIndex++)
        {
            string name = null;
            Material loadedSkybox = null;
            GameObject loadedModel = null;
            Sprite loadedPreviewImage = null;
            string loadedCredits = null;

            if (arrayIndex != 0)
            {
                string url = "file:///" + localLoadingBasePath + localEnvironmentsFromInspector[arrayIndex].LoadingPath;
                var request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(url, 0);

                AsyncOperation sentRequest = request.SendWebRequest();
                while (!sentRequest.isDone)
                { }

                if (UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request) != null)
                {
                    AssetBundle bundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
                    if (bundle != null)
                    {
                        name = serverEnvironmentsFromInspector[arrayIndex].Name;
                        loadedSkybox = bundle.LoadAllAssets<Material>()[0];
                        if (bundle.LoadAllAssets<GameObject>().Length != 0)
                            loadedModel = bundle.LoadAllAssets<GameObject>()[0];
                        loadedPreviewImage = bundle.LoadAllAssets<Sprite>()[0];
                        loadedCredits = bundle.LoadAllAssets<TextAsset>()[0].text;
                    }
                }
            }

            if (loadedPreviewImage != null && loadedSkybox != null)
            {
                environments.Add(new EnvironmentData(name, loadedPreviewImage, loadedSkybox, loadedModel, loadedCredits, serverEnvironmentsFromInspector[arrayIndex].LoadingPath));
            }
        }
        yield return null;
    }
}