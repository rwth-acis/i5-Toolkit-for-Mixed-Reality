using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using i5.Toolkit.Core.Utilities;
using UnityEngine.UI;

namespace VirtualEnvironments
{
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
        private GameObject currentEnvironmentInstance;


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

                if (defaultPreviewImage != null)
                {
                    environments.Add(new EnvironmentData("Default", defaultPreviewImage, defaultSkybox, defaultModel, defaultCredits, ""));
                    Debug.Log("Added the default virtual environment.");
                }
                else
                {
                    Debug.Log("No default virtual environment has been added. For a default virtual environment to be added at least a preview image should be given.");
                }

            if (serverEnvironmentsFromInspector.Length > 0)
            {
                StartCoroutine(GetAssetBundleObjectsFromServer());
            }
            else
            {
                Debug.Log("No environment bundles are being fetched from a server.");
            }

            if (localEnvironmentsFromInspector.Length > 0)
            {
                StartCoroutine(GetAssetBundleObjectsFromLocal());
            }
            else
            {
                Debug.Log("No environment bundles are being fetched from the local disk.");
            }
            Debug.Log(environments.Count + " environments have been added.");
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
                        {
                            loadedModel = bundle.LoadAllAssets<GameObject>()[0];
                        }
                        else
                        {
                            Debug.Log("AssetBundle " + name + " did not contain a 3D model as prefab. Loading AssetBundle without it.");
                        }

                        loadedPreviewImage = bundle.LoadAllAssets<Sprite>()[0];
                        loadedCredits = bundle.LoadAllAssets<TextAsset>()[0].text;
                    }
                    else
                    {
                        Debug.Log("Unable to fetch AssetBundle at index " + arrayIndex + " of the server item list!");
                    }
                }
                else
                {
                    Debug.Log("Unable to fetch AssetBundle at index " + arrayIndex + " of the server item list!");
                }

                if (loadedPreviewImage != null && loadedSkybox != null)
                {
                    environments.Add(new EnvironmentData(name, loadedPreviewImage, loadedSkybox, loadedModel, loadedCredits, serverEnvironmentsFromInspector[arrayIndex].LoadingPath));
                }
                else
                {
                    Debug.Log("Unable to add the virtual environment form the AssetBundle at index " + arrayIndex + " of the server item list. Please make sure, that at least a skybox material and a preview image is contained in the bundle.");
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
                        name = localEnvironmentsFromInspector[arrayIndex].Name;
                        loadedSkybox = bundle.LoadAllAssets<Material>()[0];

                        if (bundle.LoadAllAssets<GameObject>().Length != 0)
                        {
                            loadedModel = bundle.LoadAllAssets<GameObject>()[0];
                        }
                        else
                        {
                            Debug.Log("AssetBundle " + name + " did not contain a 3D model as prefab. Loading AssetBundle without it.");
                        }

                        loadedPreviewImage = bundle.LoadAllAssets<Sprite>()[0];
                        loadedCredits = bundle.LoadAllAssets<TextAsset>()[0].text;
                    }
                    else
                    {
                        Debug.Log("Unable to fetch AssetBundle at index " + arrayIndex + "of the local item list!");
                    }
                }
                else
                {
                    Debug.Log("Unable to fetch AssetBundle at index " + arrayIndex + "of the local item list!");
                }

                if (loadedPreviewImage != null && (loadedSkybox != null || loadedModel != null))
                {
                    environments.Add(new EnvironmentData(name, loadedPreviewImage, loadedSkybox, loadedModel, loadedCredits, localEnvironmentsFromInspector[arrayIndex].LoadingPath));
                }
                else
                {
                    Debug.Log("Unable to add the virtual environment form the AssetBundle at index " + arrayIndex + "of the local item list. Please make sure, that at least a skybox material or a 3D model as prefab and a preview image is contained in the bundle.");
                }
            }
            yield return null;
        }

        /// <summary>
        /// Instantiates the selected virtual environment after removing the currently active environment. Returns the current instance of the 3D model as prefab.
        /// </summary>
        /// <param name="selectedEnvironment">The EnvironmentData Item that should be instantiated</param>

        public GameObject InstantiateEnvironment(EnvironmentData selectedEnvironment)
        {
            if ((selectedEnvironment != null))
            {
                if (currentEnvironmentInstance != null)
                {
                    Destroy(currentEnvironmentInstance);
                    Debug.Log("The currently active virtual environment is being removed.");
                }

                if (selectedEnvironment.EnvironmentBackground != null)
                {
                    RenderSettings.skybox = selectedEnvironment.EnvironmentBackground;
                }

                if (selectedEnvironment.EnvironmentPrefab != null)
                {
                    currentEnvironmentInstance = Instantiate(selectedEnvironment.EnvironmentPrefab, selectedEnvironment.EnvironmentPrefab.transform.position, selectedEnvironment.EnvironmentPrefab.transform.rotation);
                }
            }
            return currentEnvironmentInstance;
        }
    }
}