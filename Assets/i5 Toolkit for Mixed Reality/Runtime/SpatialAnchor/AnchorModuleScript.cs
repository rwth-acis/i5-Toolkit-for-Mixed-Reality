using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Microsoft.Azure.SpatialAnchors;
using Microsoft.Azure.SpatialAnchors.Unity;
using UnityEditor;

#if WINDOWS_UWP
using Windows.Storage;
#endif

public class AnchorModuleScript : MonoBehaviour
{
    public bool UseOwnAnchorManager = true;

    public SpatialAnchorManager ASAManager;

    public string ASAAccountId;
    public string ASAAccountKey;
    public string ASAAccountDomain;


    [HideInInspector]
    // Anchor ID for anchor stored in Azure (provided by Azure) 
    public string currentAzureAnchorID = "";

    private List<IASAFeedbackReciever> feedbackList = new List<IASAFeedbackReciever>();

    private SpatialAnchorManager cloudManager;
    private CloudSpatialAnchor currentCloudAnchor;
    private AnchorLocateCriteria anchorLocateCriteria;
    private CloudSpatialAnchorWatcher currentWatcher;

    private readonly Queue<Action> dispatchQueue = new Queue<Action>();

    private Dictionary<string, List<GameObject>> objectAnchors = new Dictionary<string, List<GameObject>>();

    #region Unity Lifecycle
    void Start()
    {
        // Get a reference to the SpatialAnchorManager component (must be on the same gameobject)
        if (UseOwnAnchorManager)
        {
            cloudManager = ASAManager;
        }
        else
        {
            //Add new Spatial Anchor Manager to the gameobject
            gameObject.AddComponent<SpatialAnchorManager>();
            cloudManager = GetComponent<SpatialAnchorManager>();
            cloudManager.AuthenticationMode = AuthenticationMode.ApiKey;
            cloudManager.SpatialAnchorsAccountId = ASAAccountId;
            cloudManager.SpatialAnchorsAccountKey = ASAAccountKey;
            cloudManager.SpatialAnchorsAccountDomain = ASAAccountDomain;
        }
        
        if(cloudManager == null)
        {
            Debug.Log("There is no Spatial Anchor Manager Component on the same gameobject as the anchor module one. Please add one.");
        }
        else
        {
            Debug.Log("CloudManager ist da");
        }
        // Register for Azure Spatial Anchor events
        cloudManager.AnchorLocated += CloudManager_AnchorLocated;

        anchorLocateCriteria = new AnchorLocateCriteria();
    }

    void Update()
    {
        lock (dispatchQueue)
        {
            if (dispatchQueue.Count > 0)
            {
                dispatchQueue.Dequeue()();
            }
        }
    }

    void OnDestroy()
    {
        if (cloudManager != null && cloudManager.Session != null)
        {
            cloudManager.DestroySession();
        }

        if (currentWatcher != null)
        {
            currentWatcher.Stop();
            currentWatcher = null;
        }
    }
    #endregion

    #region Public Methods
    public void registerFeedbackReciever(IASAFeedbackReciever reciever)
    {
        feedbackList.Add(reciever);
        Debug.Log("Reciever registered successfully!");
    }
    public async Task StartAzureSession(string accountID, string accountKey, string accountDomain)
    {
        Debug.Log("\nAnchorModuleScript.StartAzureSession()");

        //Setup cloudmanager
        //cloudManager.AuthenticationMode = AuthenticationMode.ApiKey;
        //cloudManager.SpatialAnchorsAccountId = accountID;
        //cloudManager.SpatialAnchorsAccountKey = accountKey;
        //cloudManager.SpatialAnchorsAccountDomain = accountDomain;
        // Notify AnchorFeedbackScript
        OnStartASASession?.Invoke();

        Debug.Log("Starting Azure session... please wait...");

        if (cloudManager.Session == null)
        {
            // Creates a new session if one does not exist
            await cloudManager.CreateSessionAsync();
        }

        // Starts the session if not already started
        await cloudManager.StartSessionAsync();

        Debug.Log("Azure session started successfully");
    }

    public async void StopAzureSession()
    {
        Debug.Log("\nAnchorModuleScript.StopAzureSession()");

        // Notify AnchorFeedbackScript
        OnEndASASession?.Invoke();

        Debug.Log("Stopping Azure session... please wait...");

        // Stops any existing session
        cloudManager.StopSession();

        // Resets the current session if there is one, and waits for any active queries to be stopped
        await cloudManager.ResetSessionAsync();

        Debug.Log("Azure session stopped successfully");
    }

    public async Task<string> CreateAzureAnchor(GameObject theObject, DateTimeOffset? expirationOffset = null)
    {
        foreach (IASAFeedbackReciever reciever in feedbackList)
        {
            reciever.CreateAnchorStarted();
        }
        Debug.Log("\nAnchorModuleScript.CreateAzureAnchor()");

        // Notify AnchorFeedbackScript
        OnCreateAnchorStarted?.Invoke();

        // First we create a native XR anchor at the location of the object in question
        theObject.CreateNativeAnchor();

        // Notify AnchorFeedbackScript
        OnCreateLocalAnchor?.Invoke();

        // Then we create a new local cloud anchor
        CloudSpatialAnchor localCloudAnchor = new CloudSpatialAnchor();

        // Now we set the local cloud anchor's position to the native XR anchor's position
        localCloudAnchor.LocalAnchor = theObject.FindNativeAnchor().GetPointer();

        // Check to see if we got the local XR anchor pointer
        if (localCloudAnchor.LocalAnchor == IntPtr.Zero)
        {
            Debug.Log("Didn't get the local anchor...");
            return "";
        }
        else
        {
            Debug.Log("Local anchor created");
        }

        // In this sample app we delete the cloud anchor explicitly, but here we show how to set an anchor to expire automatically
        if(expirationOffset != null)
        {
            localCloudAnchor.Expiration = (DateTimeOffset)expirationOffset;
        }
        

        // Save anchor to cloud
        while (!cloudManager.IsReadyForCreate)
        {
            await Task.Delay(330);
            float createProgress = cloudManager.SessionStatus.RecommendedForCreateProgress;
            foreach(IASAFeedbackReciever reciever in feedbackList)
            {
                reciever.CreateAnchorStatusUpdate(new ASAStaus(createProgress));
            }
            QueueOnUpdate(new Action(() => Debug.Log($"Move your device to capture more environment data: {createProgress:0%}")));
        }

        bool success;

        try
        {
            Debug.Log("Creating Azure anchor... please wait...");

            // Actually save
            await cloudManager.CreateAnchorAsync(localCloudAnchor);

            // Store
            currentCloudAnchor = localCloudAnchor;
            localCloudAnchor = null;

            // Success?
            success = currentCloudAnchor != null;

            if (success)
            {
                Debug.Log($"Azure anchor with ID '{currentCloudAnchor.Identifier}' created successfully");

                // Notify AnchorFeedbackScript
                OnCreateAnchorSucceeded?.Invoke();

                // Update the current Azure anchor ID
                Debug.Log($"Current Azure anchor ID updated to '{currentCloudAnchor.Identifier}'");
                currentAzureAnchorID = currentCloudAnchor.Identifier;

                foreach (IASAFeedbackReciever reciever in feedbackList)
                {
                    reciever.CreateAnchorFinalized();
                }

                return currentCloudAnchor.Identifier;
            }
            else
            {
                Debug.Log($"Failed to save cloud anchor with ID '{currentAzureAnchorID}' to Azure");

                // Notify AnchorFeedbackScript
                OnCreateAnchorFailed?.Invoke();

                return "";
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
        }
        return "";
    }

    public void RemoveLocalAnchor(GameObject theObject)
    {
#if !(UNITY_ANDROID || UNITY_IOS)
        Debug.Log("\nAnchorModuleScript.RemoveLocalAnchor()");

        // Notify AnchorFeedbackScript
        OnRemoveLocalAnchor?.Invoke();

        theObject.DeleteNativeAnchor();

        if (theObject.FindNativeAnchor() == null)
        {
            Debug.Log("Local anchor deleted succesfully");
        }
        else
        {
            Debug.Log("Attempt to delete local anchor failed");
        }
#else

 Debug.Log("\nAnchorModuleScript.RemoveLocalAnchor()");

#endif
    }

    /// <summary>
    /// Finds an ASA anchor with the given id and sets the transform of the given target gameobject to the anchor position.
    /// </summary>
    /// <param name="id">The identifier of the ASA anchor that is searched for.</param>
    /// <param name="targetGameObject">The gameobject that will be repositioned when an anchor is found.</param>
    public void FindAzureAnchor(string id, GameObject targetGameObject)
    {
        //Guards
        if (id == "" || id == null)
        {
            Debug.Log("The anchor id is not allowed to be null or an empty string. No anchor searched.");
            return;
        }
        if (targetGameObject == null)
        {
            Debug.Log("The target gameobject is not allowed to be null. No anchor searched.");
            return;
        }

        Debug.Log("\nAnchorModuleScript.FindAzureAnchor()");

        // Notify AnchorFeedbackScript
        OnFindASAAnchor?.Invoke();

        // Set up list of anchor IDs to locate
        List<GameObject> gameObjectsWithSameAnchor;
        if(objectAnchors.TryGetValue(id,out gameObjectsWithSameAnchor))
        {
            gameObjectsWithSameAnchor.Add(targetGameObject);
        }
        else
        {
            List<GameObject> gameObjectList = new List<GameObject>();
            gameObjectList.Add(targetGameObject);
            objectAnchors.Add(id, gameObjectList);
        }

        List<string> anchorsToFind = new List<string>();
        anchorsToFind.Add(id);

        anchorLocateCriteria.Identifiers = anchorsToFind.ToArray();
        Debug.Log($"Anchor locate criteria configured to look for Azure anchor with ID '{currentAzureAnchorID}'");

        // Start watching for Anchors
        if ((cloudManager != null) && (cloudManager.Session != null))
        {
            currentWatcher = cloudManager.Session.CreateWatcher(anchorLocateCriteria);
            Debug.Log("Watcher created");
            Debug.Log("Looking for Azure anchor... please wait...");
        }
        else
        {
            Debug.Log("Attempt to create watcher failed, no session exists");
            currentWatcher = null;
        }
    }

    public async void DeleteAzureAnchor()
    {
        Debug.Log("\nAnchorModuleScript.DeleteAzureAnchor()");

        // Notify AnchorFeedbackScript
        OnDeleteASAAnchor?.Invoke();

        // Delete the Azure anchor with the ID specified off the server and locally
        await cloudManager.DeleteAnchorAsync(currentCloudAnchor);
        currentCloudAnchor = null;

        Debug.Log("Azure anchor deleted successfully");
    }

    public void SaveAzureAnchorIdToDisk()
    {
        Debug.Log("\nAnchorModuleScript.SaveAzureAnchorIDToDisk()");

        string filename = "SavedAzureAnchorID.txt";
        string path = Application.persistentDataPath;

#if WINDOWS_UWP
        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
        path = storageFolder.Path.Replace('\\', '/') + "/";
#endif

        string filePath = Path.Combine(path, filename);
        File.WriteAllText(filePath, currentAzureAnchorID);

        Debug.Log($"Current Azure anchor ID '{currentAzureAnchorID}' successfully saved to path '{filePath}'");
    }

    public void GetAzureAnchorIdFromDisk()
    {
        Debug.Log("\nAnchorModuleScript.LoadAzureAnchorIDFromDisk()");

        string filename = "SavedAzureAnchorID.txt";
        string path = Application.persistentDataPath;

#if WINDOWS_UWP
        StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
        path = storageFolder.Path.Replace('\\', '/') + "/";
#endif

        string filePath = Path.Combine(path, filename);
        currentAzureAnchorID = File.ReadAllText(filePath);

        Debug.Log($"Current Azure anchor ID successfully updated with saved Azure anchor ID '{currentAzureAnchorID}' from path '{path}'");
    }
    #endregion

    #region Event Handlers
    private void CloudManager_AnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        QueueOnUpdate(new Action(() => Debug.Log($"Anchor recognized as a possible Azure anchor")));

        if (args.Status == LocateAnchorStatus.Located || args.Status == LocateAnchorStatus.AlreadyTracked)
        {
            currentCloudAnchor = args.Anchor;

            QueueOnUpdate(() =>
            {
                Debug.Log($"Azure anchor located successfully");

                // Notify AnchorFeedbackScript
                OnASAAnchorLocated?.Invoke();

#if WINDOWS_UWP || UNITY_WSA
                // HoloLens: The position will be set based on the unityARUserAnchor that was located.

                // Create a local anchor at the location of the object in question
                gameObject.CreateNativeAnchor();

                // Notify AnchorFeedbackScript
                OnCreateLocalAnchor?.Invoke();

                // On HoloLens, if we do not have a cloudAnchor already, we will have already positioned the
                // object based on the passed in worldPos/worldRot and attached a new world anchor,
                // so we are ready to commit the anchor to the cloud if requested.
                // If we do have a cloudAnchor, we will use it's pointer to setup the world anchor,
                // which will position the object automatically.
                if (currentCloudAnchor != null)
                {
                    Debug.Log("Local anchor position successfully set to Azure anchor position");

                    gameObject.GetComponent<UnityEngine.XR.WSA.WorldAnchor>().SetNativeSpatialAnchorPtr(currentCloudAnchor.LocalAnchor);
                }

#elif UNITY_ANDROID || UNITY_IOS
                Pose anchorPose = Pose.identity;
                anchorPose = currentCloudAnchor.GetPose();

                Debug.Log($"Setting object(s) to anchor pose with position '{anchorPose.position}' and rotation '{anchorPose.rotation}'");
                transform.position = anchorPose.position;
                transform.rotation = anchorPose.rotation;

                List<GameObject> gameObjectsForAnchor;
                if(objectAnchors.TryGetValue(currentCloudAnchor.Identifier, out gameObjectsForAnchor))
                {
                    foreach(GameObject obj in gameObjectsForAnchor)
                    {
                        obj.transform.position = anchorPose.position;
                        obj.transform.rotation = anchorPose.rotation;
                    }
                }
                else
                {
                    Debug.Log("The dictonairy does not contain any gameobjec that is connected to the anchor id.");
                }

                // Create a native anchor at the location of the object in question
                gameObject.CreateNativeAnchor();

                // Notify AnchorFeedbackScript
                OnCreateLocalAnchor?.Invoke();

#endif
            });
        }
        else
        {
            QueueOnUpdate(new Action(() => Debug.Log($"Attempt to locate Anchor with ID '{args.Identifier}' failed, locate anchor status was not 'Located' but '{args.Status}'")));
        }
    }
    #endregion

    #region Internal Methods and Coroutines
    private void QueueOnUpdate(Action updateAction)
    {
        lock (dispatchQueue)
        {
            dispatchQueue.Enqueue(updateAction);
        }
    }
    #endregion

    #region Public Events
    public delegate void StartASASessionDelegate();
    public event StartASASessionDelegate OnStartASASession;

    public delegate void EndASASessionDelegate();
    public event EndASASessionDelegate OnEndASASession;

    public delegate void CreateAnchorDelegate();
    public event CreateAnchorDelegate OnCreateAnchorStarted;
    public event CreateAnchorDelegate OnCreateAnchorSucceeded;
    public event CreateAnchorDelegate OnCreateAnchorFailed;

    public delegate void CreateLocalAnchorDelegate();
    public event CreateLocalAnchorDelegate OnCreateLocalAnchor;

    public delegate void RemoveLocalAnchorDelegate();
    public event RemoveLocalAnchorDelegate OnRemoveLocalAnchor;

    public delegate void FindAnchorDelegate();
    public event FindAnchorDelegate OnFindASAAnchor;

    public delegate void AnchorLocatedDelegate();
    public event AnchorLocatedDelegate OnASAAnchorLocated;

    public delegate void DeleteASAAnchorDelegate();
    public event DeleteASAAnchorDelegate OnDeleteASAAnchor;
    #endregion
}