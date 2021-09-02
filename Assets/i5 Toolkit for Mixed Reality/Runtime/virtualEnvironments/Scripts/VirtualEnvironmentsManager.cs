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
    [Space(10)]
    [SerializeField] private EnvironmentsDisplayManager environmentsDisplayManager;
    public EnvironmentLoadingInformation[] serverEnvironmentsFromInspector;
    public EnvironmentLoadingInformation[] localEnvironmentsFromInspector;

    [Space(10)]
    [Tooltip("The values of the default virtual environment. These should be equal to the initial values, such that the user can return to the standard settings.")]
    [Header("Default Virtual Environment Values")]
    public Material defaultSkybox;
    public GameObject defaultModel;
    public Sprite defaultPreviewImage;
    public string defaultCredits;


    private Material currentSkybox;
    private GameObject currentPrefab;
    private bool coroutinesFinished = false;
    private string assetBundlesURL;

    private List<EnvironmentData> environments = new List<EnvironmentData>();

    private int page = 0;
    private bool windowEnabled = true;
    private GameObject currentEnvironmentInstance;

    /// <summary>
    /// States whether the window is enabled
    /// If set to false, the window will remain visible but all interactable controls are disabled
    /// </summary>
    /// <value></value>
    public bool WindowEnabled
    {
        get
        {
            return windowEnabled;
        }
        set
        {
            windowEnabled = value;
            environmentsDisplayManager.pageUpButton.Enabled = value;
            environmentsDisplayManager.pageDownButton.Enabled = value;
        }
    }

    public bool WindowOpen
    {
        get; private set;
    }

    /// <summary>
    /// Event which is invoked if the window is closed
    /// </summary>
    public event EventHandler WindowClosed;

    /// <summary>
    /// Initializes the component, makes sure that it is set up correctly
    /// </summary>
    private void Awake()
    {
        if (environmentsDisplayManager.pageUpButton == null)
        {
            SpecialDebugMessages.LogMissingReferenceError(this, nameof(environmentsDisplayManager.pageUpButton));
        }
        if (environmentsDisplayManager.pageDownButton == null)
        {
            SpecialDebugMessages.LogMissingReferenceError(this, nameof(environmentsDisplayManager.pageDownButton));
        }


        assetBundlesURL = "file:///" + Application.dataPath + "/AssetBundles/";
        if (UnityEngine.XR.WSA.HolographicSettings.IsDisplayOpaque)
            //TODO add this, if default environment is added from inspector 
            //environments.Add(new EnvironmentData("Default", defaultPreviewImage, defaultSkybox, defaultModel, defaultCredits, "");
            StartCoroutine(GetAssetBundleObjectsFromServer());
            StartCoroutine(GetAssetBundleObjectsFromLocal());

        environmentsDisplayManager.ItemSelected += OnEnvironmentSelected;

        UpdateEnvironmentDisplay();
        SetPageButtonStates();
    }

    /// <summary>
    /// Called if a element of the room list view was selected by the user
    /// </summary>
    /// <param name="skybox">The selected skybox</param>
    /// <param name="e">Arguments about the list view selection event</param>
    private void OnEnvironmentSelected(object sender, ListViewItemSelectedArgs e)
    {
        if ((environmentsDisplayManager.SeletedItem != null) && windowEnabled)
        {
            if (environmentsDisplayManager.SeletedItem.EnvironmentBackground != null)
            {
                RenderSettings.skybox = environmentsDisplayManager.SeletedItem.EnvironmentBackground;
            }
            if (currentEnvironmentInstance != null)
            {
                Destroy(currentEnvironmentInstance);
            }
            if (environmentsDisplayManager.SeletedItem.EnvironmentPrefab != null)
            {
                currentEnvironmentInstance = Instantiate(environmentsDisplayManager.SeletedItem.EnvironmentPrefab, environmentsDisplayManager.SeletedItem.EnvironmentPrefab.transform.position, environmentsDisplayManager.SeletedItem.EnvironmentPrefab.transform.rotation);
            }
        }
    }

    /// <summary>
    /// Called if the user pushes the page up button
    /// Swiches to the previous page
    /// </summary>
    public void PageUp()
    {
        page = Mathf.Max(0, page - 1);
        SetPageButtonStates();
        UpdateEnvironmentDisplay();
    }

    /// <summary>
    /// Called if the user pages the page down button
    /// Switches to the next page
    /// </summary>
    public void PageDown()
    {
        page = Mathf.Min(page + 1, ((environments.Count - 1) / environmentsDisplayManager.entriesPerPage));
        SetPageButtonStates();
        UpdateEnvironmentDisplay();
    }


    /// <summary>
    /// Adapts the button states of the page up and page down buttons
    /// If the first page is shown, the up button is disabled and if the last page is shown, the down button is disabled
    /// </summary>
    private void SetPageButtonStates()
    {
        if (page == 0) // first page
        {
            environmentsDisplayManager.pageUpButton.Enabled = false;
        }
        else
        {
            environmentsDisplayManager.pageUpButton.Enabled = true;
        }

        if (page == ((environments.Count - 1) / environmentsDisplayManager.entriesPerPage)) // last page
        {
            environmentsDisplayManager.pageDownButton.Enabled = false;
        }
        else
        {
            environmentsDisplayManager.pageDownButton.Enabled = true;
        }
    }


    /// <summary>
    /// Updates the list view showing the environment lists (on the current page)
    /// </summary>
    private void UpdateEnvironmentDisplay()
    {
        if (environments.Count > 0)
        {
            // get the start index and length of the sub array to display
            // make sure that it stays within the bounds of the room list
            int startIndex = Mathf.Min(page * environmentsDisplayManager.entriesPerPage, environments.Count - 1);
            int length = Mathf.Min(environments.Count - startIndex, environmentsDisplayManager.entriesPerPage);
            environmentsDisplayManager.Items = environments.GetRange(startIndex, length);
        }
        else
        {
            environmentsDisplayManager.Items = new List<EnvironmentData>();
        }
    }

 
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
                //TODO set to url variable
                string url = assetBundlesURL + serverEnvironmentsFromInspector[arrayIndex].LoadingPath;
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
                //TODO set to path variable
                string url = assetBundlesURL + localEnvironmentsFromInspector[arrayIndex].LoadingPath;
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