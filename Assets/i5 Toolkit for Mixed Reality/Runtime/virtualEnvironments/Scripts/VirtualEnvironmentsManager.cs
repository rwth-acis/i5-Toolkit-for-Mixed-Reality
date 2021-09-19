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
    /// <summary>
    /// Lists of asset bundles that are to be loaded. Contains information on the loading url and bundle name from both the server and local disk.
    /// </summary>
    public EnvironmentLoadingInformation[] serverEnvironmentsFromInspector;
    public EnvironmentLoadingInformation[] localEnvironmentsFromInspector;

    /// <summary>
    /// Values for a default environment. The default environment is given by a skybox and a 3D model as prefab, as well as a preview image and credits.
    /// </summary>
    public Material defaultSkybox;
    public GameObject defaultModel;
    public Sprite defaultPreviewImage;
    public string defaultCredits;

    private bool coroutinesFinished = false;

    /// <summary>
    /// The base URL for loading from a server and the base path to the local folder, respectively.
    /// </summary>
    public string serverLoadingBaseURL;
    public string localLoadingBasePath;

    /// <summary>
    /// The final environment list
    /// </summary>
    public List<EnvironmentData> environments = new List<EnvironmentData>();


   
    /// <summary>
    /// Creates the environments list by first adding the default item and then loading the asset bundles from the server and the local disc, respectively.
    /// </summary>
    private void Awake()
    {
        if (UnityEngine.XR.WSA.HolographicSettings.IsDisplayOpaque)
            
            if(defaultPreviewImage != null)
            {
                environments.Add(new EnvironmentData("Default", defaultPreviewImage, defaultSkybox, defaultModel, defaultCredits, "");
            }

            if (serverEnvironmentsFromInspector.Length > 0)
            {
                StartCoroutine(GetAssetBundleObjectsFromServer());
            }

            if (localEnvironmentsFromInspector.Length > 0)
            {
                StartCoroutine(GetAssetBundleObjectsFromLocal());
            }
    }


    /// <summary>
    /// Using a Web-Request, the asset bundles referenced in the serverEnvironmentsFromInspector list are loaded containing the skybox material, the 3D model as prefab, the preview image as sprite anf the credits of the creator for all used assets. This environment data is added to the list of environment.
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
    /// Using a Web-Request, the asset bundles referenced in the localEnvironmentsFromInspector list are loaded from the disk containing the skybox material, the 3D model as prefab, the preview image as sprite anf the credits of the creator for all used assets. This environment data is added to the list of environment.
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