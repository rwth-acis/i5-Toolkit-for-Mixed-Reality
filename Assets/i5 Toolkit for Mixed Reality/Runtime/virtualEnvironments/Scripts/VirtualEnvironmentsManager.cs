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

    [Space(10)]
    [Header("Environments Display Name")]
    [Tooltip("The name of the virtual environment that should be displayed. Must be at least of size 1, where the first entry corresponds to the default choice.")]
    [SerializeField] private string[] environmentNames;

    [Space(10)]
    [Header("Environments Loading URL")]
    [Tooltip("The URL is used to fetch the virtual environment as an asset bundle from either a server or a local folder. Must be at least of size 1, where the first entry corresponds to the default choice. Note: Leave the first entry empty.")]
    [SerializeField] private string[] environmentURL;


    private GameObject[] environmentPrefabs;
    private string[] environmentCredits;
    private Material[] environmentSkyboxes;
    private Sprite[] previewImages;

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

        environmentPrefabs = new GameObject[environmentURL.Length];
        previewImages = new Sprite[environmentURL.Length];
        environmentSkyboxes = new Material[environmentURL.Length];
        environmentCredits = new string[environmentURL.Length];

        previewImages[0] = defaultPreviewImage;
        environmentSkyboxes[0] = defaultSkybox;
        environmentCredits[0] = defaultCredits;

        assetBundlesURL = "file:///" + Application.dataPath + "/AssetBundles/";
        if (UnityEngine.XR.WSA.HolographicSettings.IsDisplayOpaque)
            StartCoroutine(GetAssetBundleObjects());

        environmentsDisplayManager.ItemSelected += OnEnvironmentSelected;

        //Close();
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

    /// <summary>
    /// Opens the window
    /// </summary>
    public void Open()
    {
        gameObject.SetActive(true);
        WindowOpen = true;
        UpdateEnvironmentDisplay();
    }

    public void Open(Vector3 position, Vector3 eulerAngles)
    {
        Open();
        transform.localPosition = position;
        transform.localEulerAngles = eulerAngles;
    }

    /// <summary>
    /// Closes the window
    /// </summary>
    public void Close()
    {
        WindowOpen = false;
        WindowClosed?.Invoke(this, EventArgs.Empty);
        gameObject.SetActive(false);
    }

    IEnumerator GetAssetBundleObjects()
    {
        for (int arrayIndex = 0; arrayIndex < environmentURL.Length; arrayIndex++)
        {
            if (arrayIndex != 0)
            {
                currentSkybox = null;
                currentPrefab = null;
                string url = assetBundlesURL + environmentURL[arrayIndex];
                var request = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(url, 0);
                AsyncOperation sentRequest = request.SendWebRequest();
                while (!sentRequest.isDone)
                { }

                if (UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request) != null)
                {
                    AssetBundle bundle = UnityEngine.Networking.DownloadHandlerAssetBundle.GetContent(request);
                    if (bundle != null)
                    {
                        environmentSkyboxes[arrayIndex] = bundle.LoadAllAssets<Material>()[0];
                        if (bundle.LoadAllAssets<GameObject>().Length != 0)
                            environmentPrefabs[arrayIndex] = bundle.LoadAllAssets<GameObject>()[0];
                        previewImages[arrayIndex] = bundle.LoadAllAssets<Sprite>()[0];
                        environmentCredits[arrayIndex] = bundle.LoadAllAssets<TextAsset>()[0].text;
                    }
                }
            }

            if (previewImages[arrayIndex] != null && environmentSkyboxes[arrayIndex] != null)
            {
                environments.Add(new EnvironmentData(environmentNames[arrayIndex], previewImages[arrayIndex], environmentSkyboxes[arrayIndex], environmentPrefabs[arrayIndex], environmentCredits[arrayIndex], environmentURL[arrayIndex]));
            }
        }
        yield return null;
    }
}