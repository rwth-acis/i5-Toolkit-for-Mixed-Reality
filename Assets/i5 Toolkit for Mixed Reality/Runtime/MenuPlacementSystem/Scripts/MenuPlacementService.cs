using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.Utilities;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace i5.Toolkit.MixedReality.MenuPlacementSystem {
    /// <summary>
    /// The central component of the Menu Placement Service.
    /// This service should be initialized at the start of the application. 
    /// It has the references to all menu objects and stores some data of them. 
    /// The Menu Handler component should call its methods when needed.
    /// </summary>
    [CreateAssetMenu(menuName = "i5 Mixed Reality Toolkit/Menu Placement Service")]
    public class MenuPlacementService : ScriptableObject, IService {

        #region Enums
        public enum MenuPlacementServiceMode {
            Automatic,
            Manual,
            //The adjustment mode is just needed when the app bar is expended and should not be visible to the users.
            Adjustment
        }

        private enum DefaultMode {
            Automatic,
            Manual
        }
        #endregion

        #region Serizalize Fields
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
        #endregion

        #region Non-serializable Properties

        private Bounds floatingMainMenuBoundingBox;
        private Bounds compactMainMenuBoundingBox;

        //Use the corresponding ID of a MenuVariants to get the ObjectPool for it.
        private int floatingMainMenuPoolID;
        private int compactMainMenuPoolID;
        //Dictionary<menuID, menuPoolID>
        private Dictionary<int, int> floatingObjectMenuPoolIDs = new Dictionary<int, int>();
        private Dictionary<int, int> compactObjectMenuPoolIDs = new Dictionary<int, int>();
        //Buffers for main menus
        private List<Tuple<Vector3, Quaternion, Vector3, float>> retrieveBufferFloatingMainMenu = new List<Tuple<Vector3, Quaternion, Vector3, float>>();
        private List<Tuple<Vector3, Quaternion, Vector3, float>> retrieveBufferCompactMainMenu = new List<Tuple<Vector3, Quaternion, Vector3, float>>();
        //For ManipulationLogic = OneToAll, the dictionaries will be read and written by all menus
        //Dictionary<menuID, Bounds>
        private Dictionary<int, Bounds> boundingBoxFloatingObjectMenu = new Dictionary<int, Bounds>();
        private Dictionary<int, Bounds> boundingBoxCompactObjectMenu = new Dictionary<int, Bounds>();
        //Dictionary<targetObject, Bounds>
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
        private MenuPlacementServiceMode placementMode;
        // A counter for the expanded app bars.
        private int adjustmentModeSemaphore = 0;
        public bool SuggestionPanelOn { get; set; }
        /// <summary>
        /// The previous mode. Should only be called in adjustment mode.
        /// </summary>
        public MenuPlacementServiceMode PreviousMode { get; set; }

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
        #endregion

        #region Getters & Setters
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

        /// <summary>
        /// Current placement mode.
        /// </summary>
        public MenuPlacementServiceMode PlacementMode
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
        #endregion

        #region IService Methods

        public void Initialize(IServiceManager owner) {
            CheckMenuInitialization();
            CreateObjectPools();
            InitializeProperties();
            InitializeMainMenuBuffers();
            InitializeOneToAllRetrieveBuffers();
            InitializeCurrentOneToAllOffsets();
            CreateInBetweenTarget();
            CreateMenuController();
            CheckCapability();
        }

        public void Cleanup() {
            InitializeProperties();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the InBewteen target, which is an empty objects slightly below the head.
        /// </summary>
        public GameObject GetInBetweenTarget() {
            return inBetweenTarget;
        }
        
        public void Quit() {
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
            if(placementMode == MenuPlacementServiceMode.Automatic) {
                placementMode = MenuPlacementServiceMode.Manual;
            }
            else {
                placementMode = MenuPlacementServiceMode.Automatic;
            }
        }
        
        public void EnterManualMode() {
            placementMode = MenuPlacementServiceMode.Manual;
        }

        public void EnterAutomaticMode() {
            placementMode = MenuPlacementServiceMode.Automatic;
        }

        public void EnterAdjustmentMode() {
            if(adjustmentModeSemaphore == 0) {
                PreviousMode = placementMode;
                placementMode = MenuPlacementServiceMode.Adjustment;
            }
            adjustmentModeSemaphore++;
        }

        public void ExitAdjustmentMode() {
            adjustmentModeSemaphore--;
            if (adjustmentModeSemaphore < 0) {
                Debug.LogError("AdjustmentModeSemaphore is smaller than 0, it has value " + adjustmentModeSemaphore + ". It is forced reset to 0.");
                adjustmentModeSemaphore = 0;
            }
            if(adjustmentModeSemaphore == 0) {
                placementMode = PreviousMode;
            }
        }
        /// <summary>
        /// Get the retrieve buffer with given parameters, you can leave certain parameters null or set them casually if they are not important to identify the buffer. Select solverName from Orbital, InBetween,and HandConstraint.
        /// </summary>
        /// <param name="solverName">Select from Orbital, InBetween, HandConstraint</param>
        public List<Tuple<Vector3, Quaternion, Vector3, float>> GetRetrieveBuffer(MenuHandler.MenuVariantType variant, MenuHandler.MenuManipulationLogic manipulationLogic, bool compact, int menuID, string solverName, GameObject targetObject) {
            switch (variant) {
                case MenuHandler.MenuVariantType.MainMenu:
                    return GetRetrieveBufferMainMenu(menuID);
                case MenuHandler.MenuVariantType.ObjectMenu:
                    switch (manipulationLogic) {
                        case MenuHandler.MenuManipulationLogic.OneToAll:
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
                        case MenuHandler.MenuManipulationLogic.OneToOne:
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
        public Tuple<Vector3, Quaternion, Vector3, float> GetOffset(MenuHandler.MenuManipulationLogic manipulationLogic, bool compact, int menuID, string solverName, GameObject targetObject) {
            switch (manipulationLogic) {
                case MenuHandler.MenuManipulationLogic.OneToAll:
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
                case MenuHandler.MenuManipulationLogic.OneToOne:
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
            if(menuID == mainMenu.floatingMenu.GetComponent<MenuHandler>().menuID) {
                return retrieveBufferFloatingMainMenu;
            }
            else {
                return retrieveBufferCompactMainMenu;
            }
        }

        public List<Tuple<Vector3, Quaternion, Vector3, float>> GetRetrieveBufferOneToAllWithoutOrbital(GameObject menu) {
            int menuID = menu.GetComponent<MenuHandler>().menuID;
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
            int menuID = menu.GetComponent<MenuHandler>().menuID;
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
            return GetCurrentOneToAllOffsetInBetween(menu.GetComponent<MenuHandler>().menuID);
        }

        public void SetCurrentOneToAllOffsetInBetween(int menuID, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            currentOneToAllOffsetInBetween[menuID] = offset;
        }

        public void SetCurrentOneToAllOffsetInBetween(GameObject menu, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            SetCurrentOneToAllOffsetInBetween(menu.GetComponent<MenuHandler>().menuID, offset);
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
            return GetCurrentOneToAllOffsetOrbital(menu.GetComponent<MenuHandler>().menuID);
        }

        public void SetCurrentOneToAllOffsetOrbital(int menuID, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            currentOneToAllOffsetOrbital[menuID] = offset;
        }

        public void SetCurrentOneToAllOffsetOrbital(GameObject menu, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            SetCurrentOneToAllOffsetOrbital(menu.GetComponent<MenuHandler>().menuID, offset);
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
            return GetCurrentOneToAllOffsetHandConstraint(menu.GetComponent<MenuHandler>().menuID);
        }

        public void SetCurrentOneToAllOffsetHandConstraint(int menuID, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            currentOneToAllOffsetHandConstraint[menuID] = offset;
        }

        public void SetCurrentOneToAllOffsetHandConstraint(GameObject menu, Tuple<Vector3, Quaternion, Vector3, float> offset) {
            SetCurrentOneToAllOffsetHandConstraint(menu.GetComponent<MenuHandler>().menuID, offset);
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
        /// Fetch a menu object from the ObjectPool of the origin menu
        /// </summary>
        public GameObject InstantiateMenu(GameObject origin) {
            MenuHandler handler = origin.GetComponent<MenuHandler>();
            if (handler.menuVariantType == MenuHandler.MenuVariantType.MainMenu) {
                if (handler.compact) {
                    return ObjectPool<GameObject>.RequestResource(compactMainMenuPoolID, () => { return Instantiate(mainMenu.compactMenu); });
                }
                else {
                    return ObjectPool<GameObject>.RequestResource(floatingMainMenuPoolID, () => { return Instantiate(mainMenu.floatingMenu); });
                }
            }
            else {
                if (handler.compact) {
                    return ObjectPool<GameObject>.RequestResource(compactObjectMenuPoolIDs[handler.menuID], () => { return Instantiate(GetObjectMenuWithID(handler.menuID)); });
                }
                else {
                    return ObjectPool<GameObject>.RequestResource(floatingObjectMenuPoolIDs[handler.menuID], () => { return Instantiate(GetObjectMenuWithID(handler.menuID)); });
                }  
            }
        }

        /// <summary>
        /// Return a menu object to its ObjectPool
        /// </summary>
        public void CloseMenu(GameObject menu) {
            MenuHandler handler = menu.GetComponent<MenuHandler>();
            if (handler.menuVariantType == MenuHandler.MenuVariantType.MainMenu) {
                if (handler.compact) {
                    Debug.Log("Close Compact Main Menu");
                    ObjectPool<GameObject>.ReleaseResource(compactMainMenuPoolID, menu);
                }
                else {
                    ObjectPool<GameObject>.ReleaseResource(floatingMainMenuPoolID, menu);
                }    
            }
            else {
                if (handler.compact) {
                    ObjectPool<GameObject>.ReleaseResource(compactObjectMenuPoolIDs[handler.menuID], menu);
                }
                else {
                    ObjectPool<GameObject>.ReleaseResource(floatingObjectMenuPoolIDs[handler.menuID], menu);
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
                    if (!menu.GetComponent<MenuHandler>().compact) {
                        if (SwitchToCompact(menu) != menu) {
                            menu.GetComponent<MenuHandler>().Close();
                            SwitchToCompact(menu).GetComponent<MenuHandler>().Open(menu.GetComponent<MenuHandler>().TargetObject);
                        }
                    }
                    break;
                case PlacementMessage.SwitchType.CompactToFloating:
                    //Debug.Log("The menu is compact: " + menu.GetComponent<MenuHandler>().compact);
                    if (menu.GetComponent<MenuHandler>().compact) {
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
            if (handler.menuVariantType == MenuHandler.MenuVariantType.MainMenu) {
                if (handler.compact) {
                    return floatingMainMenuBoundingBox;
                }
                else {                  
                    return compactMainMenuBoundingBox;
                }
            }
            else {
                Bounds res;
                if (handler.compact) {
                    int menuID = SwitchToFloating(menu).GetComponent<MenuHandler>().menuID;
                    if (boundingBoxFloatingObjectMenu.TryGetValue(menuID, out res)){
                        return res;
                    }
                    else {
                        Debug.LogError("Bounding Box Not Fund");
                        return new Bounds();
                    }
                }
                else {
                    int menuID = SwitchToCompact(menu).GetComponent<MenuHandler>().menuID;
                    if(boundingBoxCompactObjectMenu.TryGetValue(menuID, out res)) {
                        return res;
                    }
                    else{
                        Debug.LogError("Bounding Box Not Fund");
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
            if (handler.menuVariantType == MenuHandler.MenuVariantType.MainMenu) {
                if (handler.compact) {
                    compactMainMenuBoundingBox = boundingBox;
                }
                else {
                    floatingMainMenuBoundingBox = boundingBox;
                }
            }
            else {
                if (handler.compact) {
                    if (boundingBoxCompactObjectMenu.ContainsKey(handler.menuID)) {
                        boundingBoxCompactObjectMenu[handler.menuID] = boundingBox;
                    }
                    else {
                        boundingBoxCompactObjectMenu.Add(handler.menuID, boundingBox);
                    }
                }
                else {
                    if (boundingBoxFloatingObjectMenu.ContainsKey(handler.menuID)) {
                        boundingBoxFloatingObjectMenu[handler.menuID] = boundingBox;
                    }
                    else {
                        boundingBoxFloatingObjectMenu.Add(handler.menuID, boundingBox);
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
            int menuID = oppositeHandler.menuID;
            if (handler.compact) {
                if(oppositeHandler.ManipulationLogic == MenuHandler.MenuManipulationLogic.OneToAll) {
                    if (currentOneToAllOffsetInBetween.TryGetValue(menuID, out offset)) {
                        return offset.Item1;
                    }
                    else {
                        Debug.LogError("No lastOffsetInBetween found for menu with menuID" + menuID);
                        return new Vector3();
                    }
                }
                else {
                    return GetCurrentOneToOneOffsetInBetween(handler.TargetObject).Item1;
                }

            }
            else {
                if(oppositeHandler.ManipulationLogic == MenuHandler.MenuManipulationLogic.OneToAll) {
                    if (currentOneToAllOffsetInBetween.TryGetValue(menuID, out offset)) {
                        return offset.Item1;
                    }
                    else {
                        Debug.LogError("No lastOffsetInBetween found for menu with menuID" + menuID);
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
            int menuID = oppositeHandler.menuID;
            if (handler.compact) {
                if(oppositeHandler.ManipulationLogic == MenuHandler.MenuManipulationLogic.OneToAll) {                 
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
                if(oppositeHandler.ManipulationLogic == MenuHandler.MenuManipulationLogic.OneToAll) {
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
            if (handler.menuVariantType == MenuHandler.MenuVariantType.MainMenu) {
                if (handler.compact) {
                    return mainMenu.floatingMenu.GetComponent<MenuHandler>().OrbitalOffset;
                }
                else {
                    return mainMenu.compactMenu.GetComponent<MenuHandler>().OrbitalOffset;
                }
            }
            else {
                if (handler.compact) {
                    return SwitchToFloating(GetObjectMenuWithID(handler.menuID)).GetComponent<MenuHandler>().OrbitalOffset;
                }
                else {
                    return SwitchToCompact(GetObjectMenuWithID(handler.menuID)).GetComponent<MenuHandler>().OrbitalOffset;
                }
            }
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
                if (!mainMenu.compactMenu.GetComponent<MenuHandler>().compact) {
                    Debug.LogError("The compact main menu is not set to 'compact'. Make sure you set it correctly in the insepctor.");
                }
                menus.Add(mainMenu.compactMenu);
            }

            if(mainMenu.floatingMenu == null) {
                Debug.LogWarning("The floating main menu is not assigned in Menu Placement Service. Make sure you set it correctly unless you don't want to have it.");
            }
            else {
                if (mainMenu.floatingMenu.GetComponent<MenuHandler>().compact) {
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
                    if (!m.compactMenu.GetComponent<MenuHandler>().compact) {
                        Debug.LogError("The compact object menu '" + m.compactMenu + "' is not set to 'compact'. Make sure you set it correctly in the insepctor");
                    }
                    menus.Add(m.compactMenu);
                }

                if (m.floatingMenu == null) {
                    Debug.LogWarning("One floating object menu is not assigned to Menu Placement Service. Make sure you set it correctly unless you don't want to have it.");
                }
                else {
                    if (m.floatingMenu.GetComponent<MenuHandler>().compact) {
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
                    if (menus[i].GetComponent<MenuHandler>().menuID == menus[j].GetComponent<MenuHandler>().menuID) {
                        Debug.LogError("The menu objects '" + menus[i] + "' and '" + menus[j] + "' have the same menuID. Make sure every menu object you assigned to Menu Placement Service has an identical menuID.");
                    }
                }
            }

            //Check initialization in scene (if there are unregistered menus)
            MenuHandler[] menuInScene = FindObjectsOfType<MenuHandler>(true);
            foreach (MenuHandler handler in menuInScene) {
                if(handler.menuVariantType == MenuHandler.MenuVariantType.ObjectMenu) {
                    if (GetObjectMenuWithID(handler.menuID) == null) {
                        Debug.LogError("Unregistered object menus with menuID " + handler.menuID + " found in scene, please make sure all menus are registered in Menu Placement Service");
                    }
                }
                if(handler.menuVariantType == MenuHandler.MenuVariantType.MainMenu) {
                    if(mainMenu.floatingMenu.GetComponent<MenuHandler>().menuID != handler.menuID && mainMenu.compactMenu.GetComponent<MenuHandler>().menuID != handler.menuID) {
                        Debug.LogError("Unregistered main menus with menuID " + handler.menuID + " found in scene, please make sure all menus are registered in Menu Placement Service");
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
                    floatingObjectMenuPoolIDs.Add(m.floatingMenu.GetComponent<MenuHandler>().menuID, ObjectPool<GameObject>.CreateNewPool(5));
                }
                if (m.compactMenu != null) {
                    compactObjectMenuPoolIDs.Add(m.compactMenu.GetComponent<MenuHandler>().menuID, ObjectPool<GameObject>.CreateNewPool(5));
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
                placementMode = MenuPlacementServiceMode.Automatic;
            }
            else {
                placementMode = MenuPlacementServiceMode.Manual;
            }
            adjustmentModeSemaphore = 0;
            SuggestionPanelOn = false;
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
                    if (v.floatingMenu.GetComponent<MenuHandler>().ManipulationLogic == MenuHandler.MenuManipulationLogic.OneToAll) {
                        retrieveBufferOneToAllOrbital.Add(v.floatingMenu.GetComponent<MenuHandler>().menuID, new List<Tuple<Vector3, Quaternion, Vector3, float>>(v.floatingMenu.GetComponent<MenuHandler>().RetrieveBufferSize));
                        retrieveBufferOneToAllWithoutOrbital.Add(v.floatingMenu.GetComponent<MenuHandler>().menuID, new List<Tuple<Vector3, Quaternion, Vector3, float>>(v.floatingMenu.GetComponent<MenuHandler>().RetrieveBufferSize));
                    }
                }           
                if(v.compactMenu != null) {
                    if (v.compactMenu.GetComponent<MenuHandler>().ManipulationLogic == MenuHandler.MenuManipulationLogic.OneToAll) {
                        retrieveBufferOneToAllOrbital.Add(v.compactMenu.GetComponent<MenuHandler>().menuID, new List<Tuple<Vector3, Quaternion, Vector3, float>>(v.compactMenu.GetComponent<MenuHandler>().RetrieveBufferSize));
                        retrieveBufferOneToAllWithoutOrbital.Add(v.compactMenu.GetComponent<MenuHandler>().menuID, new List<Tuple<Vector3, Quaternion, Vector3, float>>(v.compactMenu.GetComponent<MenuHandler>().RetrieveBufferSize));
                    }
                }
            }
        }

        private void InitializeCurrentOneToAllOffsets() {
            foreach(MenuVariants v in objectMenus) {
                if(v.floatingMenu != null) {
                    if(v.floatingMenu.GetComponent<MenuHandler>().ManipulationLogic == MenuHandler.MenuManipulationLogic.OneToAll) {
                        currentOneToAllOffsetInBetween.Add(v.floatingMenu.GetComponent<MenuHandler>().menuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, v.floatingMenu.GetComponent<MenuHandler>().DefaultTargetViewPercentV));
                        currentOneToAllOffsetOrbital.Add(v.floatingMenu.GetComponent<MenuHandler>().menuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, v.floatingMenu.GetComponent<MenuHandler>().DefaultTargetViewPercentV));
                        currentOneToAllOffsetHandConstraint.Add(v.floatingMenu.GetComponent<MenuHandler>().menuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, v.floatingMenu.GetComponent<MenuHandler>().DefaultTargetViewPercentV));
                    }
                }
                if (v.compactMenu != null) {
                    if (v.compactMenu.GetComponent<MenuHandler>().ManipulationLogic == MenuHandler.MenuManipulationLogic.OneToAll) {
                        currentOneToAllOffsetInBetween.Add(v.compactMenu.GetComponent<MenuHandler>().menuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, v.compactMenu.GetComponent<MenuHandler>().DefaultTargetViewPercentV));
                        currentOneToAllOffsetOrbital.Add(v.compactMenu.GetComponent<MenuHandler>().menuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, v.compactMenu.GetComponent<MenuHandler>().DefaultTargetViewPercentV));
                        currentOneToAllOffsetHandConstraint.Add(v.compactMenu.GetComponent<MenuHandler>().menuID, new Tuple<Vector3, Quaternion, Vector3, float>(Vector3.zero, Quaternion.identity, Vector3.one, v.compactMenu.GetComponent<MenuHandler>().DefaultTargetViewPercentV));
                    }
                }
            }
        }

        private GameObject GetObjectMenuWithID(int menuID) {
            foreach(MenuVariants v in objectMenus) {
                if(v.floatingMenu.GetComponent<MenuHandler>().menuID == menuID) {
                    return v.floatingMenu;
                }
                if(v.compactMenu.GetComponent<MenuHandler>().menuID == menuID) {
                    return v.compactMenu;
                }
            }
            Debug.LogError("The menu with menuID = " + menuID + " cannot be found");
            return null;
        }

        //Check if a menu is a clone of an origin through menuID.
        private bool isSameMenu(GameObject origin, GameObject clone) {
            if(origin.GetComponent<MenuHandler>().menuID == clone.GetComponent<MenuHandler>().menuID) {
                return true;
            }
            else {
                return false;
            }

        }

        private GameObject SwitchToCompact(GameObject menu) {
            if (menu.GetComponent<MenuHandler>().menuVariantType == MenuHandler.MenuVariantType.ObjectMenu) {
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

        private GameObject SwitchToFloating(GameObject menu) {
            if (menu.GetComponent<MenuHandler>().menuVariantType == MenuHandler.MenuVariantType.ObjectMenu) {
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
