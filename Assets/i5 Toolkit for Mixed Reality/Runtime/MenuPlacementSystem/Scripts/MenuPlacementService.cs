using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.Utilities;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace i5.Toolkit.MixedReality.MenuPlacementSystem {

    #region Public Enums

    public enum MenuPlacementMode {
        Automatic,
        Manual,
        //The adjustment mode is just needed when the app bar is expended and should not be visible to the users.
        Adjustment
    }

    /// <summary>
    /// How is the offsets applied to other menu instances
    /// </summary>
    public enum ManipulationLogic {
        OneToAll, //different objects have the same offset for object menus
        OneToOne  //every object has its own offset for the object menu assigned to it.
    }

    public enum BoundsType {
        BasedOnColliders,
        BasedOnRenderers
    }

    public enum OrientationType {
        CameraAligned,
        CameraFacing,
        CameraFacingReverse,
        YawOnly,
        FollowTargetObject,
        Unmodified
    }

    public enum MenuType {
        MainMenu,
        ObjectMenu
    }

    public enum Handedness {
        Left,
        Right
    }

    public enum VariantType {
        Floating,
        Compact
    }

    #endregion

    /// <summary>
    /// The central component of the Menu Placement Service.
    /// This service should be initialized at the start of the application. 
    /// It has the references to all menu objects and stores some data of them. 
    /// The Menu Handler component should call its methods when needed.
    /// </summary>
    [CreateAssetMenu(menuName = "i5 Mixed Reality Toolkit/Menu Placement Service")]
    public class MenuPlacementService : ScriptableObject, IService {

        #region Private Enums
        private enum DefaultMode {
            Automatic,
            Manual
        }
        #endregion

        #region Serizalizable Fields
        //Use to switch between the floating and campact version
        [Header("Menus")]
        [Tooltip("Drag the menu objects here. If you don't want to use one type of them, just leave it.")]
        [SerializeField] private MenuVariants mainMenu;
        [Tooltip("Drag the menu objects here. If you don't want to use one type of them, just leave it.")]
        [SerializeField] private List<MenuVariants> objectMenus;

        [Header("User Interface Components")]
        [Tooltip("The Menu Controller enables the manipulation of created menus and interaction with the system.")]
        [SerializeField] private GameObject systemControlPanel;
        [Tooltip("The app bar for manipulation of menus")]
        [SerializeField] private GameObject appBar;
        [Tooltip("The dialog prefab for suggestions in manual mode")]
        [SerializeField] private GameObject suggestionPanel;
        [Tooltip("The default placement mode")]
        [SerializeField] private DefaultMode defaultPlacementMode = DefaultMode.Automatic;

        [Header("Physics")]
        [Tooltip("The layer of Spatial Awareness used in Raycast, which is Layer 31 by MRTK default settings. Please select the correct layer if the default one is occupied.")]
        [SerializeField] private LayerMask spatialAwarenessLayer = 1 << 31;
        [Tooltip("The layer of menus used in Raycast, which is Layer 9 by default settings. Please select the correct layer if the default one is occupied.")]
        [SerializeField] private LayerMask menuLayer = 1 << 9;

        [Header("Runtime Menu")]
        [Tooltip("Whether adding menus in the runtime is allowed.")]
        [SerializeField] private bool addingRuntimeMenuEnabled = false;
        [Tooltip("The layer of menus to be added as runtime menu.")]
        [SerializeField] private LayerMask runtimeMenuLayer = 1 << 9;
        #endregion

        #region Non-serializable Fields

        //For adding menus at runtime (e.g. mockup-Editor)
        private List<MenuVariants> runtimeMenus = new List<MenuVariants>();

        //Use the corresponding ID of a MenuVariants to get the ObjectPool for it.      
        private int floatingMainMenuPoolID;
        private int compactMainMenuPoolID;
        //Dictionary<menuID, menuPoolID>
        private Dictionary<int, int> floatingObjectMenuPoolIDs = new Dictionary<int, int>();
        private Dictionary<int, int> compactObjectMenuPoolIDs = new Dictionary<int, int>();

        //Main menu bounding boxes. Although we have only one main menu in the inspector, we can add runtime main menus.
        //Dictionary<menuID, Bounds>
        private Dictionary<int, Bounds> boundingBoxFloatingMainMenu = new Dictionary<int, Bounds>();
        private Dictionary<int, Bounds> boundingBoxCompactMainMenu = new Dictionary<int, Bounds>();
        private Dictionary<int, Bounds> boundingBoxFloatingObjectMenu = new Dictionary<int, Bounds>();
        private Dictionary<int, Bounds> boundingBoxCompactObjectMenu = new Dictionary<int, Bounds>();

        //Dictionary<targetObject, Bounds>
        //Buffers for main menus
        private List<Tuple<Vector3, Quaternion, Vector3, float>> retrieveBufferFloatingMainMenu = new List<Tuple<Vector3, Quaternion, Vector3, float>>();
        private List<Tuple<Vector3, Quaternion, Vector3, float>> retrieveBufferCompactMainMenu = new List<Tuple<Vector3, Quaternion, Vector3, float>>();
        //For ManipulationLogic = OneToAll, the dictionaries will be read and written by all menus
        //The buffer "with orbital" is for floating and compact object menus with Orbital enabled.
        private Dictionary<int, List<Tuple<Vector3, Quaternion, Vector3, float>>> retrieveBufferOneToAllOrbital = new Dictionary<int, List<Tuple<Vector3, Quaternion, Vector3, float>>>();
        //The buffer "without orbital" is for floating object menus with InBetween, and compact object menus with HandConstraint enabled.
        private Dictionary<int, List<Tuple<Vector3, Quaternion, Vector3, float>>> retrieveBufferOneToAllWithoutOrbital = new Dictionary<int, List<Tuple<Vector3, Quaternion, Vector3, float>>>();

        //Dictionary<menuID, offsets>
        private Dictionary<int, Tuple<Vector3, Quaternion, Vector3, float>> currentOneToAllOffsetInBetween = new Dictionary<int, Tuple<Vector3, Quaternion, Vector3, float>>();
        private Dictionary<int, Tuple<Vector3, Quaternion, Vector3, float>> currentOneToAllOffsetOrbital = new Dictionary<int, Tuple<Vector3, Quaternion, Vector3, float>>();
        private Dictionary<int, Tuple<Vector3, Quaternion, Vector3, float>> currentOneToAllOffsetHandConstraint = new Dictionary<int, Tuple<Vector3, Quaternion, Vector3, float>>();

        //For ManipulationLogic = OneToOne, the buffers/offsets are only for one single object, and will be read an written by the menu assigned to it.
        //Dictionary<targetObject, List<Tupel<Position, Rotaion, Scale, ViewPercentageV>>>
        private Dictionary<GameObject, List<Tuple<Vector3, Quaternion, Vector3, float>>> retrieveBufferOneToOneOrbitalFloating = new Dictionary<GameObject, List<Tuple<Vector3, Quaternion, Vector3, float>>>();
        private Dictionary<GameObject, List<Tuple<Vector3, Quaternion, Vector3, float>>> retrieveBufferOneToOneOrbitalCompact = new Dictionary<GameObject, List<Tuple<Vector3, Quaternion, Vector3, float>>>();
        private Dictionary<GameObject, List<Tuple<Vector3, Quaternion, Vector3, float>>> retrieveBufferOneToOneInBetween = new Dictionary<GameObject, List<Tuple<Vector3, Quaternion, Vector3, float>>>();
        private Dictionary<GameObject, List<Tuple<Vector3, Quaternion, Vector3, float>>> retrieveBufferOneToOneHandConstraint = new Dictionary<GameObject, List<Tuple<Vector3, Quaternion, Vector3, float>>>();
        //Dictionary<targetObject, Tuple<Vector3, Quaternion, Vector3, float>>
        private Dictionary<GameObject, Tuple<Vector3, Quaternion, Vector3, float>> currentOneToOneOffsetOrbitalFloating = new Dictionary<GameObject, Tuple<Vector3, Quaternion, Vector3, float>>();
        private Dictionary<GameObject, Tuple<Vector3, Quaternion, Vector3, float>> currentOneToOneOffsetOrbitalCompact = new Dictionary<GameObject, Tuple<Vector3, Quaternion, Vector3, float>>();
        private Dictionary<GameObject, Tuple<Vector3, Quaternion, Vector3, float>> currentOneToOneOffsetInBetween = new Dictionary<GameObject, Tuple<Vector3, Quaternion, Vector3, float>>();
        private Dictionary<GameObject, Tuple<Vector3, Quaternion, Vector3, float>> currentOneToOneOffsetHandConstraint = new Dictionary<GameObject, Tuple<Vector3, Quaternion, Vector3, float>>();

        private GameObject inBetweenTarget;
        private MenuPlacementMode placementMode;
        // A counter for the current expanded app bars.
        private int adjustmentModeSemaphore = 0;
        // The maximum MenuID of all menus.
        private int maximumMenuID = 0;
        #endregion

        #region Properties
        /// <summary>
        /// The app bar for manipulation of menu objects.
        /// </summary>
        public GameObject AppBar
        {
            get => appBar;
        }

        /// <summary>
        /// The dialog panel for suggestion in the manual mode.
        /// </summary>
        public GameObject SuggestionPanel
        {
            get => suggestionPanel;
        }

        public bool SuggestionPanelOn { get; set; }
        /// <summary>
        /// The previous mode. Should only be called in adjustment mode.
        /// </summary>
        public MenuPlacementMode PreviousMode { get; set; }

        /// <summary>
        /// If the current running platform supports articulated hand (HoloLens 2)
        /// </summary>
        public bool ArticulatedHandSupported { get; private set; }

        /// <summary>
        /// If the current running platform supports motion controller.
        /// </summary>
        public bool MotionControllerSupported { get; private set; }

        /// <summary>
        /// If the current running platform supports GGV hand.
        /// </summary>
        public bool GGVHandSupported { get; private set; }

        /// <summary>
        /// If the current running platform supports hand tracking. It is equal to ArticulatedHandSupported || MotionControllerSupported.
        /// </summary>
        public bool HandTrackingEnabled { get; private set; }

        /// <summary>
        /// Current placement mode.
        /// </summary>
        public MenuPlacementMode PlacementMode
        {
            get => placementMode;
            set
            {
                placementMode = value;
            }
        }

        /// <summary>
        /// The Layermask of the "spatial awareness" layer, default is 31 by MRTK.
        /// </summary>
        public LayerMask SpatialAwarenessLayer
        {
            get => spatialAwarenessLayer;
        }

        /// <summary>
        /// The Layermask of the "menu" layer. All menu used in the menu placement system should be assigned to this layer.
        /// </summary>
        public LayerMask MenuLayer
        {
            get => menuLayer;
        }

        /// <summary>
        /// The Layermask of the "runtime menu" layer. If "Adding Runtime Menu Enabled" is true, only objects on this layer will be considered.
        /// </summary>
        public LayerMask RuntimeMenuLayer
        {
            get => runtimeMenuLayer;
        }

        #endregion

        #region IService Methods

        public void Initialize(IServiceManager owner) {
            CheckMenuInitialization();
            CreateObjectPools();
            InitializeProperties();
            InitializeMainMenuBuffers();
            InitializeOneToAllRetrieveBuffers();
            InitializeCurrentOneToAllOffsets();
            InitializeRuntimeMenus();
            CreateInBetweenTarget();
            CreateMenuController();
            CheckCapability();
        }

        public void Cleanup() {
            InitializeProperties();
        }

        #endregion

        #region Public Methods

        #region Methods for Buffer & Offsets

        /// <summary>
        /// Get the retrieve buffer with given parameters, you can leave certain parameters null or set them casually if they are not important to identify the buffer. Select solverName from Orbital, InBetween,and HandConstraint.
        /// </summary>
        /// <param name="solverName">Select from Orbital, InBetween, HandConstraint</param>
        public List<Tuple<Vector3, Quaternion, Vector3, float>> GetRetrieveBuffer(MenuType variant, ManipulationLogic manipulationLogic, bool compact, int menuID, string solverName, GameObject targetObject) {
            switch (variant) {
                case MenuType.MainMenu:
                    return GetRetrieveBufferMainMenu(menuID);
                case MenuType.ObjectMenu:
                    switch (manipulationLogic) {
                        case ManipulationLogic.OneToAll:
                            switch (solverName) {
                                case "Orbital":
                                    return GetRetrieveBufferOneToAllOrbital(menuID);
                                case "InBetween":
                                    return GetRetrieveBufferOneToAllWithoutOrbital(menuID);
                                case "HandConstraint":
                                    return GetRetrieveBufferOneToAllWithoutOrbital(menuID);
                                default:
                                    Debug.LogError("No Such Buffer, please check the solverName: " + solverName + ", the variant: " + variant + ", and the logic: " + manipulationLogic);
                                    return null;
                            }
                        case ManipulationLogic.OneToOne:
                            if (compact) {
                                switch (solverName) {
                                    case "Orbital":
                                        return GetRetrieveBufferOneToOneOrbitalCompact(targetObject);
                                    case "HandConstraint":
                                        return GetRetrieveBufferOneToOneHandConstraint(targetObject);
                                    default:
                                        return null;
                                }
                            }
                            else {
                                switch (solverName) {
                                    case "Orbital":
                                        return GetRetrieveBufferOneToOneOrbitalFloating(targetObject);
                                    case "InBetween":
                                        return GetRetrieveBufferOneToOneInBetween(targetObject);
                                    default:
                                        Debug.LogError("No Such Buffer, please check the solverName: " + solverName + ", the variant: " + variant + ", and the logic: " + manipulationLogic);
                                        return null;
                                }
                            }
                        default:
                            return null;
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the current offset with given parameters, you can leave certain parameters null or set them casually if they are not important to identify the offset. Select solverName from Orbital, InBetween,and HandConstraint.
        /// </summary>
        /// <param name="solverName">Select from Orbital, InBetween, HandConstraint</param>
        public Tuple<Vector3, Quaternion, Vector3, float> GetOffset(ManipulationLogic manipulationLogic, bool compact, int menuID, string solverName, GameObject targetObject) {
            switch (manipulationLogic) {
                case ManipulationLogic.OneToAll:
                    switch (solverName) {
                        case "Orbital":
                            return GetCurrentOneToAllOffsetOrbital(menuID);
                        case "InBetween":
                            return GetCurrentOneToAllOffsetInBetween(menuID);
                        case "HandConstraint":
                            return GetCurrentOneToAllOffsetHandConstraint(menuID);
                        default:
                            Debug.LogError("No Such Offset, please check the solverName: " + solverName + ", and the logic: " + manipulationLogic);
                            return null;
                    }
                case ManipulationLogic.OneToOne:
                    if (compact) {
                        switch (solverName) {
                            case "Orbital":
                                return GetCurrentOneToOneOffsetOrbitalCompact(targetObject);
                            case "HandConstraint":
                                return GetCurrentOneToOneOffsetHandConstraint(targetObject);
                            default:
                                return null;
                        }
                    }
                    else {
                        switch (solverName) {
                            case "Orbital":
                                return GetCurrentOneToOneOffsetOrbitalFloating(targetObject);
                            case "InBetween":
                                return GetCurrentOneToOneOffsetInBetween(targetObject);
                            default:
                                Debug.LogError("No Such Buffer, please check the solverName: " + solverName + ", and the logic: " + manipulationLogic);
                                return null;
                        }
                    }
                default:
                    return null;
            }
        }

        public List<Tuple<Vector3, Quaternion, Vector3, float>> GetRetrieveBufferMainMenu(int menuID) {
            if(menuID == mainMenu.floatingMenu.GetComponent<MenuHandler>().MenuID) {
                return retrieveBufferFloatingMainMenu;
            }
            else {
                return retrieveBufferCompactMainMenu;
            }
        }

        public List<Tuple<Vector3, Quaternion, Vector3, float>> GetRetrieveBufferOneToAllWithoutOrbital(GameObject menu) {
            int menuID = menu.GetComponent<MenuHandler>().MenuID;
            return GetRetrieveBufferOneToAllWithoutOrbital(menuID);
        }

        public List<Tuple<Vector3, Quaternion, Vector3, float>> GetRetrieveBufferOneToAllWithoutOrbital(int menuID) {
            if (retrieveBufferOneToAllWithoutOrbital.TryGetValue(menuID, out List<Tuple<Vector3, Quaternion, Vector3, float>> buffer)) {
                return buffer;
            }
            else {
                Debug.LogError("RetrieveBufferAutomaticModeWithoutOrbital for menu with ID \"" + menuID + "\" not found.");
                return buffer;
            }
        }

        public List<Tuple<Vector3, Quaternion, Vector3, float>> GetRetrieveBufferOneToAllOrbital(GameObject menu) {
            int menuID = menu.GetComponent<MenuHandler>().MenuID;
            return GetRetrieveBufferOneToAllOrbital(menuID);
        }

        public List<Tuple<Vector3, Quaternion, Vector3, float>> GetRetrieveBufferOneToAllOrbital(int menuID) {
            if (retrieveBufferOneToAllOrbital.TryGetValue(menuID, out List<Tuple<Vector3, Quaternion, Vector3, float>> buffer)) {
                return buffer;
            }
            else {
                Debug.LogError("RetrieveBufferAutomaticModeOrbital for menu with ID \"" + menuID + "\" not found.");
                return buffer;
            }
        }

        public Tuple<Vector3, Quaternion, Vector3, float> GetCurrentOneToAllOffsetInBetween(int menuID) {
            if (currentOneToAllOffsetInBetween.TryGetValue(menuID, out Tuple<Vector3, Quaternion, Vector3, float> offset)) {
                return offset;
            }
            else {
                Debug.LogError("LastOffsetInbetween for menu with ID \"" + menuID + "\" not found.");
                return offset;
            }
        }

        public Tuple<Vector3, Quaternion, Vector3, float> GetCurrentOneToAllOffsetInBetween(GameObject menu) {
            return GetCurrentOneToAllOffsetInBetween(menu.GetComponent<MenuHandler>().MenuID);
        }

        public void SetCurrentOneToAllOffsetInBetween(int menuID, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            currentOneToAllOffsetInBetween[menuID] = offset;
        }

        public void SetCurrentOneToAllOffsetInBetween(GameObject menu, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            SetCurrentOneToAllOffsetInBetween(menu.GetComponent<MenuHandler>().MenuID, offset);
        }

        public Tuple<Vector3, Quaternion, Vector3, float> GetCurrentOneToAllOffsetOrbital(int menuID) {
            if (currentOneToAllOffsetOrbital.TryGetValue(menuID, out Tuple<Vector3, Quaternion, Vector3, float> offset)) {
                return offset;
            }
            else {
                Debug.LogError("LastOffsetOrbital for menu with ID \"" + menuID + "\" not found.");
                return offset;
            }
        }

        public Tuple<Vector3, Quaternion, Vector3, float> GetCurrentOneToAllOffsetOrbital(GameObject menu) {
            return GetCurrentOneToAllOffsetOrbital(menu.GetComponent<MenuHandler>().MenuID);
        }

        public void SetCurrentOneToAllOffsetOrbital(int menuID, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            currentOneToAllOffsetOrbital[menuID] = offset;
        }

        public void SetCurrentOneToAllOffsetOrbital(GameObject menu, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            SetCurrentOneToAllOffsetOrbital(menu.GetComponent<MenuHandler>().MenuID, offset);
        }

        public Tuple<Vector3, Quaternion, Vector3, float> GetCurrentOneToAllOffsetHandConstraint(int menuID) {
            if (currentOneToAllOffsetHandConstraint.TryGetValue(menuID, out Tuple<Vector3, Quaternion, Vector3, float> offset)) {
                return offset;
            }
            else {
                Debug.LogError("LastOffsetHandConstraint for menu with ID \"" + menuID + "\" not found.");
                return offset;
            }
        }

        public Tuple<Vector3, Quaternion, Vector3, float> GetCurrentOneToAllOffsetHandConstraint(GameObject menu) {
            return GetCurrentOneToAllOffsetHandConstraint(menu.GetComponent<MenuHandler>().MenuID);
        }

        public void SetCurrentOneToAllOffsetHandConstraint(int menuID, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            currentOneToAllOffsetHandConstraint[menuID] = offset;
        }

        public void SetCurrentOneToAllOffsetHandConstraint(GameObject menu, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            SetCurrentOneToAllOffsetHandConstraint(menu.GetComponent<MenuHandler>().MenuID, offset);
        }

        public void InitializeRetrieveBufferOneToOneOrbitalFloating(GameObject targetObject, MenuHandler handler) {
            if (!retrieveBufferOneToOneOrbitalFloating.ContainsKey(targetObject)) {
                retrieveBufferOneToOneOrbitalFloating.Add(targetObject, new List<Tuple<Vector3, Quaternion, Vector3, float>>(handler.RetrieveBufferSize));
            }
        }

        public void InitializeRetrieveBufferOneToOneOrbitalCompact(GameObject targetObject, MenuHandler handler) {
            if (!retrieveBufferOneToOneOrbitalCompact.ContainsKey(targetObject)) {
                retrieveBufferOneToOneOrbitalCompact.Add(targetObject, new List<Tuple<Vector3, Quaternion, Vector3, float>>(handler.RetrieveBufferSize));
            }
        }

        public void InitializeRetrieveBufferOneToOneInBetween(GameObject targetObject, MenuHandler handler) {
            if (!retrieveBufferOneToOneInBetween.ContainsKey(targetObject)) {
                retrieveBufferOneToOneInBetween.Add(targetObject, new List<Tuple<Vector3, Quaternion, Vector3, float>>(handler.RetrieveBufferSize));
            }
        }

        public void InitializeRetrieveBufferOneToOneHandConstraint(GameObject targetObject, MenuHandler handler) {
            if (!retrieveBufferOneToOneHandConstraint.ContainsKey(targetObject)) {
                retrieveBufferOneToOneHandConstraint.Add(targetObject, new List<Tuple<Vector3, Quaternion, Vector3, float>>(handler.RetrieveBufferSize));
            }
        }

        public List<Tuple<Vector3, Quaternion, Vector3, float>> GetRetrieveBufferOneToOneOrbitalFloating(GameObject targetObject) {
            return retrieveBufferOneToOneOrbitalFloating[targetObject];
        }

        public List<Tuple<Vector3, Quaternion, Vector3, float>> GetRetrieveBufferOneToOneOrbitalCompact(GameObject targetObject) {
            return retrieveBufferOneToOneOrbitalCompact[targetObject];
        }

        public List<Tuple<Vector3, Quaternion, Vector3, float>> GetRetrieveBufferOneToOneInBetween(GameObject targetObject) {
            return retrieveBufferOneToOneInBetween[targetObject];
        }

        public List<Tuple<Vector3, Quaternion, Vector3, float>> GetRetrieveBufferOneToOneHandConstraint(GameObject targetObject) {
            return retrieveBufferOneToOneHandConstraint[targetObject];
        }

        public void InitializeCurrentOneToOneOffsetOrbitalFloating(GameObject targetObject, MenuHandler handler) {
            if (!currentOneToOneOffsetOrbitalFloating.ContainsKey(targetObject)) {
                currentOneToOneOffsetOrbitalFloating.Add(targetObject, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, handler.DefaultTargetViewPercentV));
            }
        }

        public void InitializeCurrentOneToOneOffsetOrbitalCompact(GameObject targetObject, MenuHandler handler) {
            if (!currentOneToOneOffsetOrbitalCompact.ContainsKey(targetObject)) {
                currentOneToOneOffsetOrbitalCompact.Add(targetObject, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, handler.DefaultTargetViewPercentV));
            }
        }

        public void InitializeCurrentOneToOneOffsetInBetween(GameObject targetObject, MenuHandler handler) {
            if (!currentOneToOneOffsetInBetween.ContainsKey(targetObject)) {
                currentOneToOneOffsetInBetween.Add(targetObject, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, handler.DefaultTargetViewPercentV));
            }
        }

        public void InitializeCurrentOneToOneOffsetHandConstraint(GameObject targetObject, MenuHandler handler) {
            if (!currentOneToOneOffsetHandConstraint.ContainsKey(targetObject)) {
                currentOneToOneOffsetHandConstraint.Add(targetObject, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, handler.DefaultTargetViewPercentV));
            }
        }

        public Tuple<Vector3, Quaternion, Vector3, float> GetCurrentOneToOneOffsetOrbitalFloating(GameObject targetObject) {
            return currentOneToOneOffsetOrbitalFloating[targetObject];
        }

        public Tuple<Vector3, Quaternion, Vector3, float> GetCurrentOneToOneOffsetOrbitalCompact(GameObject targetObject) {
            return currentOneToOneOffsetOrbitalCompact[targetObject];
        }

        public Tuple<Vector3, Quaternion, Vector3, float> GetCurrentOneToOneOffsetInBetween(GameObject targetObject) {
            return currentOneToOneOffsetInBetween[targetObject];
        }

        public Tuple<Vector3, Quaternion, Vector3, float> GetCurrentOneToOneOffsetHandConstraint(GameObject targetObject) {
            return currentOneToOneOffsetHandConstraint[targetObject];
        }

        public void SetCurrentOneToOneOffsetOrbitalFloating(GameObject targetObject, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            currentOneToOneOffsetOrbitalFloating[targetObject] = offset;
        }

        public void SetCurrentOneToOneOffsetOrbitalCompact(GameObject targetObject, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            currentOneToOneOffsetOrbitalCompact[targetObject] = offset;
        }

        public void SetCurrentOneToOneOffsetInBetween(GameObject targetObject, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            currentOneToOneOffsetInBetween[targetObject] = offset;
        }

        public void SetCurrentOneToOneOffsetHandConstraint(GameObject targetObject, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            currentOneToOneOffsetHandConstraint[targetObject] = offset;
        }

        #endregion

        /// <summary>
        /// Remove the entries of targetObject from all OneToOne buffers and offsets.
        /// </summary>
        /// <param name="targetObject">The targetObject</param>
        public void RemoveTargetObject(GameObject targetObject) {
            if (retrieveBufferOneToOneOrbitalFloating.ContainsKey(targetObject)) {
                retrieveBufferOneToOneOrbitalFloating.Remove(targetObject);
            }
            if (retrieveBufferOneToOneOrbitalCompact.ContainsKey(targetObject)) {
                retrieveBufferOneToOneOrbitalCompact.Remove(targetObject);
            }
            if (retrieveBufferOneToOneInBetween.ContainsKey(targetObject)) {
                retrieveBufferOneToOneInBetween.Remove(targetObject);
            }
            if (retrieveBufferOneToOneHandConstraint.ContainsKey(targetObject)) {
                retrieveBufferOneToOneHandConstraint.Remove(targetObject);
            }
            if (currentOneToOneOffsetOrbitalFloating.ContainsKey(targetObject)) {
                currentOneToOneOffsetOrbitalFloating.Remove(targetObject);
            }
            if (currentOneToOneOffsetOrbitalCompact.ContainsKey(targetObject)) {
                currentOneToOneOffsetOrbitalCompact.Remove(targetObject);
            }
            if (currentOneToOneOffsetInBetween.ContainsKey(targetObject)) {
                currentOneToOneOffsetInBetween.Remove(targetObject);
            }
            if (currentOneToOneOffsetHandConstraint.ContainsKey(targetObject)) {
                currentOneToOneOffsetHandConstraint.Remove(targetObject);
            }
        }

        /// <summary>
        /// Get the greatest menuID in all saved main and object menus.
        /// This method is used to find the maximum menuID in order to add a new menu in the runtime and avoid duplicates of menuIDs.
        /// </summary>
        /// <returns>The greatest menuID among all main and object menus.</returns>
        public int GetMaximumMenuID() {
            foreach (MenuVariants v in runtimeMenus) {
                maximumMenuID = maximumMenuID < v.floatingMenu.GetComponent<MenuHandler>().MenuID ? v.floatingMenu.GetComponent<MenuHandler>().MenuID : maximumMenuID;
                maximumMenuID = maximumMenuID < v.compactMenu.GetComponent<MenuHandler>().MenuID ? v.compactMenu.GetComponent<MenuHandler>().MenuID : maximumMenuID;
            }
            return maximumMenuID;
        }

        /// <summary>
        /// Get the InBewteen target, which is an empty objects slightly below the head.
        /// </summary>
        public GameObject GetInBetweenTarget() {
            return inBetweenTarget;
        }

        public void Exit() {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
            Application.Quit();
        }

        /// <summary>
        /// Switch between automatic and manual mode on the users' side.
        /// For adjustment mode, use the EnterAdjustmentMode() method instead.
        /// </summary>
        public void SwitchMode() {
            if (placementMode == MenuPlacementMode.Automatic) {
                placementMode = MenuPlacementMode.Manual;
            }
            else {
                placementMode = MenuPlacementMode.Automatic;
            }
        }

        public void EnterManualMode() {
            placementMode = MenuPlacementMode.Manual;
        }

        public void EnterAutomaticMode() {
            placementMode = MenuPlacementMode.Automatic;
        }
        /// <summary>
        /// Enter the adjustment mode, increase the counter by 1
        /// </summary>
        public void EnterAdjustmentMode() {
            if (adjustmentModeSemaphore == 0) {
                PreviousMode = placementMode;
                placementMode = MenuPlacementMode.Adjustment;
            }
            adjustmentModeSemaphore++;
        }
        
        /// <summary>
        /// Exit the adjustment mode(for this app bar, for example). Actually this method decrease the counter by 1, and the adjustment mode is exited when the counter reached 0.
        /// </summary>
        public void ExitAdjustmentMode() {
            adjustmentModeSemaphore--;
            if (adjustmentModeSemaphore < 0) {
                Debug.LogError("AdjustmentModeSemaphore is smaller than 0, it has value " + adjustmentModeSemaphore + ". It is forced reset to 0.");
                adjustmentModeSemaphore = 0;
            }
            if (adjustmentModeSemaphore == 0) {
                placementMode = PreviousMode;
            }
        }

        /// <summary>
        /// Recognize an object as a menu (main or object) and add it to the system during the runtime. This method also add a MenuHandler component to the given object.
        /// All properties except 'MenuType' are not modifiable, and will have their default values.
        /// It is only for some special purposes which ask developers to add a menu during the runtime, e.g. the mockup-editor:
        /// https://github.com/rwth-acis/VIAProMa/wiki/Mockup-Editor
        /// </summary>
        /// <param name="variants">The floating and/or the compact variant that should be recogized as a menu</param>
        public void AddMenuRuntime(MenuVariants variants) {
            runtimeMenus.Add(variants);
            if (variants.floatingMenu) {
                MenuHandler handler = variants.floatingMenu.GetComponent<MenuHandler>();
                retrieveBufferOneToAllOrbital.Add(handler.MenuID, new List<Tuple<Vector3, Quaternion, Vector3, float>>(handler.RetrieveBufferSize));
                currentOneToAllOffsetInBetween.Add(handler.MenuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, handler.DefaultTargetViewPercentV));
                currentOneToAllOffsetOrbital.Add(handler.MenuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, handler.DefaultTargetViewPercentV));
            }
            if (variants.compactMenu) {
                MenuHandler handler = variants.compactMenu.GetComponent<MenuHandler>();
                retrieveBufferOneToAllWithoutOrbital.Add(handler.MenuID, new List<Tuple<Vector3, Quaternion, Vector3, float>>(handler.RetrieveBufferSize));
                currentOneToAllOffsetOrbital.Add(handler.MenuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, handler.DefaultTargetViewPercentV));
                currentOneToAllOffsetHandConstraint.Add(handler.MenuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, handler.DefaultTargetViewPercentV));
            }

        }



        /// <summary>
        /// Fetch a menu object from the ObjectPool of the origin menu
        /// </summary>
        public GameObject InstantiateMenu(GameObject origin) {
            MenuHandler handler = origin.GetComponent<MenuHandler>();
            if (handler.IsRuntimeMenu) {
                origin.SetActive(true);
                return origin;
            }
            else {
                if (handler.MenuType == MenuType.MainMenu) {
                    if (handler.VariantType == VariantType.Compact) {
                        return ObjectPool<GameObject>.RequestResource(compactMainMenuPoolID, () => { return Instantiate(mainMenu.compactMenu); });
                    }
                    else {
                        return ObjectPool<GameObject>.RequestResource(floatingMainMenuPoolID, () => { return Instantiate(mainMenu.floatingMenu); });
                    }
                }
                else {
                    if (handler.VariantType == VariantType.Compact) {
                        return ObjectPool<GameObject>.RequestResource(compactObjectMenuPoolIDs[handler.MenuID], () => { return Instantiate(GetObjectMenuWithID(handler.MenuID)); });
                    }
                    else {
                        return ObjectPool<GameObject>.RequestResource(floatingObjectMenuPoolIDs[handler.MenuID], () => { return Instantiate(GetObjectMenuWithID(handler.MenuID)); });
                    }
                }
            }
        }

        /// <summary>
        /// Return a menu object to its ObjectPool
        /// </summary>
        public void CloseMenu(GameObject menu) {
            MenuHandler handler = menu.GetComponent<MenuHandler>();
            if (handler.IsRuntimeMenu) {
                menu.SetActive(false);
            }
            else {
                if (handler.MenuType == MenuType.MainMenu) {
                    if (handler.VariantType == VariantType.Compact) {
                        Debug.Log("Close Compact Main Menu");
                        ObjectPool<GameObject>.ReleaseResource(compactMainMenuPoolID, menu);
                    }
                    else {
                        ObjectPool<GameObject>.ReleaseResource(floatingMainMenuPoolID, menu);
                    }
                }
                else {
                    if (handler.VariantType == VariantType.Compact) {
                        ObjectPool<GameObject>.ReleaseResource(compactObjectMenuPoolIDs[handler.MenuID], menu);
                    }
                    else {
                        ObjectPool<GameObject>.ReleaseResource(floatingObjectMenuPoolIDs[handler.MenuID], menu);
                    }
                }
            }
        }

        /// <summary>
        /// Switch a menu to another variant according to the placement message.
        /// </summary>
        /// <param name="message"> the variant switch to</param>
        /// <param name="menu">the menu object</param>
        public void UpdatePlacement(PlacementMessage message, GameObject menu) {
            switch (message.switchType) {
                case PlacementMessage.SwitchType.FloatingToCompact:
                    //Debug.Log("The menu is floating: " + !menu.GetComponent<MenuHandler>().compact);
                    if (menu.GetComponent<MenuHandler>().VariantType == VariantType.Floating) {
                        if (SwitchToCompact(menu) != menu) {
                            menu.GetComponent<MenuHandler>().Close();
                            SwitchToCompact(menu).GetComponent<MenuHandler>().Open(menu.GetComponent<MenuHandler>().TargetObject);
                        }
                    }
                    break;
                case PlacementMessage.SwitchType.CompactToFloating:
                    //Debug.Log("The menu is compact: " + menu.GetComponent<MenuHandler>().compact);
                    if (menu.GetComponent<MenuHandler>().VariantType == VariantType.Compact) {
                        if (SwitchToFloating(menu) != menu) {
                            menu.GetComponent<MenuHandler>().Close();
                            SwitchToFloating(menu).GetComponent<MenuHandler>().Open(menu.GetComponent<MenuHandler>().TargetObject);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Get the bounding box of the other variant, which is saved in MenuPlacementService on its close.
        /// </summary>
        /// <param name="menu">the current activated menu variant</param>
        /// <returns>the bounding box of the other variant</returns>
        public Bounds GetStoredBoundingBoxOppositeVariant(GameObject menu) {
            MenuHandler handler = menu.GetComponent<MenuHandler>();
            if (handler.MenuType == MenuType.MainMenu) {
                if (handler.VariantType == VariantType.Compact) {
                    int menuID = SwitchToFloating(menu).GetComponent<MenuHandler>().MenuID;
                    if (boundingBoxFloatingMainMenu.ContainsKey(menuID)) {
                        return boundingBoxFloatingMainMenu[menuID];
                    }
                    else {
                        Debug.Log("Bounding box not found for opposite variant.");
                        return new Bounds();
                    }
                    
                }
                else {
                    int menuID = SwitchToCompact(menu).GetComponent<MenuHandler>().MenuID;
                    if (boundingBoxCompactMainMenu.ContainsKey(menuID)) {
                        return boundingBoxCompactMainMenu[menuID];
                    }
                    else {
                        Debug.Log("Bounding box not found for opposite variant.");
                        return new Bounds();
                    }
                    
                }
            }
            else {
                Bounds res;
                if (handler.VariantType == VariantType.Compact) {
                    int menuID = SwitchToFloating(menu).GetComponent<MenuHandler>().MenuID;
                    if (boundingBoxFloatingObjectMenu.TryGetValue(menuID, out res)){
                        return res;
                    }
                    else {
                        Debug.Log("Bounding box not found for opposite variant.");
                        return new Bounds();
                    }
                }
                else {
                    int menuID = SwitchToCompact(menu).GetComponent<MenuHandler>().MenuID;
                    if(boundingBoxCompactObjectMenu.TryGetValue(menuID, out res)) {
                        return res;
                    }
                    else{
                        Debug.Log("Bounding box not found for opposite variant.");
                        return new Bounds();
                    }
                }
            }
        }

        /// <summary>
        /// Save the bounding box of one menu object. 
        /// </summary>
        /// <param name="menu">the closed (to be closed) menu</param>
        /// <param name="boundingBox">the bounding box of the menu</param>
        public void StoreBoundingBox(GameObject menu, Bounds boundingBox) {
            MenuHandler handler = menu.GetComponent<MenuHandler>();
            if (handler.MenuType == MenuType.MainMenu) {
                if (handler.VariantType == VariantType.Compact) {
                    if (boundingBoxCompactMainMenu.ContainsKey(handler.MenuID)) {
                        boundingBoxCompactMainMenu[handler.MenuID] = boundingBox;
                    }
                    else {
                        boundingBoxCompactMainMenu.Add(handler.MenuID, boundingBox);
                    }
                }
                else {
                    if (boundingBoxFloatingMainMenu.ContainsKey(handler.MenuID)) {
                        boundingBoxFloatingMainMenu[handler.MenuID] = boundingBox;
                    }
                    else {
                        boundingBoxFloatingMainMenu.Add(handler.MenuID, boundingBox);
                    }
                }
            }
            else {
                if (handler.VariantType == VariantType.Compact) {
                    if (boundingBoxCompactObjectMenu.ContainsKey(handler.MenuID)) {
                        boundingBoxCompactObjectMenu[handler.MenuID] = boundingBox;
                    }
                    else {
                        boundingBoxCompactObjectMenu.Add(handler.MenuID, boundingBox);
                    }
                }
                else {
                    if (boundingBoxFloatingObjectMenu.ContainsKey(handler.MenuID)) {
                        boundingBoxFloatingObjectMenu[handler.MenuID] = boundingBox;
                    }
                    else {
                        boundingBoxFloatingObjectMenu.Add(handler.MenuID, boundingBox);
                    }
                }
                
            }
        }

        /// <summary>
        /// Get the stored InBetween position offset of the other menu variant.
        /// </summary>
        /// <param name="menu"> current menu</param>
        /// <returns>the InBetween offset of the other variant</returns>
        public Vector3 GetInBetweenPositionOffsetOppositeVariant(GameObject menu) {
            MenuHandler handler = menu.GetComponent<MenuHandler>();
            Tuple<Vector3, Quaternion, Vector3, float> offset;
            MenuHandler oppositeHandler = SwitchToFloating(menu).GetComponent<MenuHandler>();
            int menuID = oppositeHandler.MenuID;
            if (handler.VariantType == VariantType.Compact) {
                if(oppositeHandler.ManipulationLogic == ManipulationLogic.OneToAll) {
                    if (currentOneToAllOffsetInBetween.TryGetValue(menuID, out offset)) {
                        return offset.Item1;
                    }
                    else {
                        Debug.Log("No lastOffsetInBetween found for menu with menuID " + menuID);
                        return new Vector3();
                    }
                }
                else {
                    return GetCurrentOneToOneOffsetInBetween(handler.TargetObject).Item1;
                }

            }
            else {
                if(oppositeHandler.ManipulationLogic == ManipulationLogic.OneToAll) {
                    if (currentOneToAllOffsetInBetween.TryGetValue(menuID, out offset)) {
                        return offset.Item1;
                    }
                    else {
                        Debug.Log("No lastOffsetInBetween found for menu with menuID " + menuID);
                        return new Vector3();
                    }
                }
                else {
                    return GetCurrentOneToOneOffsetInBetween(handler.TargetObject).Item1;
                }
            }          
        }

        /// <summary>
        /// Get the stored Orbital position offset of the other menu variant (not OrbitalOffset).
        /// </summary>
        /// <param name="menu"> current menu</param>
        /// <returns>the Orbital position offset of the other variant</returns>
        public Vector3 GetOrbitalPositionOffsetOppositeVariant(GameObject menu) {
            MenuHandler handler = menu.GetComponent<MenuHandler>();
            Tuple<Vector3, Quaternion, Vector3, float> offset;
            MenuHandler oppositeHandler = SwitchToFloating(menu).GetComponent<MenuHandler>();
            int menuID = oppositeHandler.MenuID;
            if (handler.VariantType == VariantType.Compact) {
                if(oppositeHandler.ManipulationLogic == ManipulationLogic.OneToAll) {                 
                    if (currentOneToAllOffsetOrbital.TryGetValue(menuID, out offset)) {
                        return offset.Item1;
                    }
                    else {
                        Debug.LogError("No lastOffsetOrbital found for menu with menuID" + menuID);
                        return new Vector3();
                    }
                }
                else {
                    return GetCurrentOneToOneOffsetOrbitalFloating(handler.TargetObject).Item1;
                }

            }
            else {
                if(oppositeHandler.ManipulationLogic == ManipulationLogic.OneToAll) {
                    if (currentOneToAllOffsetOrbital.TryGetValue(menuID, out offset)) {
                        return offset.Item1;
                    }
                    else {
                        Debug.LogError("No lastOffsetOrbital found for menu with menuID" + menuID);
                        return new Vector3();
                    }
                }
                else {
                    return GetCurrentOneToOneOffsetOrbitalCompact(handler.TargetObject).Item1;
                }

            }
        }

        /// <summary>
        /// Get the OrbitalOffset set in the inspector of the other menu variant.
        /// </summary>
        /// <param name="menu"> current menu</param>
        public Vector3 GetOrbitalOffsetOppositeVariant(GameObject menu) {
            MenuHandler handler = menu.GetComponent<MenuHandler>();
            /*if (handler.IsRuntimeMenu) {
                foreach(MenuVariants v in runtimeMenus) {
                    if(handler.VariantType == VariantType.Floating) {
                        if(v.floatingMenu.GetComponent<MenuHandler>().MenuID == handler.MenuID) {
                            return v.compactMenu ? v.compactMenu.GetComponent<MenuHandler>().OrbitalOffset : new Vector3();
                        }
                    }
                    else {
                        if(v.compactMenu.GetComponent<MenuHandler>().MenuID == handler.MenuID) {
                            return v.floatingMenu ? v.compactMenu.GetComponent<MenuHandler>().OrbitalOffset : new Vector3();
                        }
                    }
                }
                Debug.LogError("Runtime Menu with MenuID " + handler.MenuID + "not found.");
                return new Vector3();
            }*/
            /*else {*/
                if (handler.MenuType == MenuType.MainMenu) {
                    if (handler.VariantType == VariantType.Compact) {
                        return mainMenu.floatingMenu ? mainMenu.floatingMenu.GetComponent<MenuHandler>().OrbitalOffset : new Vector3();
                    }
                    else {
                        return mainMenu.compactMenu ? mainMenu.compactMenu.GetComponent<MenuHandler>().OrbitalOffset : new Vector3();
                    }
                }
                else {
                    if (handler.VariantType == VariantType.Compact) {
                        return SwitchToFloating(GetObjectMenuWithID(handler.MenuID)).GetComponent<MenuHandler>().OrbitalOffset;
                    }
                    else {
                        return SwitchToCompact(GetObjectMenuWithID(handler.MenuID)).GetComponent<MenuHandler>().OrbitalOffset;
                    }
                }
            //}
        }
        #endregion Public Methods

        #region Private Methods

        //Check if all menus and their properties are initialized properly.
        private void CheckMenuInitialization() {
            //Check initialization in Menu Placement Service
            List<GameObject> menus = new List<GameObject>();
            if (mainMenu.compactMenu == null) {
                Debug.LogWarning("The compact main menu is not assigned in Menu Placement Service. Make sure you set it correctly unless you don't want to have it.");
            }
            else {
                if (mainMenu.compactMenu.GetComponent<MenuHandler>().VariantType == VariantType.Floating) {
                    Debug.LogError("The compact main menu is not set to 'compact'. Make sure you set it correctly in the insepctor.");
                }
                menus.Add(mainMenu.compactMenu);
            }

            if(mainMenu.floatingMenu == null) {
                Debug.LogWarning("The floating main menu is not assigned in Menu Placement Service. Make sure you set it correctly unless you don't want to have it.");
            }
            else {
                if (mainMenu.floatingMenu.GetComponent<MenuHandler>().VariantType == VariantType.Compact) {
                    Debug.LogError("The floating main menu is set to 'compact'. Make sure you set it correctly in the insepctor");
                }
                menus.Add(mainMenu.floatingMenu);
            }

            if(mainMenu.floatingMenu != null && mainMenu.compactMenu != null) {
                if(mainMenu.floatingMenu.GetComponent<MenuHandler>().MinFloatingDistance != mainMenu.compactMenu.GetComponent<MenuHandler>().MinFloatingDistance) {
                    Debug.LogError("The 'Min Floating Distance' of main menus are not identical");
                }

                if(mainMenu.floatingMenu.GetComponent<MenuHandler>().MaxFloatingDistance != mainMenu.compactMenu.GetComponent<MenuHandler>().MaxFloatingDistance) {
                    Debug.LogError("The 'Max Floating Distance' of main menus are not identical");
                }

                if(mainMenu.floatingMenu.GetComponent<MenuHandler>().DefaultFloatingDistance != mainMenu.compactMenu.GetComponent<MenuHandler>().DefaultFloatingDistance) {
                    Debug.LogError("The 'Default Floating Distance' of main menus are not identical");
                }
            }

            foreach (MenuVariants m in objectMenus){
                if (m.compactMenu == null) {
                    Debug.LogWarning("One compact object menu is not assigned to Menu Placement Service. Make sure you set it correctly unless you don't want to have it.");
                }
                else {
                    if (m.compactMenu.GetComponent<MenuHandler>().VariantType == VariantType.Floating) {
                        Debug.LogError("The compact object menu '" + m.compactMenu + "' is not set to 'compact'. Make sure you set it correctly in the insepctor");
                    }
                    menus.Add(m.compactMenu);
                }

                if (m.floatingMenu == null) {
                    Debug.LogWarning("One floating object menu is not assigned to Menu Placement Service. Make sure you set it correctly unless you don't want to have it.");
                }
                else {
                    if (m.floatingMenu.GetComponent<MenuHandler>().VariantType == VariantType.Compact) {
                        Debug.LogError("The floating object menu '" + m.floatingMenu + "' is set to 'compact'. Make sure you set it correctly in the inspector.");
                    }
                    menus.Add(m.floatingMenu);
                }

                if (m.floatingMenu != null && m.compactMenu != null) {
                    if (m.floatingMenu.GetComponent<MenuHandler>().MinFloatingDistance != m.compactMenu.GetComponent<MenuHandler>().MinFloatingDistance) {
                        Debug.LogError("The 'Min Floating Distance' of the object menus " + m + "are not identical");
                    }

                    if (m.floatingMenu.GetComponent<MenuHandler>().MaxFloatingDistance != m.compactMenu.GetComponent<MenuHandler>().MaxFloatingDistance) {
                        Debug.LogError("The 'Max Floating Distance' of the object menus " + m + "are not identical");
                    }

                    if (m.floatingMenu.GetComponent<MenuHandler>().DefaultFloatingDistance != m.compactMenu.GetComponent<MenuHandler>().DefaultFloatingDistance) {
                        Debug.LogError("The 'Default Floating Distance' of the object menus " + m + "are not identical");
                    }
                }

            }

            foreach (GameObject menu in menus) {
                if (!menu.GetComponent<MenuHandler>()) {
                    Debug.LogError("The menu object '" + menu + "' doesn't have a MenuHandler component. Please make sure every menu object you assigned to Menu Placement Service has a MenuHandler attached to it");
                }
                if(menu.layer != LayerMask.NameToLayer("Menu")) {
                    Debug.LogError("The menu object '" + menu + "' doesn't on the layer 'Menu', please assign it correctly.");
                }
            }

            for (int i = 0; i < menus.Count; i++) {
                for (int j = i + 1; j < menus.Count; j++) {
                    if (menus[i].GetComponent<MenuHandler>().MenuID == menus[j].GetComponent<MenuHandler>().MenuID) {
                        Debug.LogError("The menu objects '" + menus[i] + "' and '" + menus[j] + "' have the same menuID. Make sure every menu object you assigned to Menu Placement Service has an identical menuID.");
                    }
                }
            }

            //Check initialization in scene (if there are unregistered menus)
            MenuHandler[] menuInScene = FindObjectsOfType<MenuHandler>(true);
            foreach (MenuHandler handler in menuInScene) {
                if(handler.MenuType == MenuType.ObjectMenu) {
                    if (GetObjectMenuWithID(handler.MenuID) == null) {
                        Debug.LogError("Unregistered object menus with menuID " + handler.MenuID + " found in scene, please make sure all menus are registered in Menu Placement Service");
                    }
                }
                if(handler.MenuType == MenuType.MainMenu) {
                    if(mainMenu.floatingMenu.GetComponent<MenuHandler>().MenuID != handler.MenuID && mainMenu.compactMenu.GetComponent<MenuHandler>().MenuID != handler.MenuID) {
                        Debug.LogError("Unregistered main menus with menuID " + handler.MenuID + " found in scene, please make sure all menus are registered in Menu Placement Service");
                    }
                }
            }
        }

        private void CreateMenuController() {
            Instantiate(systemControlPanel);
        }
        //make a pool for each MenuVariants and assign the index of the pool to the corresponding variable. Ignore the not assigned menu.
        private void CreateObjectPools() {

            if (mainMenu.floatingMenu != null) {
                floatingMainMenuPoolID = ObjectPool<GameObject>.CreateNewPool(1);
            }
            if (mainMenu.compactMenu != null) {
                compactMainMenuPoolID = ObjectPool<GameObject>.CreateNewPool(1);
            }

            foreach (MenuVariants m in objectMenus) {
                if (m.floatingMenu != null) {
                    floatingObjectMenuPoolIDs.Add(m.floatingMenu.GetComponent<MenuHandler>().MenuID, ObjectPool<GameObject>.CreateNewPool(5));
                }
                if (m.compactMenu != null) {
                    compactObjectMenuPoolIDs.Add(m.compactMenu.GetComponent<MenuHandler>().MenuID, ObjectPool<GameObject>.CreateNewPool(5));
                }
            }
        }

        // Create a target which is lower than head for the InBetween solver on object menus, which can prevent the menu occluding users' sights.
        private void CreateInBetweenTarget() {
            inBetweenTarget = new GameObject("InBetween Target");
            inBetweenTarget.transform.parent = CameraCache.Main.transform;
            inBetweenTarget.transform.position = new Vector3(CameraCache.Main.transform.position.x, CameraCache.Main.transform.position.y - 0.15f, CameraCache.Main.transform.position.z);
        }

        private void InitializeProperties() {
            if (defaultPlacementMode == DefaultMode.Automatic) {
                placementMode = MenuPlacementMode.Automatic;
            }
            else {
                placementMode = MenuPlacementMode.Manual;
            }
            adjustmentModeSemaphore = 0;
            SuggestionPanelOn = false;
            //Compute maximumMenuID
            if(mainMenu.floatingMenu != null) {
                maximumMenuID = maximumMenuID < mainMenu.floatingMenu.GetComponent<MenuHandler>().MenuID ? mainMenu.floatingMenu.GetComponent<MenuHandler>().MenuID : maximumMenuID;
            }
            if(mainMenu.compactMenu != null) {
                maximumMenuID = maximumMenuID < mainMenu.compactMenu.GetComponent<MenuHandler>().MenuID ? mainMenu.compactMenu.GetComponent<MenuHandler>().MenuID : maximumMenuID;
            }
            foreach (MenuVariants v in objectMenus) {
                maximumMenuID = maximumMenuID < v.floatingMenu.GetComponent<MenuHandler>().MenuID ? v.floatingMenu.GetComponent<MenuHandler>().MenuID : maximumMenuID;
                maximumMenuID = maximumMenuID < v.compactMenu.GetComponent<MenuHandler>().MenuID ? v.compactMenu.GetComponent<MenuHandler>().MenuID : maximumMenuID;
            }
            if (!addingRuntimeMenuEnabled) {
                if(systemControlPanel.transform.Find("Advanced Option Panel/Scrolling Object Collection/Container/Buttons/Add Runtime Menu Button")) {
                    systemControlPanel.transform.Find("Advanced Option Panel/Scrolling Object Collection/Container/Buttons/Add Runtime Menu Button").gameObject.SetActive(false);
                }
            }
            else {
                if (systemControlPanel.transform.Find("Advanced Option Panel/Scrolling Object Collection/Container/Buttons/Add Runtime Menu Button")) {
                    systemControlPanel.transform.Find("Advanced Option Panel/Scrolling Object Collection/Container/Buttons/Add Runtime Menu Button").gameObject.SetActive(true);
                }
            }
            systemControlPanel.transform.Find("Advanced Option Panel/Scrolling Object Collection/Container/Buttons").gameObject.GetComponent<GridObjectCollection>().UpdateCollection();
        }

        private void InitializeMainMenuBuffers() {
            if (mainMenu.floatingMenu != null) {
                retrieveBufferFloatingMainMenu = new List<Tuple<Vector3, Quaternion, Vector3, float>>(mainMenu.floatingMenu.GetComponent<MenuHandler>().RetrieveBufferSize);
            }
            if (mainMenu.compactMenu != null) {
                retrieveBufferCompactMainMenu = new List<Tuple<Vector3, Quaternion, Vector3, float>>(mainMenu.compactMenu.GetComponent<MenuHandler>().RetrieveBufferSize);
            }
        }

        // initialize the retrieve buffers for main menus and object menus with "OneToAll" logic
        private void InitializeOneToAllRetrieveBuffers() { 
            //If the logic is OneToOne, the buffers will be linked to the object but not the menu, so we don't consider it here.
            foreach(MenuVariants v in objectMenus) {
                if (v.floatingMenu != null) {
                    if (v.floatingMenu.GetComponent<MenuHandler>().ManipulationLogic == ManipulationLogic.OneToAll) {
                        retrieveBufferOneToAllOrbital.Add(v.floatingMenu.GetComponent<MenuHandler>().MenuID, new List<Tuple<Vector3, Quaternion, Vector3, float>>(v.floatingMenu.GetComponent<MenuHandler>().RetrieveBufferSize));
                        retrieveBufferOneToAllWithoutOrbital.Add(v.floatingMenu.GetComponent<MenuHandler>().MenuID, new List<Tuple<Vector3, Quaternion, Vector3, float>>(v.floatingMenu.GetComponent<MenuHandler>().RetrieveBufferSize));
                    }
                }           
                if(v.compactMenu != null) {
                    if (v.compactMenu.GetComponent<MenuHandler>().ManipulationLogic == ManipulationLogic.OneToAll) {
                        retrieveBufferOneToAllOrbital.Add(v.compactMenu.GetComponent<MenuHandler>().MenuID, new List<Tuple<Vector3, Quaternion, Vector3, float>>(v.compactMenu.GetComponent<MenuHandler>().RetrieveBufferSize));
                        retrieveBufferOneToAllWithoutOrbital.Add(v.compactMenu.GetComponent<MenuHandler>().MenuID, new List<Tuple<Vector3, Quaternion, Vector3, float>>(v.compactMenu.GetComponent<MenuHandler>().RetrieveBufferSize));
                    }
                }
            }
        }

        private void InitializeCurrentOneToAllOffsets() {
            foreach(MenuVariants v in objectMenus) {
                if(v.floatingMenu != null) {
                    if(v.floatingMenu.GetComponent<MenuHandler>().ManipulationLogic == ManipulationLogic.OneToAll) {
                        currentOneToAllOffsetInBetween.Add(v.floatingMenu.GetComponent<MenuHandler>().MenuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, v.floatingMenu.GetComponent<MenuHandler>().DefaultTargetViewPercentV));
                        currentOneToAllOffsetOrbital.Add(v.floatingMenu.GetComponent<MenuHandler>().MenuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, v.floatingMenu.GetComponent<MenuHandler>().DefaultTargetViewPercentV));
                        currentOneToAllOffsetHandConstraint.Add(v.floatingMenu.GetComponent<MenuHandler>().MenuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, v.floatingMenu.GetComponent<MenuHandler>().DefaultTargetViewPercentV));
                    }
                }
                if (v.compactMenu != null) {
                    if (v.compactMenu.GetComponent<MenuHandler>().ManipulationLogic == ManipulationLogic.OneToAll) {
                        currentOneToAllOffsetInBetween.Add(v.compactMenu.GetComponent<MenuHandler>().MenuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, v.compactMenu.GetComponent<MenuHandler>().DefaultTargetViewPercentV));
                        currentOneToAllOffsetOrbital.Add(v.compactMenu.GetComponent<MenuHandler>().MenuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, v.compactMenu.GetComponent<MenuHandler>().DefaultTargetViewPercentV));
                        currentOneToAllOffsetHandConstraint.Add(v.compactMenu.GetComponent<MenuHandler>().MenuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, v.compactMenu.GetComponent<MenuHandler>().DefaultTargetViewPercentV));
                    }
                }
            }
        }

        private void InitializeRuntimeMenus() {
            runtimeMenus = new List<MenuVariants>();
        }

        private GameObject GetObjectMenuWithID(int menuID) {
            foreach(MenuVariants v in objectMenus) {
                if(v.floatingMenu && v.floatingMenu.GetComponent<MenuHandler>().MenuID == menuID) {
                    return v.floatingMenu;
                }
                if(v.compactMenu && v.compactMenu.GetComponent<MenuHandler>().MenuID == menuID) {
                    return v.compactMenu;
                }
            }
            foreach(MenuVariants v in runtimeMenus) {
                if(v.floatingMenu && v.floatingMenu.GetComponent<MenuHandler>().MenuID == menuID) {
                    return v.floatingMenu;
                }
                if(v.compactMenu && v.compactMenu.GetComponent<MenuHandler>().MenuID == menuID) {
                    return v.compactMenu;
                }
            }
            Debug.LogError("The menu with menuID = " + menuID + " cannot be found");
            return null;
        }

        //Check if a menu is a clone of an origin through menuID.
        private bool isSameMenu(GameObject origin, GameObject clone) {
            if(origin.GetComponent<MenuHandler>().MenuID == clone.GetComponent<MenuHandler>().MenuID) {
                return true;
            }
            else {
                return false;
            }

        }

        private GameObject SwitchToCompact(GameObject menu) {
            if (menu.GetComponent<MenuHandler>().IsRuntimeMenu) {
                foreach (MenuVariants v in runtimeMenus) {
                    if (isSameMenu(v.floatingMenu, menu)) {
                        if(v.compactMenu != null) {
                            return v.compactMenu;
                        }
                    }
                }
                //no compact variant
                return menu;
            }
            else {
                if (menu.GetComponent<MenuHandler>().MenuType == MenuType.ObjectMenu) {
                    foreach (MenuVariants v in objectMenus) {
                        if (isSameMenu(v.floatingMenu, menu)) {
                            if (v.compactMenu != null) {
                                return v.compactMenu;
                            }

                        }
                    }
                }
                else {
                    if (mainMenu.compactMenu != null) {
                        return mainMenu.compactMenu;
                    }
                }
                //No compact variant
                return menu;
            }

        }

        private GameObject SwitchToFloating(GameObject menu) {
            if (menu.GetComponent<MenuHandler>().IsRuntimeMenu) {
                foreach (MenuVariants v in runtimeMenus) {
                    if (isSameMenu(v.compactMenu, menu)) {
                        if (v.floatingMenu != null) {
                            return v.floatingMenu;
                        }
                    }
                }
                //no compact variant
                return menu;
            }
            else {
                if (menu.GetComponent<MenuHandler>().MenuType == MenuType.ObjectMenu) {
                    foreach (MenuVariants v in objectMenus) {
                        if (isSameMenu(v.compactMenu, menu)) {
                            if (v.floatingMenu != null) {
                                return v.floatingMenu;
                            }
                        }
                    }
                }
                else {
                    if (mainMenu.floatingMenu != null) {
                        return mainMenu.floatingMenu;
                    }
                }
                //No floating variant
                return menu;
            }

        }

        //Check available capabilities of the current ruuning platform
        private void CheckCapability() {
            if (CoreServices.InputSystem is IMixedRealityCapabilityCheck capabilityChecker) {
                ArticulatedHandSupported = capabilityChecker.CheckCapability(MixedRealityCapability.ArticulatedHand);
                MotionControllerSupported = capabilityChecker.CheckCapability(MixedRealityCapability.MotionController);
                GGVHandSupported = capabilityChecker.CheckCapability(MixedRealityCapability.GGVHand);
                if (ArticulatedHandSupported || MotionControllerSupported) {
                    HandTrackingEnabled = true;
                }
                else {
                    HandTrackingEnabled = false;
                }
            }
        }

        #endregion Private Methods

    }
}
