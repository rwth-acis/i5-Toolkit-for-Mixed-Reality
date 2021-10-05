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
        /// Lists of asset bundles that are to be loaded. Contains information on the loading URL and bundle name from both the server and local disk.
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
        private EnvironmentData currentEnvironment;
        private bool defaultEnvironmentExists;


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
                    defaultEnvironmentExists = true;
                }
                else
                {
                    Debug.Log("No default virtual environment has been added. For a default virtual environment to be added at least a preview image should be given.");
                    defaultEnvironmentExists = false;
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
        /// Using a Web-Request, the asset bundles referenced in the serverEnvironmentsFromInspector list are loaded containing the skybox material, the 3D model as prefab, the preview image as sprite and the credits of the creator for all used assets. This environment data is added to the list of environment.
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

                string url = serverLoadingBaseURL + "/" + serverEnvironmentsFromInspector[arrayIndex].LoadingPath;
                var request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(url, 0);
                request.timeout = 15;
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
        /// Using a Web-Request, the asset bundles referenced in the localEnvironmentsFromInspector list are loaded from the disk containing the skybox material, the 3D model as prefab, the preview image as sprite and the credits of the creator for all used assets. This environment data is added to the list of environment.
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

                string url = "file:///" + localLoadingBasePath + "/" + localEnvironmentsFromInspector[arrayIndex].LoadingPath;
                var request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(url, 0);
                request.timeout = 15;
                AsyncOperation sentRequest = request.SendWebRequest();
                while (!sentRequest.isDone)
                {}

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
                    Debug.Log("Unable to fetch AssetBundle at index " + arrayIndex + " of the local item list!");
                }

                if (loadedPreviewImage != null && (loadedSkybox != null || loadedModel != null))
                {
                    environments.Add(new EnvironmentData(name, loadedPreviewImage, loadedSkybox, loadedModel, loadedCredits, localEnvironmentsFromInspector[arrayIndex].LoadingPath));
                }
                else
                {
                    Debug.Log("Unable to add the virtual environment from the AssetBundle at index " + arrayIndex + " of the local item list. Please make sure, that at least a skybox material or a 3D model as prefab and a preview image is contained in the bundle.");
                }
            }
            yield return null;
        }


        /// <summary>
        /// Instantiates the selected virtual environment after removing the currently active environment. Returns the currently active environment.
        /// </summary>
        /// <param name="selectedEnvironment">The EnvironmentData Item that should be instantiated.</param>
        public EnvironmentData InstantiateEnvironment(EnvironmentData selectedEnvironment)
        {
            if ((selectedEnvironment != null))
            {
                if (currentEnvironmentInstance != null)
                {
                    Destroy(currentEnvironmentInstance);
                    Debug.Log("The currently active virtual environment is being changed.");
                }

                if (selectedEnvironment.EnvironmentBackground != null)
                {
                    RenderSettings.skybox = selectedEnvironment.EnvironmentBackground;
                    currentEnvironment = selectedEnvironment;
                }

                if (selectedEnvironment.EnvironmentPrefab != null)
                {
                    currentEnvironmentInstance = Instantiate(selectedEnvironment.EnvironmentPrefab, selectedEnvironment.EnvironmentPrefab.transform.position, selectedEnvironment.EnvironmentPrefab.transform.rotation);
                }
            }
            return currentEnvironment;
        }


        /// <summary>
        /// Returns the list of environments that have been added from the inspector.
        /// </summary>
        public List<EnvironmentData> GetAllEnvironments()
        {
            return environments;
        }


        /// <summary>
        /// Returns the instantiated 3D model of the currently active virtual environment.
        /// </summary>
        public GameObject GetCurrent3DModelInstance()
        {
            return currentEnvironmentInstance;
        }


        /// <summary>
        /// Returns the data of the currently active virtual environment.
        /// </summary>
        public EnvironmentData GetCurrentEnvironmentData()
        {
            return currentEnvironment;
        }


        /// <summary>
        /// Allows the world space position of the currently instantiated 3D model to be set and changed, respectively.
        /// </summary>
        /// <param name="position"> The world space position the currently instantiated 3D model should be changed to.</param>
        public void SetCurrent3DModelPosition(Vector3 position)
        {
            if(currentEnvironmentInstance != null)
            {
                currentEnvironmentInstance.transform.position = position;
            }
            else
            {
                Debug.Log("Currently, no 3D model is instantiated!");
            }
        }


        /// <summary>
        /// Allows the local scale of the currently instantiated 3D model to be set and changed, respectively.
        /// </summary>
        /// <param name="scale"> The local scale the currently instantiated 3D model should be changed to.</param>
        public void SetCurrent3DModelScale(Vector3 scale)
        {
            if (currentEnvironmentInstance != null)
            {
                currentEnvironmentInstance.transform.localScale = scale;
            }
            else
            {
                Debug.Log("Currently, no 3D model is instantiated!");
            }
        }


        /// <summary>
        /// Allows the local rotation of the currently instantiated 3D model to be set and changed, respectively.
        /// </summary>
        /// <param name="rotation"> The local rotation the currently instantiated 3D model should be changed to.</param>
        public void SetCurrent3DModelRotation(Quaternion rotation)
        {
            if (currentEnvironmentInstance != null)
            {
                currentEnvironmentInstance.transform.localRotation = rotation;
            }
            else
            {
                Debug.Log("Currently, no 3D model is instantiated!");
            }
        }


        /// <summary>
        /// Sets the current environment to the default environment.
        /// </summary>
        public void ReturnToDefaultEnvironment()
        {
            if (defaultEnvironmentExists)
            {
                InstantiateEnvironment(environments[0]);
            }
            else
            {
                Debug.Log("No default virtual environment has been added. Returning to it is not possible.");
            }
        }


        /// <summary>
        /// Removes the environment at an index in the list and returns the new list.
        /// </summary>
        /// <param name="index"> Index of the item that is to be removed.</param>
        public List<EnvironmentData> RemoveEnvironmentAt(int index)
        {
            if (index >= 0 && index < environments.Count)
            {
                environments.RemoveAt(index);
            }
            else
            {
                Debug.Log("The given index is invalid. No environment has been deleted");
            }
            return environments;
        }


        /// <summary>
        /// Removes a given environment from the environment list and returns the new list.
        /// </summary>
        /// <param name="environment"> Environment item that should be removed.</param>
        public List<EnvironmentData> RemoveEnvironment(EnvironmentData environment)
        {
            environments.Remove(environment);
            return environments;
        }


        /// <summary>
        /// Reloads all items from the server that a given in the inspector.
        /// </summary>
        public void ReloadFromServer()
        {
            GetAssetBundleObjectsFromServer();
        }


        /// <summary>
        /// Reloads all items from the local disc that a given in the inspector.
        /// </summary>
        public void ReloadFromLocal()
        {
            GetAssetBundleObjectsFromLocal();
        }
    }
}