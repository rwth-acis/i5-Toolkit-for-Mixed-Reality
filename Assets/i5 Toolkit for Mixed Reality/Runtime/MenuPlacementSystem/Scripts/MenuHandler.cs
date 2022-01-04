using System;
using System.Collections.Generic;
using System.Linq;
using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;


namespace i5.Toolkit.MixedReality.MenuPlacementSystem {
    public class MenuHandler : MonoBehaviour {
        #region Enums
        //The assignment of manipulation offsets of object menus
        public enum MenuManipulationLogic {
            OneToAll, //different objects have the same offset for object menus
            OneToOne  //every object has its own offset for the object menu assigned to it.
        }

        public enum BoundsType {
            BasedOnColliders,
            BasedOnRenderers
        }

        public enum MenuOrientationType {
            CameraAligned,
            Unmodified
        }

        public enum MenuVariantType {
            MainMenu,
            ObjectMenu
        }

        public enum Handedness {
            Left,
            Right
        }
        #endregion

        #region Serialize Fields
        [Header("General Properties")]
        [Tooltip("True, if the menu is an object menu. False, if the menu is a main menu")]
        public MenuVariantType menuVariantType;
        [Tooltip("Is the menu a normal one or a compact one?")]
        public bool compact;
        [Tooltip("The ID of this menu. Make sure they are different from one to another")]
        public int menuID;
        [Tooltip("The orientation type used on the solver attached to the object by the placement system if applicable")]
        public MenuOrientationType menuOrientationType = MenuOrientationType.CameraAligned;
        [Tooltip("If enabled, the menu will be closed automatically if it is not in the users' head gaze for a while. The time threshold can be set in 'Inactivity Time Threshold'.")]
        [SerializeField] private bool inactivityDetectionEnabled = true;
        [Tooltip("If enabled, an App Bar will be instantiate for this menu object on start. For better performance and avoiding potential problems, it is better to add a BoxCollider and a BoundingBox script manually.")]
        [SerializeField] private bool manipulationEnabled = true;
        [Tooltip("If enabled, the 'ConstantViewSize' solver will be added to the menu object, and the scale option of manipulation will be deactivated, instead a slider for 'Target View Percent V' of 'ConstantViewSize;")]
        [SerializeField] private bool constantViewSizeEnabled = true;        
        [Tooltip("The bounding box will be used to decide whether the space is enough to place the menu. It is a 'Bounds' object containing all Bounds of the corresponding base")]
        [SerializeField] private BoundsType boundingBoxType = BoundsType.BasedOnColliders;
        [Tooltip("The default dominant hand which will influence the handedness of HandConstraint. If it is set to LEFT, the menu will be constraint on users' RIGHT hand, and vice versa.")]
        [SerializeField] private Handedness dominantHand = Handedness.Right;

        [Header("Thresholds")]
        [Tooltip("The time between two placement updates")]
        [SerializeField] private float updateTimeInterval = 0.1f;
        [Tooltip("The time threshold for inactivity detection in seconds.")]
        [SerializeField] private float inactivityTimeThreshold = 10;
        [Tooltip("The least time between two suggestion dialogs in manual mode, if any")]
        [SerializeField] private float suggestionTimeInterval = 10f;
        [Tooltip("The numbers of manipulation results stored for retrieving")]
        [SerializeField] private int retrieveBufferSize = 5;

        [Header("Global Offsets")]
        [Tooltip("The max and min floating distance should be indentical for one menu variant (floating and compact)")]
        [SerializeField] private float maxFloatingDistance = 0.6f;
        [SerializeField] private float minFloatingDistance = 0.3f;
        [Tooltip("The distance when the floating menu is first instantiate. If it is smaller than Min Floating Distance, it will be set to the average of Min and Max Floating Distance")]
        [SerializeField] private float defaultFloatingDistance = 0;

        //Constant View Size Offsets
        [Tooltip("The default Target View Percent V of ConstantViewSize")]
        [Range(0f, 1f)]
        [SerializeField] private float defaultTargetViewPercentV = 0.5f;

        //Main Menu Settings
        [Tooltip("Position Offset to the head for floating main menus. It will be directly added to the 'AdditionalOffset' on SolverHandler.")]
        [SerializeField] private Vector3 followOffset = Vector3.zero;
        [Tooltip("Max view degrees in horizontal direction for Follow solver of floating main menus.")]
        [SerializeField] private float followMaxViewHorizontalDegrees = 50f;
        [Tooltip("Max view degrees in vertical direction for Follow solver of floating main menus.")]
        [SerializeField] private float followMaxViewVerticalDegrees = 100f;
        [Tooltip("Offsets for the distance between the surface and the attached menu, i.e. the 'Surface Normal Offset' of the SurfaceMagnetism solver")]
        [SerializeField] private float surfaceMagnetismSafetyOffset = 0.05f;

        //Object Menu Settings
        [Tooltip("Position Offset to the target object. For object menus, the additive inverse of the X (right) offset will be taken if the menu is on the left side of the targetObjects (e.g. 2 to -2).")]
        [SerializeField] private Vector3 orbitalOffset = Vector3.zero;
        [Tooltip("Manipulation logic of object menus in the AUTOMATIC MODE. If \" One To All\", the offsets calculated in manipulation will be assigned to all instances of this menu. If \" One To One\", " +
            "the offsets are binded with the single target object, which means all menu instances assigned to this target object will have the offsets.")]
        [SerializeField] private MenuManipulationLogic manipulationLogic = MenuManipulationLogic.OneToAll;



        #endregion Serialize Fields

        #region Non-serializable Properties
        private MenuPlacementService placementService;
        private PlacementMessage message = new PlacementMessage();
        private PlacementMessage.SwitchType switchTo = PlacementMessage.SwitchType.NoSwitch;
        private Camera head;
        //The OrbitalOffset set in inspector of the opposite type (the other variant)
        private Vector3 orbitalOffsetOppositeType;
        //Expired time in the thresholds.
        private float inactivityTime = 0;
        private float updateTime = 0;
        private float suggestionTime = 0;
        //The suggestionPanel will only be displayed if the collision or occlusion exists for 3 seconds.
        private float accumulatedTimeForSuggestionThreshold = 3f;
        private float accumulatedTimeForSuggestion = 0;
        private GameObject appBar;

        //Tupel<Position, Rotaion, Scale>
        private List<Tuple<Vector3, Quaternion, Vector3>> retrieveBufferManualMode = new List<Tuple<Vector3, Quaternion, Vector3>>();

        private Vector3 manualModePositionOffset;
        private Vector3 manualModeRotationOffset;
        private Vector3 manualModeScaleOffset;
        private Vector3 originalScale;
        private bool manualModeEntered = false;
        #endregion

        #region Getters&Setters

        /// <summary>
        /// The target object of the OBJECT menu.
        /// </summary>
        public GameObject TargetObject { get; private set; }

        /// <summary>
        /// Orbital offset of the OBJECT menu.
        /// </summary>
        public Vector3 OrbitalOffset
        {
            get => orbitalOffset;
            set
            {
                orbitalOffset = value;
            }
        }

        /// <summary>
        /// The dominant hand of the menu (user) set in the inspector. It decides on which hand the compact variant locked.
        /// If the dominant hand is "right", the compact menu will be constraint on the "left" hand.
        /// </summary>
        public Handedness DominantHand
        {
            get => dominantHand;
            set
            {
                dominantHand = value;
            }
        }

        /// <summary>
        /// The manipulation logic set in the inspector.
        /// </summary>
        public MenuManipulationLogic ManipulationLogic
        {
            get => manipulationLogic;
        }

        /// <summary>
        /// The size of the retrieve buffers set in the inspector.
        /// </summary>
        public int RetrieveBufferSize
        {
            get => retrieveBufferSize;
        }

        /// <summary>
        /// If the ConstantViewSize solver is enabled.
        /// </summary>
        public bool ConstantViewSizeEnabled
        {
            get => constantViewSizeEnabled;
        }

        /// <summary>
        /// The minimum floating distance. The menu will generally not be closer than this value.
        /// </summary>
        public float MinFloatingDistance
        {
            get => minFloatingDistance;
        }

        /// <summary>
        /// The maximum floating distance. The menu will generally not be further than this value.
        /// </summary>
        public float MaxFloatingDistance
        {
            get => maxFloatingDistance;
        }

        /// <summary>
        /// The default floating distance. If it is not set explicitly in the inspector, it is the average of min and maxFloatingDistance.
        /// </summary>
        public float DefaultFloatingDistance
        {
            get => defaultFloatingDistance;
        }

        /// <summary>
        /// The object take up this percent vertically in our view (not technically a percent use 0.5 for 50%). Used for ConstantViewSize
        /// </summary>
        public float DefaultTargetViewPercentV
        {
            get => defaultTargetViewPercentV;
        }

        #endregion

        #region MonoBehaviour Functions

        // Start is called before the first frame update
        void Start() {
            if (defaultFloatingDistance < minFloatingDistance || defaultFloatingDistance > maxFloatingDistance) {
                defaultFloatingDistance = (minFloatingDistance + maxFloatingDistance) / 2;
            }

            placementService = ServiceManager.GetService<MenuPlacementService>();
            head = CameraCache.Main;
            if (menuVariantType == MenuVariantType.MainMenu) {
                InitializeMainMenu();
            }
            retrieveBufferManualMode.Capacity = retrieveBufferSize;
            originalScale = gameObject.transform.localScale;
            manualModePositionOffset = new Vector3(0, 0, defaultFloatingDistance);
            manualModeRotationOffset = Vector3.zero;
            manualModeScaleOffset = Vector3.one;
        }

        // Update is called once per frame
        void Update() {
            if(TargetObject == null) {
                if(menuVariantType == MenuVariantType.ObjectMenu) {
                    if(manipulationLogic == MenuManipulationLogic.OneToOne) {
                        placementService.RemoveTargetObject(TargetObject);
                    }
                    Close();
                }   
            }
            if (placementService.PlacementMode == MenuPlacementService.MenuPlacementServiceMode.Automatic) {
                if (manualModeEntered) {
                    ExitManualMode();
                }
                if (updateTime > updateTimeInterval) {                
                    gameObject.GetComponent<SolverHandler>().enabled = true;
                    CheckSpatialMapping();
                    CheckOcclusion();
                    if (switchTo != PlacementMessage.SwitchType.NoSwitch) {
                        placementService.UpdatePlacement(message, gameObject);
                        switchTo = PlacementMessage.SwitchType.NoSwitch;
                    }
                    updateTime = 0;
                }
            }else if(placementService.PlacementMode == MenuPlacementService.MenuPlacementServiceMode.Manual){
                gameObject.GetComponent<SolverHandler>().enabled = false;
                if (!manualModeEntered) {
                    EnterManualMode();
                }
                ShowSuggestion();
            }
            else {
                gameObject.GetComponent<SolverHandler>().enabled = false;
            }
        }


        private void FixedUpdate() {
            if(placementService.PlacementMode == MenuPlacementService.MenuPlacementServiceMode.Automatic) {
                if (inactivityDetectionEnabled) {
                    CheckInactivity();
                }
                if (updateTime <= updateTimeInterval) {
                    updateTime += Time.deltaTime;
                }
            }
            if(placementService.PlacementMode == MenuPlacementService.MenuPlacementServiceMode.Manual) {
                if (suggestionTime <= suggestionTimeInterval) {
                    suggestionTime += Time.deltaTime;
                }
            }

        }

#endregion MonoBehaviour Functions

        #region Public Methods
        /// <summary>
        /// Open the menu given its target object.
        /// </summary>
        /// <param name="targetObject"> The target object controlled by this menu </param>
        public void Open(GameObject targetObject) {
            //Find all menus in the scene. If there is already one menu for the targetObject, no menu will be opened.
            MenuHandler [] menus = FindObjectsOfType<MenuHandler>();
            foreach(MenuHandler m in menus) {
                if (menuVariantType == MenuVariantType.MainMenu) {
                    if(m.menuID == menuID) {
                        return;
                    }
                }
                else {
                    if (m.menuID == menuID && m.TargetObject == targetObject) {
                        return;
                    }
                }
            }
            if (placementService == null) {
                placementService = ServiceManager.GetService<MenuPlacementService>();
            }
            GameObject menu = placementService.InstantiateMenu(gameObject);
            menu.GetComponent<MenuHandler>().orbitalOffsetOppositeType = placementService.GetOrbitalOffsetOppositeVariant(menu);
            menu.GetComponent<MenuHandler>().head = CameraCache.Main;
            menu.GetComponent<MenuHandler>().updateTime = -0.15f; //safty offset
            if (manipulationEnabled) {
                menu.GetComponent<MenuHandler>().InitializeAppBar();
            }
            //Initialize the solvers for object menus.
            if (menuVariantType == MenuVariantType.ObjectMenu) {
                //If manipulation Logic is OneToOne, add buffers and current offsets to the target object
                if(manipulationLogic == MenuManipulationLogic.OneToOne) {
                    if (compact) {
                        placementService.InitializeRetrieveBufferOneToOneOrbitalCompact(targetObject, this);
                        placementService.InitializeRetrieveBufferOneToOneHandConstraint(targetObject, this);
                        placementService.InitializeCurrentOneToOneOffsetOrbitalCompact(targetObject, this);
                        placementService.InitializeCurrentOneToOneOffsetHandConstraint(targetObject, this);
                    }
                    else {
                        placementService.InitializeRetrieveBufferOneToOneOrbitalFloating(targetObject, this);
                        placementService.InitializeRetrieveBufferOneToOneInBetween(targetObject, this);
                        placementService.InitializeCurrentOneToOneOffsetOrbitalFloating(targetObject, this);
                        placementService.InitializeCurrentOneToOneOffsetInBetween(targetObject, this);
                    }
                }
                //To avoid collision at the beginning, or the menu may switch between two variants all the time.
                menu.transform.position = placementService.GetInBetweenTarget().transform.position;
                menu.GetComponent<MenuHandler>().TargetObject = targetObject;
                (menu.GetComponent<SolverHandler>() ?? menu.AddComponent<SolverHandler>()).TrackedTargetType = TrackedObjectType.CustomOverride;
                menu.GetComponent<SolverHandler>().TransformOverride = targetObject.transform;
                float targetDistance = (menu.GetComponent<MenuHandler>().head.transform.position - targetObject.transform.position).magnitude;
                (menu.GetComponent<Orbital>() ?? menu.AddComponent<Orbital>()).UpdateLinkedTransform = true;
                menu.GetComponent<Orbital>().OrientationType = SolverOrientationType.Unmodified;
                menu.GetComponent<Orbital>().LocalOffset = Vector3.zero;
                menu.GetComponent<Orbital>().enabled = true;
                //For floating version, add the InBetween solver additionally for far manipulation
                if (!compact) {                 
                    (menu.GetComponent<InBetween>() ?? menu.AddComponent<InBetween>()).SecondTrackedObjectType = TrackedObjectType.CustomOverride;
                    menu.GetComponent<InBetween>().UpdateLinkedTransform = true;
                    menu.GetComponent<InBetween>().SecondTransformOverride = targetObject.transform;
                    menu.GetComponent<InBetween>().PartwayOffset = 1 - (defaultFloatingDistance / targetDistance);
                    (menu.GetComponent<FinalPlacementOptimizer>() ?? menu.AddComponent<FinalPlacementOptimizer>()).OrbitalOffset = orbitalOffset;
                    menu.GetComponent<FinalPlacementOptimizer>().OrientationType = menuOrientationType;
                    menu.GetComponent<FinalPlacementOptimizer>().enabled = true;
                    menu.GetComponent<FinalPlacementOptimizer>().OriginalScale = gameObject.transform.localScale;
                    (menu.GetComponent<ConstantViewSize>() ?? menu.AddComponent<ConstantViewSize>()).MinDistance = minFloatingDistance;
                    menu.GetComponent<ConstantViewSize>().MaxDistance = MaxFloatingDistance;
                    //restore offsets
                    if (targetDistance > maxFloatingDistance) {
                        Tuple<Vector3, Quaternion, Vector3, float> currentOffsetInBetween;
                        if (menu.GetComponent<MenuHandler>().manipulationLogic == MenuManipulationLogic.OneToAll) {
                            currentOffsetInBetween = placementService.GetCurrentOneToAllOffsetInBetween(menu);
                        }
                        else {
                            currentOffsetInBetween = placementService.GetCurrentOneToOneOffsetInBetween(targetObject);
                        }
                        menu.GetComponent<FinalPlacementOptimizer>().PositionOffset = currentOffsetInBetween.Item1;
                        menu.GetComponent<FinalPlacementOptimizer>().RotationOffset = currentOffsetInBetween.Item2.eulerAngles;
                        menu.GetComponent<FinalPlacementOptimizer>().ScaleOffset = currentOffsetInBetween.Item3;
                        menu.GetComponent<ConstantViewSize>().TargetViewPercentV = currentOffsetInBetween.Item4;
                        menu.GetComponent<SolverHandler>().TransformOverride = placementService.GetInBetweenTarget().transform;
                        menu.GetComponent<Orbital>().enabled = false;
                    }
                    else {
                        Tuple<Vector3, Quaternion, Vector3, float> currentOffsetOrbital;
                        if (menu.GetComponent<MenuHandler>().manipulationLogic == MenuManipulationLogic.OneToAll) {
                            currentOffsetOrbital = placementService.GetCurrentOneToAllOffsetOrbital(menu);
                        }
                        else {
                            currentOffsetOrbital = placementService.GetCurrentOneToOneOffsetOrbitalFloating(targetObject);
                        }
                        menu.GetComponent<FinalPlacementOptimizer>().PositionOffset = currentOffsetOrbital.Item1;
                        menu.GetComponent<FinalPlacementOptimizer>().RotationOffset = currentOffsetOrbital.Item2.eulerAngles;
                        menu.GetComponent<FinalPlacementOptimizer>().ScaleOffset = currentOffsetOrbital.Item3;
                        menu.GetComponent<ConstantViewSize>().TargetViewPercentV = currentOffsetOrbital.Item4;
                        menu.GetComponent<InBetween>().enabled = false;
                    }
                }
                //For compact version, add the HandConstraint solver (or Follow solver) additionally for interaction in very narror space.
                else {
                    if (placementService.HandTrackingEnabled) {
                        (menu.GetComponent<HandConstraint>() ?? menu.AddComponent<HandConstraint>()).SafeZone = HandConstraint.SolverSafeZone.UlnarSide;
                        menu.GetComponent<HandConstraint>().SafeZoneBuffer = 0.05f;
                        menu.GetComponent<HandConstraint>().UpdateLinkedTransform = true;
                        menu.GetComponent<HandConstraint>().UpdateWhenOppositeHandNear = true;
                    }
                    else {
                        (menu.GetComponent<Follow>() ?? menu.AddComponent<Follow>()).MinDistance = 0.3f;
                        menu.GetComponent<Follow>().MaxDistance = 0.3f;
                        menu.GetComponent<Follow>().DefaultDistance = 0.3f;
                        menu.GetComponent<Follow>().MaxViewHorizontalDegrees = 40f;
                        menu.GetComponent<Follow>().MaxViewVerticalDegrees = 40f;
                        menu.GetComponent<Follow>().OrientToControllerDeadzoneDegrees = 20f;
                        menu.GetComponent<Follow>().OrientationType = SolverOrientationType.Unmodified;
                        menu.GetComponent<Follow>().UpdateLinkedTransform = true;
                    }
                    (menu.GetComponent<FinalPlacementOptimizer>() ?? menu.AddComponent<FinalPlacementOptimizer>()).OrbitalOffset = orbitalOffset;
                    menu.GetComponent<FinalPlacementOptimizer>().OrientationType = menuOrientationType;
                    menu.GetComponent<FinalPlacementOptimizer>().enabled = true;
                    menu.GetComponent<FinalPlacementOptimizer>().OriginalScale = gameObject.transform.localScale;
                    (menu.GetComponent<ConstantViewSize>() ?? menu.AddComponent<ConstantViewSize>()).MinDistance = minFloatingDistance;
                    menu.GetComponent<ConstantViewSize>().MaxDistance = maxFloatingDistance;
                    //Switch on solvers according to the targetDistance
                    if (targetDistance > maxFloatingDistance) {
                        if (placementService.HandTrackingEnabled) {
                            if (placementService.ArticulatedHandSupported) {
                                menu.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.HandJoint;
                                if(dominantHand == Handedness.Left) {
                                    menu.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
                                }
                                else {
                                    menu.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;
                                }

                            }
                            else {
                                menu.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.ControllerRay;
                                if (dominantHand == Handedness.Left) {
                                    menu.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
                                }
                                else {
                                    menu.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;
                                }
                            }
                            menu.GetComponent<HandConstraint>().enabled = true;
                        }
                        else {
                            menu.GetComponent<Follow>().enabled = true;
                        }
                        Tuple<Vector3, Quaternion, Vector3, float> currentOffsetHandConstraint;
                        if (menu.GetComponent<MenuHandler>().manipulationLogic == MenuManipulationLogic.OneToAll) {
                            currentOffsetHandConstraint = placementService.GetCurrentOneToAllOffsetHandConstraint(menu);
                        }
                        else {
                            currentOffsetHandConstraint = placementService.GetCurrentOneToOneOffsetHandConstraint(targetObject);
                        }
                        menu.GetComponent<FinalPlacementOptimizer>().PositionOffset = currentOffsetHandConstraint.Item1;
                        menu.GetComponent<FinalPlacementOptimizer>().RotationOffset = currentOffsetHandConstraint.Item2.eulerAngles;
                        menu.GetComponent<FinalPlacementOptimizer>().ScaleOffset = currentOffsetHandConstraint.Item3;
                        menu.GetComponent<ConstantViewSize>().TargetViewPercentV = currentOffsetHandConstraint.Item4;
                        menu.GetComponent<Orbital>().enabled = false;
                    }
                    else {
                        Tuple<Vector3, Quaternion, Vector3, float> currentOffsetOrbital;
                        if (menu.GetComponent<MenuHandler>().manipulationLogic == MenuManipulationLogic.OneToAll) {
                            currentOffsetOrbital = placementService.GetCurrentOneToAllOffsetOrbital(menu);
                        }
                        else {
                            currentOffsetOrbital = placementService.GetCurrentOneToOneOffsetOrbitalCompact(targetObject);
                        }
                        menu.GetComponent<FinalPlacementOptimizer>().PositionOffset = currentOffsetOrbital.Item1;
                        menu.GetComponent<FinalPlacementOptimizer>().RotationOffset = currentOffsetOrbital.Item2.eulerAngles;
                        menu.GetComponent<FinalPlacementOptimizer>().ScaleOffset = currentOffsetOrbital.Item3;
                        menu.GetComponent<ConstantViewSize>().TargetViewPercentV = currentOffsetOrbital.Item4;
                        if (placementService.HandTrackingEnabled) {
                            menu.GetComponent<HandConstraint>().enabled = false;
                        }
                        else {
                            menu.GetComponent<Follow>().enabled = false;
                        }
                    }                    
                }
                menu.GetComponent<ConstantViewSize>().enabled = constantViewSizeEnabled;
            }
            menu.SetActive(true);
            menu.GetComponent<MenuBase>().Initialize();
            if(placementService.PlacementMode == MenuPlacementService.MenuPlacementServiceMode.Manual) {
                menu.GetComponent<MenuHandler>().EnterManualMode();
            }
        }

        /// <summary>
        /// Close the menu object and return it to its object pool.
        /// </summary>
        public void Close() {
            placementService.StoreBoundingBox(gameObject, GetBoundingBox());
            placementService.CloseMenu(gameObject);
            gameObject.GetComponent<MenuBase>().OnClose();
            if (manualModeEntered) {
                ExitManualMode();
            }
            gameObject.SetActive(false);
            if (appBar) {
                appBar.SetActive(false);
            }
        }

        /// <summary>
        /// Save the old offsets of the menu object for later retrieve in the order of position, rotation, scale, and TargetViewPercentV.
        /// </summary>
        public void SaveOffsetBeforeManipulation(Tuple<Vector3, Quaternion, Vector3, float> oldOffsets)
        {      
            if (placementService.PreviousMode == MenuPlacementService.MenuPlacementServiceMode.Automatic) {
                if (menuVariantType == MenuVariantType.ObjectMenu && gameObject.GetComponent<Orbital>() != null && gameObject.GetComponent<Orbital>().enabled) {
                    Vector3 scaleOffset = new Vector3(oldOffsets.Item3.x / originalScale.x, oldOffsets.Item3.y / originalScale.y, oldOffsets.Item3.z / originalScale.z);
                    List<Tuple<Vector3, Quaternion, Vector3, float>> retrieveBufferOrbital;
                    if (manipulationLogic == MenuManipulationLogic.OneToAll) {
                        retrieveBufferOrbital = placementService.GetRetrieveBufferOneToAllOrbital(menuID);
                    }
                    else {
                        if (!compact) {
                            retrieveBufferOrbital = placementService.GetRetrieveBufferOneToOneOrbitalFloating(TargetObject);
                        }
                        else {
                            retrieveBufferOrbital = placementService.GetRetrieveBufferOneToOneOrbitalCompact(TargetObject);
                        }
                    }
                    if (retrieveBufferOrbital.Count == retrieveBufferOrbital.Capacity) {
                        retrieveBufferOrbital.RemoveAt(0);
                    }
                    Tuple<Vector3, Quaternion, Vector3, float> oldOffset = new Tuple<Vector3, Quaternion, Vector3, float>(gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset, Quaternion.Euler(gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset), scaleOffset, oldOffsets.Item4);
                    retrieveBufferOrbital.Add(oldOffset);
                }
                else {
                    Vector3 scaleOffset = new Vector3(oldOffsets.Item3.x / originalScale.x, oldOffsets.Item3.y / originalScale.y, oldOffsets.Item3.z / originalScale.z);
                    if(menuVariantType == MenuVariantType.MainMenu) {
                        List<Tuple<Vector3, Quaternion, Vector3, float>> retrieveBufferMainMenu = placementService.GetRetrieveBufferMainMenu(menuID);
                        if (retrieveBufferMainMenu.Count == retrieveBufferMainMenu.Capacity) {
                            retrieveBufferMainMenu.RemoveAt(0);
                        }
                        Tuple<Vector3, Quaternion, Vector3, float> oldOffset = new Tuple<Vector3, Quaternion, Vector3, float>(gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset, Quaternion.Euler(gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset), scaleOffset, oldOffsets.Item4);
                        retrieveBufferMainMenu.Add(oldOffset);
                    }
                    else {
                        List<Tuple<Vector3, Quaternion, Vector3, float>> retrieveBufferWithoutOrbital;
                        if (manipulationLogic == MenuManipulationLogic.OneToAll) {
                            retrieveBufferWithoutOrbital = placementService.GetRetrieveBufferOneToAllWithoutOrbital(menuID);
                        }
                        else {
                            if (!compact) {
                                retrieveBufferWithoutOrbital = placementService.GetRetrieveBufferOneToOneInBetween(TargetObject);
                            }
                            else {
                                retrieveBufferWithoutOrbital = placementService.GetRetrieveBufferOneToOneHandConstraint(TargetObject);
                            }
                        }
                        if (retrieveBufferWithoutOrbital.Count == retrieveBufferWithoutOrbital.Capacity) {
                            retrieveBufferWithoutOrbital.RemoveAt(0);
                        }
                        Tuple<Vector3, Quaternion, Vector3, float> oldOffset = new Tuple<Vector3, Quaternion, Vector3, float>(gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset, Quaternion.Euler(gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset), scaleOffset, oldOffsets.Item4);
                        retrieveBufferWithoutOrbital.Add(oldOffset);
                    }

                }                
            }
            else {
                Vector3 scaleOffset = new Vector3(oldOffsets.Item3.x / originalScale.x, oldOffsets.Item3.y / originalScale.y, oldOffsets.Item3.z / originalScale.z);
                if (retrieveBufferManualMode.Count == retrieveBufferManualMode.Capacity) {
                    retrieveBufferManualMode.RemoveAt(0);
                }
                Tuple<Vector3, Quaternion, Vector3> oldOffset = new Tuple<Vector3, Quaternion, Vector3>(oldOffsets.Item1, oldOffsets.Item2, scaleOffset);
                retrieveBufferManualMode.Add(oldOffset);
            }
        }

        /// <summary>
        /// Update the offsets for the menu object, used on app bar.
        /// </summary>  
        /// <param name="newOffsets"> offsets after manipulation</param>
        /// <param name="oldOffsets"> offsets before manipulation </param>
        public void UpdateOffset(Tuple<Vector3, Quaternion, Vector3, float> newOffsets, Tuple<Vector3, Quaternion, Vector3, float> oldOffsets) {
            if (placementService.PreviousMode == MenuPlacementService.MenuPlacementServiceMode.Automatic) {
                if (menuVariantType == MenuVariantType.ObjectMenu && gameObject.GetComponent<Orbital>() != null && gameObject.GetComponent<Orbital>().enabled) {
                    Vector3 offsetsCameraCoord = Camera.main.transform.InverseTransformVector(newOffsets.Item1) - Camera.main.transform.InverseTransformVector(oldOffsets.Item1);
                    Vector3 objectToMenu = Vector3.Normalize(gameObject.transform.position - TargetObject.transform.position);
                    //Is the menu on the right side of the object (in the camera/user space)?
                    bool rightSide = Vector3.Dot(objectToMenu, gameObject.transform.right) > 0 ? true : false;
                    if (rightSide) {
                        gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset += offsetsCameraCoord;
                    }
                    else {
                        //move away from the target object
                        if (offsetsCameraCoord.x < 0) {
                            gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = new Vector3(gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.x - offsetsCameraCoord.x, gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.y + offsetsCameraCoord.y, gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.z + offsetsCameraCoord.z);
                            if (gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.x > 0) {
                                gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = new Vector3(Math.Abs(gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.x), gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.y, gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.z);
                            }
                        }
                        //move closer to the target object
                        else {
                            gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = new Vector3(gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.x - offsetsCameraCoord.x, gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.y + offsetsCameraCoord.y, gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.z + offsetsCameraCoord.z);
                        }
                    }                    
                    gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset += newOffsets.Item2.eulerAngles - oldOffsets.Item2.eulerAngles;
                    gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset = new Vector3(newOffsets.Item3.x / originalScale.x, newOffsets.Item3.y / originalScale.y, newOffsets.Item3.z / originalScale.z);
                }
                else {
                    Vector3 offsetsCameraCoord = Camera.main.transform.InverseTransformVector(newOffsets.Item1) - Camera.main.transform.InverseTransformVector(oldOffsets.Item1);
                    gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset += offsetsCameraCoord;
                    gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset += newOffsets.Item2.eulerAngles - oldOffsets.Item2.eulerAngles;
                    gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset = new Vector3(newOffsets.Item3.x / originalScale.x, newOffsets.Item3.y / originalScale.y, newOffsets.Item3.z / originalScale.z);
                }
                //TargetViewPercentV of ConstantViewSize is already updated in AppBarControllerMPS, so we don't update it here.
                if(menuVariantType == MenuVariantType.ObjectMenu) {
                    Tuple<Vector3, Quaternion, Vector3, float> currentOffset = new Tuple<Vector3, Quaternion, Vector3, float>(gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset, Quaternion.Euler(gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset), gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset, gameObject.GetComponent<ConstantViewSize>().TargetViewPercentV);
                    if (manipulationLogic == MenuManipulationLogic.OneToAll) {
                        if (gameObject.GetComponent<Orbital>() != null && gameObject.GetComponent<Orbital>().enabled) {
                            placementService.SetCurrentOneToAllOffsetOrbital(menuID, currentOffset);
                        }
                        if(gameObject.GetComponent<InBetween>() != null && gameObject.GetComponent<InBetween>().enabled) {
                            placementService.SetCurrentOneToAllOffsetInBetween(menuID, currentOffset);
                        }
                        if(gameObject.GetComponent<HandConstraint>() != null && gameObject.GetComponent<HandConstraint>().enabled) {
                            placementService.SetCurrentOneToAllOffsetHandConstraint(menuID, currentOffset);
                        }
                    }
                    else {
                        if (!compact) {
                            if (gameObject.GetComponent<Orbital>() != null && gameObject.GetComponent<Orbital>().enabled) {
                                placementService.SetCurrentOneToOneOffsetOrbitalFloating(TargetObject, currentOffset);
                            }
                            if (gameObject.GetComponent<InBetween>() != null && gameObject.GetComponent<InBetween>().enabled) {
                                placementService.SetCurrentOneToOneOffsetInBetween(TargetObject, currentOffset);
                            }
                        }
                        else {
                            if (gameObject.GetComponent<Orbital>() != null && gameObject.GetComponent<Orbital>().enabled) {
                                placementService.SetCurrentOneToOneOffsetOrbitalCompact(TargetObject, currentOffset);
                            }
                            if (gameObject.GetComponent<HandConstraint>() != null && gameObject.GetComponent<HandConstraint>().enabled) {
                                placementService.SetCurrentOneToAllOffsetHandConstraint(menuID, currentOffset);
                            }
                        }
                    }
                }
            }
            else {
                manualModePositionOffset = newOffsets.Item1;
                manualModeRotationOffset = newOffsets.Item2.eulerAngles;
                manualModeScaleOffset = new Vector3(newOffsets.Item3.x / originalScale.x, newOffsets.Item3.y / originalScale.y, newOffsets.Item3.z / originalScale.z);
            }
        }

        /// <summary>
        /// Retrieve the last offsets of the menu object.
        /// </summary>
        public void Retrieve() {
            if(placementService.PreviousMode == MenuPlacementService.MenuPlacementServiceMode.Automatic){
                if (menuVariantType == MenuVariantType.ObjectMenu && gameObject.GetComponent<Orbital>() != null && gameObject.GetComponent<Orbital>().enabled) {
                    List<Tuple<Vector3, Quaternion, Vector3, float>> retrieveBufferOrbital;
                    if (manipulationLogic == MenuManipulationLogic.OneToAll) {
                        retrieveBufferOrbital = placementService.GetRetrieveBufferOneToAllOrbital(menuID);
                    }
                    else {
                        if (!compact) {
                            retrieveBufferOrbital = placementService.GetRetrieveBufferOneToOneOrbitalFloating(TargetObject);
                        }
                        else {
                            retrieveBufferOrbital = placementService.GetRetrieveBufferOneToOneOrbitalCompact(TargetObject);
                        }
                    }
                    if (retrieveBufferOrbital.Count > 0) {
                        Tuple<Vector3, Quaternion, Vector3, float> oldOffset = retrieveBufferOrbital.Last();
                        Vector3 headToObject = Vector3.Normalize(TargetObject.transform.position - head.transform.position);
                        //If we look at the object's LEFT side, than the angle between head.transform.right and headToObject is an acute angle, i.e. Vector3.Dot(headToObject, head.transform.right) > 0.
                        //If we look at the object's RIGHT side, than the angle between head.transform.right and headToObject is an obtuse angle, i.e. Vector3.Dot(headToObject, head.transform.right) < 0. 
                        bool rightSide = Vector3.Dot(headToObject, head.transform.right) < 0 ? true : false;
                        if (rightSide) {
                            gameObject.transform.localPosition = gameObject.transform.localPosition - (head.transform.right * (gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.x - oldOffset.Item1.x))
                                - head.transform.up * (gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.y - oldOffset.Item1.y) - head.transform.forward * (gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.z - oldOffset.Item1.z);
                        }
                        else {
                            gameObject.transform.localPosition = gameObject.transform.localPosition - (- head.transform.right * (gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.x - oldOffset.Item1.x))
                                - head.transform.up * (gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.y - oldOffset.Item1.y) - head.transform.forward * (gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.z - oldOffset.Item1.z);
                        }
                        gameObject.transform.localEulerAngles = gameObject.transform.localEulerAngles - gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset + oldOffset.Item2.eulerAngles;
                        gameObject.transform.localScale = new Vector3(oldOffset.Item3.x * originalScale.x, oldOffset.Item3.y * originalScale.y, oldOffset.Item3.z * originalScale.z);
                        gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = oldOffset.Item1;
                        gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset = oldOffset.Item2.eulerAngles;
                        gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset = oldOffset.Item3;
                        gameObject.GetComponent<ConstantViewSize>().TargetViewPercentV = oldOffset.Item4;
                        retrieveBufferOrbital.RemoveAt(retrieveBufferOrbital.Count - 1);
                    }
                }
                else {
                    if (menuVariantType == MenuVariantType.MainMenu) {
                        List<Tuple<Vector3, Quaternion, Vector3, float>> retrieveBufferMainMenu = placementService.GetRetrieveBufferMainMenu(menuID);
                        if (retrieveBufferMainMenu.Count > 0) {
                            Tuple<Vector3, Quaternion, Vector3, float> oldOffset = retrieveBufferMainMenu.Last();
                            gameObject.transform.localPosition = gameObject.transform.localPosition - head.transform.right * (gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.x - oldOffset.Item1.x)
                                    - head.transform.up * (gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.y - oldOffset.Item1.y) - head.transform.forward * (gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.z - oldOffset.Item1.z);
                            gameObject.transform.localEulerAngles = gameObject.transform.localEulerAngles - gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset + oldOffset.Item2.eulerAngles;
                            gameObject.transform.localScale = new Vector3(oldOffset.Item3.x * originalScale.x, oldOffset.Item3.y * originalScale.y, oldOffset.Item3.z * originalScale.z);
                            gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = oldOffset.Item1;
                            gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset = oldOffset.Item2.eulerAngles;
                            gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset = oldOffset.Item3;
                            gameObject.GetComponent<ConstantViewSize>().TargetViewPercentV = oldOffset.Item4;
                            retrieveBufferMainMenu.RemoveAt(retrieveBufferMainMenu.Count - 1);
                        }
                    }
                    else { 
                        List<Tuple<Vector3, Quaternion, Vector3, float>> retrieveBufferWithoutOrbital;
                        if (manipulationLogic == MenuManipulationLogic.OneToAll) {
                            retrieveBufferWithoutOrbital = placementService.GetRetrieveBufferOneToAllWithoutOrbital(menuID);
                        }
                        else {
                            if (!compact) {
                                retrieveBufferWithoutOrbital = placementService.GetRetrieveBufferOneToOneInBetween(TargetObject);
                            }
                            else {
                                retrieveBufferWithoutOrbital = placementService.GetRetrieveBufferOneToOneHandConstraint(TargetObject);
                            }
                        }
                        if (retrieveBufferWithoutOrbital.Count > 0) {
                            Tuple<Vector3, Quaternion, Vector3, float> oldOffset = retrieveBufferWithoutOrbital.Last();
                            gameObject.transform.localPosition = gameObject.transform.localPosition - head.transform.right * (gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.x - oldOffset.Item1.x)
                                    - head.transform.up * (gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.y - oldOffset.Item1.y) - head.transform.forward * (gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset.z - oldOffset.Item1.z);
                            gameObject.transform.localEulerAngles = gameObject.transform.localEulerAngles - gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset + oldOffset.Item2.eulerAngles;
                            gameObject.transform.localScale = new Vector3(oldOffset.Item3.x * originalScale.x, oldOffset.Item3.y * originalScale.y, oldOffset.Item3.z * originalScale.z);
                            gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = oldOffset.Item1;
                            gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset = oldOffset.Item2.eulerAngles;
                            gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset = oldOffset.Item3;
                            gameObject.GetComponent<ConstantViewSize>().TargetViewPercentV = oldOffset.Item4;
                            retrieveBufferWithoutOrbital.RemoveAt(retrieveBufferWithoutOrbital.Count - 1);
                        }
                    }
                }
                //Recompute the menu's orientation based on menuOrientationType for a better appearance.
                switch (menuOrientationType) {
                    case MenuOrientationType.CameraAligned:
                        gameObject.transform.forward = Vector3.Normalize(gameObject.transform.position - head.transform.position);
                        break;
                    case MenuOrientationType.Unmodified:
                        //Do nothing
                        break;
                }
                if (menuVariantType == MenuVariantType.ObjectMenu) {
                    Tuple<Vector3, Quaternion, Vector3, float> currentOffset = new Tuple<Vector3, Quaternion, Vector3, float>(gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset, Quaternion.Euler(gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset), gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset, gameObject.GetComponent<ConstantViewSize>().TargetViewPercentV);
                    if (manipulationLogic == MenuManipulationLogic.OneToAll) {
                        if (gameObject.GetComponent<Orbital>() != null && gameObject.GetComponent<Orbital>().enabled) {
                            placementService.SetCurrentOneToAllOffsetOrbital(menuID, currentOffset);
                        }
                        if (gameObject.GetComponent<InBetween>() != null && gameObject.GetComponent<InBetween>().enabled) {
                            placementService.SetCurrentOneToAllOffsetInBetween(menuID, currentOffset);
                        }
                        if (gameObject.GetComponent<HandConstraint>() != null && gameObject.GetComponent<HandConstraint>().enabled) {
                            placementService.SetCurrentOneToAllOffsetHandConstraint(menuID, currentOffset);
                        }
                    }
                    else {
                        if (!compact) {
                            if (gameObject.GetComponent<Orbital>() != null && gameObject.GetComponent<Orbital>().enabled) {
                                placementService.SetCurrentOneToOneOffsetOrbitalFloating(TargetObject, currentOffset);
                            }
                            if (gameObject.GetComponent<InBetween>() != null && gameObject.GetComponent<InBetween>().enabled) {
                                placementService.SetCurrentOneToOneOffsetInBetween(TargetObject, currentOffset);
                            }
                        }
                        else {
                            if (gameObject.GetComponent<Orbital>() != null && gameObject.GetComponent<Orbital>().enabled) {
                                placementService.SetCurrentOneToOneOffsetOrbitalCompact(TargetObject, currentOffset);
                            }
                            if (gameObject.GetComponent<HandConstraint>() != null && gameObject.GetComponent<HandConstraint>().enabled) {
                                placementService.SetCurrentOneToAllOffsetHandConstraint(menuID, currentOffset);
                            }
                        }
                    }
                }
            }
            else {
                if (retrieveBufferManualMode.Count > 0) {
                    Tuple<Vector3, Quaternion, Vector3> oldOffset = retrieveBufferManualMode.Last();
                    gameObject.transform.localPosition = oldOffset.Item1;
                    gameObject.transform.localRotation = oldOffset.Item2;
                    gameObject.transform.localScale = new Vector3(oldOffset.Item3.x * originalScale.x, oldOffset.Item3.y * originalScale.y, oldOffset.Item3.z * originalScale.z);
                    retrieveBufferManualMode.RemoveAt(retrieveBufferManualMode.Count - 1);
                }
            }

        }

        /// <summary>
        /// place the menu in front of the user, and lock it on the user (user-referenced)
        /// </summary>
        public void EnterManualMode() {
            gameObject.transform.parent = head.transform;
            gameObject.transform.localPosition = manualModePositionOffset;
            gameObject.transform.localEulerAngles = manualModeRotationOffset;
            gameObject.transform.localScale = new Vector3(originalScale.x * manualModeScaleOffset.x, originalScale.y * manualModeScaleOffset.y, originalScale.z * manualModeScaleOffset.z);
            gameObject.transform.parent = null;
            manualModeEntered = true;
        }

        public void ExitManualMode() {
            gameObject.transform.parent = null;
            gameObject.transform.localScale = originalScale;
            manualModeEntered = false;
        }

        #endregion Public Methods

        #region Private Methods

        //This function should be called just once for one main menu in Start()
        private void InitializeMainMenu() {
            gameObject.GetComponent<MenuHandler>().updateTime = 0;
            if (!compact) {
                gameObject.AddComponent<SolverHandler>().AdditionalOffset = followOffset;
                gameObject.AddComponent<SurfaceMagnetism>().SurfaceNormalOffset = surfaceMagnetismSafetyOffset;
                gameObject.GetComponent<SurfaceMagnetism>().UpdateLinkedTransform = true;
                gameObject.GetComponent<SurfaceMagnetism>().CurrentOrientationMode = SurfaceMagnetism.OrientationMode.None;
                gameObject.GetComponent<SurfaceMagnetism>().MagneticSurfaces[0] = placementService.SpatialAwarenessLayer;
                gameObject.GetComponent<SurfaceMagnetism>().MaxRaycastDistance = maxFloatingDistance;
                gameObject.GetComponent<SurfaceMagnetism>().ClosestDistance = minFloatingDistance;
                gameObject.AddComponent<Follow>().MinDistance = minFloatingDistance;
                gameObject.GetComponent<Follow>().MaxDistance = maxFloatingDistance;
                gameObject.GetComponent<Follow>().DefaultDistance = defaultFloatingDistance;
                gameObject.GetComponent<Follow>().MaxViewHorizontalDegrees = followMaxViewHorizontalDegrees;
                gameObject.GetComponent<Follow>().MaxViewVerticalDegrees = followMaxViewVerticalDegrees;
                gameObject.GetComponent<Follow>().OrientToControllerDeadzoneDegrees = 20f;
                gameObject.GetComponent<Follow>().OrientationType = SolverOrientationType.Unmodified;
                gameObject.GetComponent<Follow>().UpdateLinkedTransform = true;
                gameObject.AddComponent<FinalPlacementOptimizer>().OriginalScale = gameObject.transform.localScale;
                gameObject.GetComponent<FinalPlacementOptimizer>().OrientationType = menuOrientationType;
                gameObject.GetComponent<FinalPlacementOptimizer>().enabled = true;               
            }
            else {
                if (placementService.HandTrackingEnabled) {
                    if (placementService.ArticulatedHandSupported) {
                        gameObject.AddComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.HandJoint;
                        if (dominantHand == Handedness.Left) {
                            gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
                        }
                        else {
                            gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;
                        }
                    }
                    else{
                        gameObject.AddComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.ControllerRay;
                        if (dominantHand == Handedness.Left) {
                            gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
                        }
                        else {
                            gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;
                        }
                    }

                    gameObject.AddComponent<HandConstraint>().SafeZone = HandConstraint.SolverSafeZone.UlnarSide;
                    gameObject.GetComponent<HandConstraint>().SafeZoneBuffer = 0.05f;
                    gameObject.GetComponent<HandConstraint>().UpdateLinkedTransform = true;
                    gameObject.GetComponent<HandConstraint>().UpdateWhenOppositeHandNear = true;
                    gameObject.AddComponent<FinalPlacementOptimizer>().OriginalScale = gameObject.transform.localScale;
                    gameObject.GetComponent<FinalPlacementOptimizer>().OrientationType = menuOrientationType;
                    gameObject.GetComponent<FinalPlacementOptimizer>().enabled = true;
                }
                else {
                    gameObject.AddComponent<SolverHandler>();
                    gameObject.AddComponent<Follow>().MinDistance = 0.3f;
                    gameObject.GetComponent<Follow>().MaxDistance = 0.3f;
                    gameObject.GetComponent<Follow>().DefaultDistance = 0.3f;
                    gameObject.GetComponent<Follow>().MaxViewHorizontalDegrees = 40f;
                    gameObject.GetComponent<Follow>().MaxViewVerticalDegrees = 40f;
                    gameObject.GetComponent<Follow>().OrientToControllerDeadzoneDegrees = 20f;
                    gameObject.GetComponent<Follow>().OrientationType = SolverOrientationType.Unmodified;
                    gameObject.GetComponent<Follow>().UpdateLinkedTransform = true;
                    gameObject.AddComponent<FinalPlacementOptimizer>().OriginalScale = gameObject.transform.localScale;
                    gameObject.GetComponent<FinalPlacementOptimizer>().OrientationType = menuOrientationType;
                    gameObject.GetComponent<FinalPlacementOptimizer>().enabled = true;
                }
            }
            (gameObject.GetComponent<ConstantViewSize>() ?? gameObject.AddComponent<ConstantViewSize>()).TargetViewPercentV = defaultTargetViewPercentV;
            gameObject.GetComponent<ConstantViewSize>().MinDistance = minFloatingDistance;
            gameObject.GetComponent<ConstantViewSize>().MaxDistance = maxFloatingDistance;
            gameObject.GetComponent<ConstantViewSize>().enabled = constantViewSizeEnabled;
            if (manipulationEnabled) {
                InitializeAppBar();
            }
            gameObject.GetComponent<MenuBase>().Initialize();
        }

        private void InitializeAppBar() {
            (gameObject.GetComponent<BoundsControl>() ?? gameObject.AddComponent<BoundsControl>()).BoundsControlActivation = BoundsControlActivationType.ActivateManually;
            gameObject.GetComponent<BoundsControl>().Target = gameObject;
            if (gameObject.GetComponent<BoxCollider>()) {
                gameObject.GetComponent<BoundsControl>().BoundsOverride = gameObject.GetComponent<BoxCollider>();
            }
            if (constantViewSizeEnabled) {
                gameObject.GetComponent<BoundsControl>().ScaleHandlesConfig.ShowScaleHandles = false;
            }
            if (!appBar) {
                appBar = Instantiate(ServiceManager.GetService<MenuPlacementService>().AppBar);
                appBar.GetComponent<AppBarControllerMPS>().TargetMenuObject = gameObject;
            } 
            else {
                appBar.SetActive(true);
            }
        }

        //The core method of the menu placement system.
        //It checks the spatial mapping in front of the user and around the menu object to determine the proper placement strategy (solver).
        private void CheckSpatialMapping() {
            float distanceToMenu = Vector3.Dot(gameObject.transform.position - head.transform.position, head.transform.forward);
            //Use to check main menu, 2 * surfaceMagnetismSafetyOffset for better accuracy
            bool closeToSpatialMapping = Physics.Raycast(head.transform.position, head.transform.forward, minFloatingDistance + 2 * surfaceMagnetismSafetyOffset, placementService.SpatialAwarenessLayer);
            if (menuVariantType == MenuVariantType.MainMenu && !compact) {
                if(Physics.Raycast(head.transform.position, head.transform.forward, maxFloatingDistance, placementService.SpatialAwarenessLayer)) { 
                    gameObject.GetComponent<SurfaceMagnetism>().enabled = false;
                }
                else {
                    gameObject.GetComponent<SurfaceMagnetism>().enabled = true;
                }
            }
            if (CollideWithSpatialMapping() || (distanceToMenu < minFloatingDistance + surfaceMagnetismSafetyOffset && closeToSpatialMapping)) {
                if (menuVariantType == MenuVariantType.MainMenu) {
                    if (!compact) {                       
                        // Cast a ray towards the position of the menu from the camera, if the menu is closer than the minFloatingDistance, switch to the compact version.
                        Ray ray = new Ray(head.transform.position, GetBoundingBox().center - head.transform.position);
                        if (Physics.Raycast(ray, minFloatingDistance, placementService.MenuLayer)) {
                            message.switchType = PlacementMessage.SwitchType.FloatingToCompact;
                            switchTo = message.switchType;
                        }
                    }
                    //Compact menus don't need to be handled here.
                }
                else {
                    float targetDistance = (head.transform.position - TargetObject.transform.position).magnitude;
                    //If the target object is far away, namely the InBetween solver is activated.
                    if (targetDistance > maxFloatingDistance) {
                        if (!compact) {
                            message.switchType = PlacementMessage.SwitchType.FloatingToCompact;
                            switchTo = message.switchType;
                            //Because the object is far, the HandConstraint solver should be activated on next call.
                        }
                        //Activate the HandConstraint solver (or Follow solver)
                        else {
                            if (placementService.HandTrackingEnabled) {
                                if (placementService.ArticulatedHandSupported) {
                                    gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.HandJoint;
                                    if (dominantHand == Handedness.Left) {
                                        gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
                                    }
                                    else {
                                        gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;
                                    }
                                }
                                else {
                                    gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.ControllerRay;
                                    if (dominantHand == Handedness.Left) {
                                        gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
                                    }
                                    else {
                                        gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;
                                    }
                                }
                                gameObject.GetComponent<HandConstraint>().enabled = true;
                                gameObject.GetComponent<Orbital>().enabled = false;
                            }
                            else {
                                gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.Head;
                                gameObject.transform.position = head.transform.position + head.transform.forward * 0.3f;
                                gameObject.GetComponent<Follow>().enabled = true;
                                gameObject.GetComponent<Orbital>().enabled = false;
                            }
                            Tuple<Vector3, Quaternion, Vector3, float> currentOffsetHandConstaint;
                            if(manipulationLogic == MenuManipulationLogic.OneToAll) {
                                currentOffsetHandConstaint = placementService.GetCurrentOneToAllOffsetHandConstraint(menuID);
                            }
                            else {
                                currentOffsetHandConstaint = placementService.GetCurrentOneToOneOffsetHandConstraint(TargetObject);
                            }
                            gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = currentOffsetHandConstaint.Item1;
                            gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset = currentOffsetHandConstaint.Item2.eulerAngles;
                            gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset = currentOffsetHandConstaint.Item3;
                            gameObject.GetComponent<ConstantViewSize>().TargetViewPercentV = currentOffsetHandConstaint.Item4;
                        }
                    }
                    else {
                        //The object is close, namely the Beside and Orbital solvers are activated.
                        //Begin the collision handling process:
                        //First step: Try to switch to compact version if it is a floating menu
                        if (!compact) {
                            message.switchType = PlacementMessage.SwitchType.FloatingToCompact;
                            switchTo = message.switchType;
                        }
                        //Second step: If it is already compact and there is still a collision, then activate the HandConstraint solver or Follow solver.
                        else {
                            if (placementService.HandTrackingEnabled) {
                                if (placementService.ArticulatedHandSupported) {
                                    gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.HandJoint;
                                    if (dominantHand == Handedness.Left) {
                                        gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
                                    }
                                    else {
                                        gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;
                                    }
                                }
                                else {
                                    gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.ControllerRay;
                                    if (dominantHand == Handedness.Left) {
                                        gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
                                    }
                                    else {
                                        gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;
                                    }
                                }
                                gameObject.GetComponent<HandConstraint>().enabled = true;
                                gameObject.GetComponent<Orbital>().enabled = false;
                            }
                            else {
                                gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.Head;
                                gameObject.transform.position = head.transform.position + head.transform.forward * 0.3f;
                                gameObject.GetComponent<Follow>().enabled = true;
                                gameObject.GetComponent<Orbital>().enabled = false;
                            }
                            Tuple<Vector3, Quaternion, Vector3, float> currentOffsetHandConstaint;
                            if (manipulationLogic == MenuManipulationLogic.OneToAll) {
                                currentOffsetHandConstaint = placementService.GetCurrentOneToAllOffsetHandConstraint(menuID);
                            }
                            else {
                                currentOffsetHandConstaint = placementService.GetCurrentOneToOneOffsetHandConstraint(TargetObject);
                            }
                            gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = currentOffsetHandConstaint.Item1;
                            gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset = currentOffsetHandConstaint.Item2.eulerAngles;
                            gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset = currentOffsetHandConstaint.Item3;
                            gameObject.GetComponent<ConstantViewSize>().TargetViewPercentV = currentOffsetHandConstaint.Item4;
                        }
                    }                    
                }
            }
            else {
                if (menuVariantType == MenuVariantType.MainMenu) {
                    if (compact) {
                        //check the bounding box of the floating menu from GetBoundingBox() in front of the user's head.
                        Vector3 center = head.transform.position + head.transform.forward * minFloatingDistance;
                        if (!Physics.CheckBox(center, placementService.GetStoredBoundingBoxOppositeVariant(gameObject).extents, Quaternion.identity, placementService.SpatialAwarenessLayer)) {
                            //Debug.Log("Front Free for Floating main menu");
                            message.switchType = PlacementMessage.SwitchType.CompactToFloating;
                            switchTo = message.switchType;
                        }
                    }
                    //Floating menus don't need to be handled here.
                }
                else {
                    float targetDistance = (head.transform.position - TargetObject.transform.position).magnitude;
                    if (!compact) {
                        //The object is between maxFloatingDistance and minFloatingDistance, use the Orbital solver.
                        if (targetDistance <= maxFloatingDistance && targetDistance >= minFloatingDistance) {
                            gameObject.GetComponent<SolverHandler>().TransformOverride = TargetObject.transform;
                            gameObject.GetComponent<Orbital>().enabled = true;
                            gameObject.GetComponent<InBetween>().enabled = false;                            
                            gameObject.GetComponent<FinalPlacementOptimizer>().enabled = true;
                            Tuple<Vector3, Quaternion, Vector3, float> currentOffsetOrbital;
                            if (manipulationLogic == MenuManipulationLogic.OneToAll) {
                                currentOffsetOrbital = placementService.GetCurrentOneToAllOffsetOrbital(menuID);
                            }
                            else {
                                currentOffsetOrbital = placementService.GetCurrentOneToOneOffsetOrbitalFloating(TargetObject);
                            }
                            gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = currentOffsetOrbital.Item1;
                            gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset = currentOffsetOrbital.Item2.eulerAngles;
                            gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset = currentOffsetOrbital.Item3;
                            gameObject.GetComponent<ConstantViewSize>().TargetViewPercentV = currentOffsetOrbital.Item4;
                        }
                        else if (targetDistance < minFloatingDistance) {
                            message.switchType = PlacementMessage.SwitchType.FloatingToCompact;
                            switchTo = message.switchType;
                        }
                        else if (targetDistance > maxFloatingDistance) {
                            //We should enable InBetween solver;
                            gameObject.GetComponent<SolverHandler>().TransformOverride = placementService.GetInBetweenTarget().transform;
                            gameObject.GetComponent<Orbital>().enabled = false;
                            gameObject.GetComponent<InBetween>().enabled = true;
                            gameObject.GetComponent<InBetween>().PartwayOffset = 1 - (defaultFloatingDistance / targetDistance);
                            gameObject.GetComponent<FinalPlacementOptimizer>().enabled = true;
                            Tuple<Vector3, Quaternion, Vector3, float> currentOffsetInBetween;
                            if (manipulationLogic == MenuManipulationLogic.OneToAll) {
                                currentOffsetInBetween = placementService.GetCurrentOneToAllOffsetInBetween(menuID);
                            }
                            else {
                                currentOffsetInBetween = placementService.GetCurrentOneToOneOffsetInBetween(TargetObject);
                            }
                            gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = currentOffsetInBetween.Item1;
                            gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset = currentOffsetInBetween.Item2.eulerAngles;
                            gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset = currentOffsetInBetween.Item3;
                            gameObject.GetComponent<ConstantViewSize>().TargetViewPercentV = currentOffsetInBetween.Item4;
                        }
                    }
                    //Compact 
                    else {
                        //Check space around and try to switch to floating version.
                        if (targetDistance > maxFloatingDistance) {
                            Vector3 inBetweenPositionOffsetFloatingVariant = placementService.GetInBetweenPositionOffsetOppositeVariant(gameObject);
                            //Check the space in front of user because InBetween solver should be activated
                            Vector3 center = placementService.GetInBetweenTarget().transform.position + Vector3.Normalize(TargetObject.transform.position - placementService.GetInBetweenTarget().transform.position) * defaultFloatingDistance 
                                + head.transform.right * inBetweenPositionOffsetFloatingVariant.x + head.transform.up * inBetweenPositionOffsetFloatingVariant.y + head.transform.forward * inBetweenPositionOffsetFloatingVariant.z;
                            if (!Physics.CheckBox(center, placementService.GetStoredBoundingBoxOppositeVariant(gameObject).extents, Quaternion.identity, placementService.SpatialAwarenessLayer)) {
                                //Debug.Log("Front Free for InBetween");
                                message.switchType = PlacementMessage.SwitchType.CompactToFloating;
                                switchTo = message.switchType;
                            }
                            else {
                                if (placementService.HandTrackingEnabled) {
                                    if (placementService.ArticulatedHandSupported) {
                                        gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.HandJoint;
                                        if (dominantHand == Handedness.Left) {
                                            gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
                                        }
                                        else {
                                            gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;
                                        }
                                    }
                                    else {
                                        gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.ControllerRay;
                                        if (dominantHand == Handedness.Left) {
                                            gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
                                        }
                                        else {
                                            gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;
                                        }
                                    }
                                    gameObject.GetComponent<HandConstraint>().enabled = true;
                                    gameObject.GetComponent<Orbital>().enabled = false;
                                }
                                else {
                                    gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.Head;
                                    gameObject.transform.position = head.transform.position + head.transform.forward * 0.3f;
                                    gameObject.GetComponent<Follow>().enabled = true;
                                    gameObject.GetComponent<Orbital>().enabled = false;
                                }
                                Tuple<Vector3, Quaternion, Vector3, float> currentOffsetHandConstaint;
                                if (manipulationLogic == MenuManipulationLogic.OneToAll) {
                                    currentOffsetHandConstaint = placementService.GetCurrentOneToAllOffsetHandConstraint(menuID);
                                }
                                else {
                                    currentOffsetHandConstaint = placementService.GetCurrentOneToOneOffsetHandConstraint(TargetObject);
                                }
                                gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = currentOffsetHandConstaint.Item1;
                                gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset = currentOffsetHandConstaint.Item2.eulerAngles;
                                gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset = currentOffsetHandConstaint.Item3;
                                gameObject.GetComponent<ConstantViewSize>().TargetViewPercentV = currentOffsetHandConstaint.Item4;
                            }
                        }
                        //Try to enable floating vairant with the Orbital solver
                        else {
                            Vector3 orbitalPositionOffsetFloatingVariant = placementService.GetOrbitalPositionOffsetOppositeVariant(gameObject);
                            Vector3 headToObject = Vector3.Normalize(TargetObject.transform.position - head.transform.position);
                            Vector3 forward = Vector3.Normalize(new Vector3(headToObject.x, 0, headToObject.y));
                            Vector3 right = Vector3.Normalize(Vector3.ProjectOnPlane(new Vector3(head.transform.right.x, 0, head.transform.right.z), headToObject));
                            Vector3 up = Vector3.up;
                            //If we look at the object's LEFT side, than the angle between head.transform.right and headToObject is an acute angle, i.e. Vector3.Dot(headToObject, head.transform.right) > 0.
                            //If we look at the object's RIGHT side, than the angle between head.transform.right and headToObject is an obtuse angle, i.e. Vector3.Dot(headToObject, head.transform.right) < 0. 
                            bool rightSide = Vector3.Dot(headToObject, head.transform.right) < 0 ? true : false;
                            Vector3 centerFloating = Vector3.zero;
                            Vector3 centerCompact = Vector3.zero;
                            Tuple<Vector3, Quaternion, Vector3, float> currentOffsetOrbitalCompact;
                            if (manipulationLogic == MenuManipulationLogic.OneToAll) {
                                currentOffsetOrbitalCompact = placementService.GetCurrentOneToAllOffsetOrbital(menuID);
                            }
                            else {
                                currentOffsetOrbitalCompact = placementService.GetCurrentOneToOneOffsetOrbitalCompact(TargetObject);
                            }
                            if (rightSide) {
                                centerFloating = TargetObject.transform.position + right * orbitalOffsetOppositeType.x + head.transform.right * orbitalPositionOffsetFloatingVariant.x + up * orbitalOffsetOppositeType.y + head.transform.up * orbitalPositionOffsetFloatingVariant.y + forward * orbitalOffsetOppositeType.z + head.transform.forward * orbitalPositionOffsetFloatingVariant.z;
                                centerCompact = TargetObject.transform.position + right * orbitalOffset.x + head.transform.right * currentOffsetOrbitalCompact.Item1.x + up * orbitalOffset.y + head.transform.up * currentOffsetOrbitalCompact.Item1.y + forward * orbitalOffset.z + head.transform.forward * currentOffsetOrbitalCompact.Item1.z;
                            }
                            else {
                                centerFloating = TargetObject.transform.position - right * orbitalOffsetOppositeType.x - head.transform.right * orbitalPositionOffsetFloatingVariant.x + up * orbitalOffsetOppositeType.y + head.transform.up * orbitalPositionOffsetFloatingVariant.y + forward * orbitalOffsetOppositeType.z + head.transform.forward * orbitalPositionOffsetFloatingVariant.z;
                                centerCompact = TargetObject.transform.position - right * orbitalOffset.x - head.transform.right * currentOffsetOrbitalCompact.Item1.x + up * orbitalOffset.y + head.transform.up * currentOffsetOrbitalCompact.Item1.y + forward * orbitalOffset.z + head.transform.forward * currentOffsetOrbitalCompact.Item1.z;
                            }                            
                            if (targetDistance >= minFloatingDistance && targetDistance <= maxFloatingDistance) {
                                //Orbital solver should be activated on the floating menu.
                                //Check the space "besides" the targetObject on the left or right side according to user's position, similar to FinalPlacementOptimizer solver.
                                if (!Physics.CheckBox(centerFloating, placementService.GetStoredBoundingBoxOppositeVariant(gameObject).extents, Quaternion.identity, placementService.SpatialAwarenessLayer)
                                    && !Physics.Raycast(TargetObject.transform.position, centerFloating - TargetObject.transform.position, (centerFloating - TargetObject.transform.position).magnitude, placementService.SpatialAwarenessLayer)) {
                                    //Debug.Log("Besides Free for Floating Orbital");
                                    message.switchType = PlacementMessage.SwitchType.CompactToFloating;
                                    switchTo = message.switchType;
                                }
                                //The space is not enough for a floating menu with the Orbital solver, so we then check if it is enough for a compact menu with the Orbital solver.
                                else if (!Physics.CheckBox(centerCompact, placementService.GetStoredBoundingBoxOppositeVariant(gameObject).extents, Quaternion.identity, placementService.SpatialAwarenessLayer)
                                   && !Physics.Raycast(TargetObject.transform.position, centerCompact - TargetObject.transform.position, (centerCompact - TargetObject.transform.position).magnitude, placementService.SpatialAwarenessLayer)) {
                                    //Debug.Log("Beside Free for Compact Orbital");
                                    gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.CustomOverride;
                                    gameObject.GetComponent<SolverHandler>().TransformOverride = TargetObject.transform;
                                    gameObject.GetComponent<Orbital>().enabled = true;
                                    gameObject.GetComponent<FinalPlacementOptimizer>().OrbitalOffset = orbitalOffset;
                                    if (placementService.HandTrackingEnabled) {
                                        gameObject.GetComponent<HandConstraint>().enabled = false;
                                    }
                                    else {
                                        gameObject.GetComponent<Follow>().enabled = false;
                                    }
                                    gameObject.GetComponent<FinalPlacementOptimizer>().enabled = true;
                                    Tuple<Vector3, Quaternion, Vector3, float> currentOffsetOrbital;
                                    if(manipulationLogic == MenuManipulationLogic.OneToAll) {
                                        currentOffsetOrbital = placementService.GetCurrentOneToAllOffsetOrbital(menuID);
                                    }
                                    else {
                                        currentOffsetOrbital = placementService.GetCurrentOneToOneOffsetOrbitalCompact(TargetObject);
                                    }
                                    gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = currentOffsetOrbital.Item1;
                                    gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset = currentOffsetOrbital.Item2.eulerAngles;
                                    gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset = currentOffsetOrbital.Item3;
                                    gameObject.GetComponent<ConstantViewSize>().TargetViewPercentV = currentOffsetOrbital.Item4;
                                }
                            }
                            //If targetDistance < minFloatingDistance, the menu should remain compact.
                            if (targetDistance < minFloatingDistance) {
                                //Try to enable the Orbital solver.
                                if (!Physics.CheckBox(centerCompact, placementService.GetStoredBoundingBoxOppositeVariant(gameObject).extents, Quaternion.identity, placementService.SpatialAwarenessLayer)
                                    && !Physics.Raycast(TargetObject.transform.position, centerCompact - TargetObject.transform.position, (centerCompact - TargetObject.transform.position).magnitude, placementService.SpatialAwarenessLayer)) {
                                    //Debug.Log("Beside Free for Compact Orbital");
                                    gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.CustomOverride;
                                    gameObject.GetComponent<SolverHandler>().TransformOverride = TargetObject.transform;
                                    gameObject.GetComponent<Orbital>().enabled = true;
                                    gameObject.GetComponent<FinalPlacementOptimizer>().OrbitalOffset = orbitalOffset;
                                    if (placementService.HandTrackingEnabled) {
                                        gameObject.GetComponent<HandConstraint>().enabled = false;
                                    }
                                    else {
                                        gameObject.GetComponent<Follow>().enabled = false;
                                    }
                                    gameObject.GetComponent<FinalPlacementOptimizer>().enabled = true;
                                    Tuple<Vector3, Quaternion, Vector3, float> currentOffsetOrbital;
                                    if(manipulationLogic == MenuManipulationLogic.OneToAll) {
                                        currentOffsetOrbital = placementService.GetCurrentOneToAllOffsetOrbital(menuID);
                                    }
                                    else {
                                        currentOffsetOrbital = placementService.GetCurrentOneToOneOffsetOrbitalCompact(TargetObject);
                                    }
                                    gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = currentOffsetOrbital.Item1;
                                    gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset = currentOffsetOrbital.Item2.eulerAngles;
                                    gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset = currentOffsetOrbital.Item3;
                                    gameObject.GetComponent<ConstantViewSize>().TargetViewPercentV = currentOffsetOrbital.Item4;
                                }
                            }
                        }
                    }
                }
            }
        }

        // Check Occlusion using raycast from the head to the center of the menu.
        // We don't need to check occlusion for main menus because of the SurfaceMagnetism solver and all menus with HandConstraint solver (or Follow on HoloLens 1) activated.
        private void CheckOcclusion() {
            if (menuVariantType == MenuVariantType.ObjectMenu) {
                float targetDistance = (head.transform.position - TargetObject.transform.position).magnitude;
                Ray ray = new Ray(head.transform.position, gameObject.transform.position - head.transform.position);
                float headMenuDistance = (gameObject.transform.position - head.transform.position).magnitude;
                if (!compact) {      
                    //InBetween or Orbital activated
                    if(Physics.Raycast(ray, headMenuDistance, placementService.SpatialAwarenessLayer)){
                        message.switchType = PlacementMessage.SwitchType.FloatingToCompact;
                        switchTo = message.switchType;
                        Debug.Log("Occlusion Detected for Floating menu");
                    }
                }
                else {
                    if(Physics.Raycast(ray, headMenuDistance, placementService.SpatialAwarenessLayer)) {
                        Debug.Log("Occlusion Detected for Compact Menu");
                        message.switchType = PlacementMessage.SwitchType.NoSwitch;
                        switchTo = message.switchType;
                        //If orbital activated，turn on the HandConstraint or Follow solver, else we do not need to do anything (remain in HandConstraint).
                        if (placementService.HandTrackingEnabled) {
                            if (!gameObject.GetComponent<HandConstraint>().enabled) {
                                if (placementService.ArticulatedHandSupported) {
                                    gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.HandJoint;
                                    if (dominantHand == Handedness.Left) {
                                        gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
                                    }
                                    else {
                                        gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;
                                    }
                                }
                                else {
                                    gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.ControllerRay;
                                    if (dominantHand == Handedness.Left) {
                                        gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right;
                                    }
                                    else {
                                        gameObject.GetComponent<SolverHandler>().TrackedHandness = Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left;
                                    }
                                }
                                gameObject.GetComponent<HandConstraint>().enabled = true;
                                gameObject.GetComponent<Orbital>().enabled = false;
                            }
                        }
                        else {
                            gameObject.GetComponent<SolverHandler>().TrackedTargetType = TrackedObjectType.Head;
                            gameObject.transform.position = head.transform.position + head.transform.forward * 0.3f;
                            gameObject.GetComponent<Follow>().enabled = true;
                            gameObject.GetComponent<Orbital>().enabled = false;
                        }
                        Tuple<Vector3, Quaternion, Vector3, float> currentOffsetHandConstaint;
                        if (manipulationLogic == MenuManipulationLogic.OneToAll) {
                            currentOffsetHandConstaint = placementService.GetCurrentOneToAllOffsetHandConstraint(menuID);
                        }
                        else {
                            currentOffsetHandConstaint = placementService.GetCurrentOneToOneOffsetHandConstraint(TargetObject);
                        }
                        gameObject.GetComponent<FinalPlacementOptimizer>().PositionOffset = currentOffsetHandConstaint.Item1;
                        gameObject.GetComponent<FinalPlacementOptimizer>().RotationOffset = currentOffsetHandConstaint.Item2.eulerAngles;
                        gameObject.GetComponent<FinalPlacementOptimizer>().ScaleOffset = currentOffsetHandConstaint.Item3;
                        gameObject.GetComponent<ConstantViewSize>().TargetViewPercentV = currentOffsetHandConstaint.Item4;
                    }
                    //Check if there is no (potential) occlusion anymore
                    if (targetDistance > maxFloatingDistance) {
                        //For InBetween
                        Vector3 inBetweenPositionOffsetFloatingVariant = placementService.GetInBetweenPositionOffsetOppositeVariant(gameObject);
                        //Check the space in front of user because InBetween solver should be activated
                        Vector3 inBetweenPosition = placementService.GetInBetweenTarget().transform.position + Vector3.Normalize(TargetObject.transform.position - placementService.GetInBetweenTarget().transform.position) * defaultFloatingDistance
                            + head.transform.right * inBetweenPositionOffsetFloatingVariant.x + head.transform.up * inBetweenPositionOffsetFloatingVariant.y + head.transform.forward * inBetweenPositionOffsetFloatingVariant.z;
                        if (Physics.Raycast(head.transform.position, inBetweenPosition - head.transform.position, (inBetweenPosition - head.transform.position).magnitude, placementService.SpatialAwarenessLayer)) {
                            Debug.Log("Occlusion detected if switch to floating menu InBetween");
                            message.switchType = PlacementMessage.SwitchType.NoSwitch;
                            switchTo = message.switchType;
                        }
                    }
                    if(targetDistance <= maxFloatingDistance && targetDistance >= minFloatingDistance) {
                        //For Orbital
                        Vector3 orbitalPositionOffsetFloatingVariant = placementService.GetOrbitalPositionOffsetOppositeVariant(gameObject);
                        Vector3 headToObject = Vector3.Normalize(TargetObject.transform.position - head.transform.position);
                        Vector3 forward = Vector3.Normalize(new Vector3(headToObject.x, 0, headToObject.y));
                        Vector3 right = Vector3.Normalize(Vector3.ProjectOnPlane(new Vector3(head.transform.right.x, 0, head.transform.right.z), headToObject));
                        Vector3 up = Vector3.up;
                        //If we look at the object's LEFT side, than the angle between head.transform.right and headToObject is an acute angle, i.e. Vector3.Dot(headToObject, head.transform.right) > 0.
                        //If we look at the object's RIGHT side, than the angle between head.transform.right and headToObject is an obtuse angle, i.e. Vector3.Dot(headToObject, head.transform.right) < 0. 
                        bool rightSide = Vector3.Dot(headToObject, head.transform.right) < 0 ? true : false;
                        Vector3 centerFloating = Vector3.zero;
                        if (rightSide) {
                            centerFloating = TargetObject.transform.position + right * orbitalOffsetOppositeType.x + head.transform.right * orbitalPositionOffsetFloatingVariant.x + up * orbitalOffsetOppositeType.y + head.transform.up * orbitalPositionOffsetFloatingVariant.y + forward * orbitalOffsetOppositeType.z + head.transform.forward * orbitalPositionOffsetFloatingVariant.z;
                        }
                        else {
                            centerFloating = TargetObject.transform.position - right * orbitalOffsetOppositeType.x - head.transform.right * orbitalPositionOffsetFloatingVariant.x + up * orbitalOffsetOppositeType.y + head.transform.up * orbitalPositionOffsetFloatingVariant.y + forward * orbitalOffsetOppositeType.z + head.transform.forward * orbitalPositionOffsetFloatingVariant.z;
                        }
                        if (Physics.Raycast(head.transform.position, centerFloating - head.transform.position, (centerFloating - head.transform.position).magnitude, placementService.SpatialAwarenessLayer)) {
                            Debug.Log("Occlusion detected if switch to floating menu Orbital");
                            message.switchType = PlacementMessage.SwitchType.NoSwitch;
                            switchTo = message.switchType;
                        }                      
                    }
                }                
            }
        }

        // Provide suggestions to the user in manual mode by showing a dialog panel.
        private void ShowSuggestion() {
            //Check collsions
            if (CollideWithSpatialMapping()) {
                accumulatedTimeForSuggestion += Time.deltaTime;
                if (suggestionTime > suggestionTimeInterval && accumulatedTimeForSuggestion > accumulatedTimeForSuggestionThreshold) {
                    placementService.EnterAdjustmentMode();
                    Component suggestionPanel = menuVariantType == MenuVariantType.MainMenu? 
                                Dialog.Open(placementService.SuggestionPanel, DialogButtonType.Close | DialogButtonType.Accept, "Menu Needs Adjustment",
                                "Collision Detected for the main menu! You might need to move to another location, move it closer or switch it to compact version if possible. You can click 'Accept' to switch to automatic mode.", true)
                                :
                                Dialog.Open(placementService.SuggestionPanel, DialogButtonType.Close | DialogButtonType.Accept, "Menu Needs Adjustment",
                                "Collision Detected for an object menu! You might need to move to another location, move it closer or switch it to compact version if possible. You can click 'Accept' to switch to automatic mode.", true);                        
                    suggestionPanel.gameObject.GetComponent<Follow>().MinDistance = 0.4f;
                    suggestionPanel.gameObject.GetComponent<Follow>().MaxDistance = 0.4f;
                    suggestionPanel.gameObject.GetComponent<Follow>().DefaultDistance = 0.4f;
                    suggestionPanel.gameObject.transform.forward = head.transform.forward;
                    suggestionTime = 0;
                    accumulatedTimeForSuggestion = 0;
                }
            }
            //Check occlusions
            else {
                Ray ray = new Ray(head.transform.position, gameObject.transform.position - head.transform.position);
                float headMenuDistance = (gameObject.transform.position - head.transform.position).magnitude;
                if(Physics.Raycast(ray, headMenuDistance, placementService.SpatialAwarenessLayer)) {
                    accumulatedTimeForSuggestion += Time.deltaTime;
                    if (suggestionTime > suggestionTimeInterval && accumulatedTimeForSuggestion > accumulatedTimeForSuggestionThreshold) {
                        placementService.EnterAdjustmentMode();
                        Component suggestionPanel = menuVariantType == MenuVariantType.MainMenu?
                                Dialog.Open(placementService.SuggestionPanel, DialogButtonType.Close | DialogButtonType.Accept, "Menu Needs Adjustment",
                                    "Occlusion Detected for the main menu! You might need to move to another location, move it closer or switch it to compact version if possible. You can click 'Accept' to switch to automatic mode.", true)
                                :
                                Dialog.Open(placementService.SuggestionPanel, DialogButtonType.Close | DialogButtonType.Accept, "Menu Needs Adjustment",
                                    "Occlusion Detected for an object menu! You might need to move to another location, move it closer or switch it to compact version if possible. You can click 'Accept' to switch to automatic mode.", true);
                        suggestionPanel.gameObject.GetComponent<Follow>().MinDistance = 0.4f;
                        suggestionPanel.gameObject.GetComponent<Follow>().MaxDistance = 0.4f;
                        suggestionPanel.gameObject.GetComponent<Follow>().DefaultDistance = 0.4f;
                        suggestionPanel.gameObject.transform.forward = head.transform.forward;
                        suggestionTime = 0;
                        accumulatedTimeForSuggestion = 0;
                    }
                }
                else {
                    accumulatedTimeForSuggestion = 0;
                }                
            }

        }
        private void CheckInactivity() {
            GameObject gazeTarget = CoreServices.InputSystem.GazeProvider.GazeTarget;
            if (gazeTarget != null) {
                if (gazeTarget.transform.IsChildOf(gameObject.transform)) {
                    inactivityTime = 0;
                }
                else {
                    inactivityTime += Time.deltaTime;
                }              
            }
            else {
                inactivityTime += Time.deltaTime;
            }
            if (inactivityTime > inactivityTimeThreshold) {
                inactivityTime = 0;
                Close();
            }
        }

        //Get the axis-aligned bounding box (AABB) which contains all renderers or colliders of the object depending on "boundingBoxType"
        private Bounds GetBoundingBox() {
            List<Bounds> allBounds = new List<Bounds>();
            Bounds bound = new Bounds(transform.position, Vector3.zero);
            switch (boundingBoxType) {
                case BoundsType.BasedOnColliders:
                    foreach (Collider c in gameObject.GetComponentsInChildren<Collider>()) {
                        allBounds.Add(c.bounds);
                    }
                    break;
                case BoundsType.BasedOnRenderers:
                    foreach (Renderer r in gameObject.GetComponentsInChildren<Renderer>()) {
                        allBounds.Add(r.bounds);
                    }
                    break;
                default:
                    foreach (Collider c in gameObject.GetComponentsInChildren<Collider>()) {
                        allBounds.Add(c.bounds);
                    }
                    break;
            }
            foreach (Bounds b in allBounds) {
                bound.Encapsulate(b);
            }
            return bound;
/*            Vector3 maxPoint = Vector3.negativeInfinity;
            Vector3 minPoint = Vector3.positiveInfinity;
            foreach (Bounds b in allBounds) {
                if (b.max.x > maxPoint.x) {
                    maxPoint.x = b.max.x;
                }
                if (b.max.y > maxPoint.y) {
                    maxPoint.y = b.max.y;
                }
                if (b.max.z > maxPoint.z) {
                    maxPoint.z = b.max.z;
                }
                if (b.min.x < minPoint.x) {
                    minPoint.x = b.min.x;
                }
                if (b.min.y < minPoint.y) {
                    minPoint.y = b.min.y;
                }
                if (b.min.z < minPoint.z) {
                    minPoint.z = b.min.z;
                }
            }
            Vector3 size = new Vector3(maxPoint.x - minPoint.x, maxPoint.y - minPoint.y, maxPoint.z - minPoint.z);
            Vector3 center = new Vector3((minPoint.x + maxPoint.x) / 2, (minPoint.y + maxPoint.y) / 2, (minPoint.z + maxPoint.z) / 2);
            return new Bounds(center, size);*/

        }

        //Only use this method for object menus, because for main menus we have the SurfaceMagnetismSafetyOffset so that it will never (ideally) collide with the spatial surfaces.
        private bool CollideWithSpatialMapping() {          
            return Physics.CheckBox(GetBoundingBox().center, GetBoundingBox().extents, Quaternion.identity, placementService.SpatialAwarenessLayer);
        }

        #endregion Private Methods
    }
}
