using i5.Toolkit.Core.ServiceCore;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

namespace i5.Toolkit.MixedReality.MenuPlacementSystem {

    /// <summary>
    /// This script is used to cache a menu variant before pass it to the Menu Placement System.
    /// </summary>
    public class RuntimeMenuConfigurer : MonoBehaviour, IMixedRealityPointerHandler {
        public MenuType MenuType { get; private set; }
        public VariantType VariantType { get; private set; }
        public bool BothVariants { get; private set; }

        [Tooltip("The material used for highlighting selected objects.")]
        [SerializeField] private Material highlightWireMaterial;

        // To indicate what the user should select now.
        private enum Phase {
            Configuration,
            FloatingVariant,
            CompactVariant,
            TargetObject,
            End
        }
        private Phase currentPhase;
        private bool inSelectingProcess;
        private MenuPlacementService placementService;
        // fileds for UI components
        private Interactable typeSwitchButton;
        private Interactable variantSwitchButton;
        private Interactable checkBoxBothVariant;
        private GameObject confirmButton;
        private GameObject nextButton;
        private TextMeshPro messageText;
        
        // fields for selected objects
        private FocusProvider focusProvider;
        private MenuVariants menuGenerated;
        private GameObject focusedObject;
        private GameObject floatingMenu;
        private GameObject compactMenu;
        private GameObject targetObject;     
        private GameObject focusedObjectHighlighter;
        private GameObject floatingMenuHighlighter;
        private GameObject compactMenuHighlighter;
        private GameObject targetObjectHighlighter;

        private void Start() {
            placementService = ServiceManager.GetService<MenuPlacementService>();
            inSelectingProcess = false;
            typeSwitchButton = transform.Find("Type Switch").GetComponent<Interactable>();
            variantSwitchButton = transform.Find("Variant Switch").GetComponent<Interactable>();
            checkBoxBothVariant = transform.Find("Checkbox Both Variant").GetComponent<Interactable>();
            confirmButton = transform.Find("Confirm Button").gameObject;
            nextButton = transform.Find("Next Button").gameObject;
            messageText = transform.Find("Message Text").GetComponent<TextMeshPro>();
            focusProvider = (FocusProvider)CoreServices.FocusProvider;
            currentPhase = Phase.Configuration;
            EndSelectingProcess();  //Initialize
            ShowMessage("Please configure menu properties");
        }

        private void Update() {
            if (inSelectingProcess) {
                GetFocusedObject();
            }
            Debug.Log(compactMenu);
        }

        private void OnDisable() {
            EndSelectingProcess();
        }

        /// <summary>
        /// Configure the menu type
        /// </summary>
        public void SwitchMenuType() {
            if(MenuType == MenuType.MainMenu) {
                MenuType = MenuType.ObjectMenu;
            }
            else {
                MenuType = MenuType.MainMenu;
            }
        }
        
        /// <summary>
        /// Configure the variant type
        /// </summary>
        public void SwitchVariantType() {
            if(VariantType == VariantType.Floating) {
                VariantType = VariantType.Compact;
            }
            else {
                VariantType = VariantType.Floating;
            }
        }

        /// <summary>
        /// Add both variant to the menu
        /// </summary>
        public void ConfirmAddingBothVariants() {
            BothVariants = !BothVariants;
        }

        /// <summary>
        /// Start to select menus and target objects if any, and the property settings will be locked and unmodifiable, 
        /// until the configure panel is closed or the process is done.
        /// </summary>
        public void StartSelectingProcess() {
            inSelectingProcess = true;
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
            typeSwitchButton.CanDeselect = false;
            typeSwitchButton.CanSelect = false;
            variantSwitchButton.CanDeselect = false;
            variantSwitchButton.CanSelect = false;
            checkBoxBothVariant.CanDeselect = false;
            checkBoxBothVariant.CanSelect = false;
            if (BothVariants) {
                currentPhase = Phase.FloatingVariant;
                ShowMessage("Please select the floating variant.");
            }else if (VariantType == VariantType.Floating) {
                currentPhase = Phase.FloatingVariant;
                ShowMessage("Please select the floating variant.");
            }
            else {
                currentPhase = Phase.CompactVariant;
                ShowMessage("Please select the compact variant.");
            }
            focusedObjectHighlighter = new GameObject("Focused Object Highlighter");
            floatingMenuHighlighter = new GameObject("Floating Menu Highlighter");
            compactMenuHighlighter = new GameObject("Compact Menu Highlighter");
            targetObjectHighlighter = new GameObject("Target Object Highlighter");
        }

        /// <summary>
        /// Initialize the properties after selecting objects.
        /// </summary>
        public void EndSelectingProcess() {
            inSelectingProcess = false;
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
            typeSwitchButton.CanDeselect = true;
            typeSwitchButton.CanSelect = true;
            variantSwitchButton.CanDeselect = true;
            variantSwitchButton.CanSelect = true;
            checkBoxBothVariant.CanDeselect = true;
            checkBoxBothVariant.CanSelect = true;
            menuGenerated = new MenuVariants();
            BothVariants = false;
            floatingMenu = null;
            compactMenu = null;
            targetObject = null;
            MenuType = MenuType.MainMenu;
            VariantType = VariantType.Floating;
            confirmButton.SetActive(false);
            nextButton.SetActive(true);
            Destroy(focusedObjectHighlighter);
            Destroy(floatingMenuHighlighter);
            Destroy(compactMenuHighlighter);
            Destroy(targetObjectHighlighter);
        }

        /// <summary>
        /// Controls what message should be shown on the configuration panel.
        /// Invoked in the OnClick() methods of the "next button" on the panel.
        /// </summary>
        public void EnterNextPhase() {
            if (currentPhase == Phase.Configuration) {
                StartSelectingProcess();
                return;
            }
            //The user should have selected the first menu variant until now.
            if (BothVariants) {
                if(floatingMenu != null && compactMenu == null) {
                    if(MenuType == MenuType.ObjectMenu) {
                        currentPhase = Phase.TargetObject;
                        ShowMessage("Please select the target object.");
                    }
                    else {
                        currentPhase = Phase.CompactVariant;
                        ShowMessage("Please select the compact variant.");
                    }
                    return;
                }
                if(compactMenu == null && targetObject != null) {
                    currentPhase = Phase.CompactVariant;
                    ShowMessage("Please select the compact variant.");
                    return;
                }
                if(compactMenu != null) {
                    currentPhase = Phase.End;
                    nextButton.SetActive(false);
                    confirmButton.SetActive(true);
                    inSelectingProcess = false;
                    ShowMessage("Please click the confirm button to finish.");
                    return;
                }
            }
            else {
                if(MenuType == MenuType.ObjectMenu) {
                    if(VariantType == VariantType.Floating) {
                        if (floatingMenu != null && currentPhase == Phase.FloatingVariant) {
                            currentPhase = Phase.TargetObject;
                            ShowMessage("Please select the target object.");
                            return;
                        }
                    }
                    else {                   
                        if (compactMenu != null && currentPhase == Phase.CompactVariant) {
                            currentPhase = Phase.TargetObject;
                            ShowMessage("Please select the target object.");
                            return;
                        }
                    }
                    if (targetObject != null && currentPhase == Phase.TargetObject) {
                        currentPhase = Phase.End;
                        nextButton.SetActive(false);
                        confirmButton.SetActive(true);
                        inSelectingProcess = false;
                        ShowMessage("Please click the confirm button to finish");
                        return;
                    }
                }
                else {
                    if(VariantType == VariantType.Floating) {
                        if(floatingMenu == null) {
                            return;
                        }
                    }
                    else {
                        if(compactMenu == null) {
                            return;
                        }
                    }
                    currentPhase = Phase.End;
                    nextButton.SetActive(false);
                    confirmButton.SetActive(true);
                    inSelectingProcess = false;
                    ShowMessage("Please click the confirm button to finish");
                    return;
                }
            }
        }

        /// <summary>
        /// Add the cached MenuVariant to the Menu Placement System as a runtime menu.
        /// Reset the properties for the next one.
        /// </summary>
        public void AddMenuToPlacementSystem() {
            if (floatingMenu) {
                UnhighlightObject(floatingMenuHighlighter);
                MenuHandler handler = floatingMenu.AddComponent<MenuHandler>();
                handler.MenuID = placementService.GetMaximumMenuID() + 1;
                handler.MenuType = MenuType;
                handler.VariantType = VariantType.Floating;
                handler.IsRuntimeMenu = true;
                if(MenuType == MenuType.ObjectMenu) {
                    handler.TargetObject = targetObject;
                    handler.OrbitalOffset = new Vector3(0.2f, 0, 0);
                }
                menuGenerated.floatingMenu = floatingMenu;
            }
            if (compactMenu) {
                UnhighlightObject(compactMenuHighlighter);
                MenuHandler handler = compactMenu.AddComponent<MenuHandler>();
                handler.MenuID = placementService.GetMaximumMenuID() + 2;
                handler.MenuType = MenuType;
                handler.VariantType = VariantType.Compact;
                handler.IsRuntimeMenu = true;
                if(MenuType == MenuType.ObjectMenu) {
                    handler.TargetObject = targetObject;
                    handler.OrbitalOffset = new Vector3(0.1f, 0, 0);
                }
                menuGenerated.compactMenu = compactMenu;
            }
            if (BothVariants) {
                compactMenu.SetActive(false);
            }
            if(MenuType == MenuType.ObjectMenu) {
                UnhighlightObject(targetObjectHighlighter);
            }
            placementService.AddMenuRuntime(menuGenerated);
            //Initialize solvers, etc.
            if (floatingMenu) {
                floatingMenu.GetComponent<MenuHandler>().Open(targetObject);
            }
            else {
                compactMenu.GetComponent<MenuHandler>().Open(targetObject);
            }
            currentPhase = Phase.End;
        }

        /// <summary>
        /// Add the selected object (whose bounding box is highlighted) to the cached MenuVariant
        /// </summary>
        public void OnPointerClicked(MixedRealityPointerEventData eventData) {
            if (focusedObject) {
                switch (currentPhase) {
                    case Phase.FloatingVariant:
                        floatingMenu = focusedObject;
                        HighlightObject(floatingMenuHighlighter, floatingMenu);
                        ShowMessage("Floating variant selected, click Next to continue.");
                        break;
                    case Phase.TargetObject:
                        if(floatingMenu == focusedObject) {
                            ShowMessage("The selected object is already the floating variant.");
                        }
                        else if(compactMenu == focusedObject) {
                            ShowMessage("The selected object is already the compact variant.");
                        }
                        else {                 
                            targetObject = focusedObject;
                            HighlightObject(targetObjectHighlighter, targetObject);
                            ShowMessage("Target object selected, click Next to continue.");
                        }
                        break;
                    case Phase.CompactVariant:
                        if(floatingMenu == focusedObject) {
                            ShowMessage("The selected object is already the floating variant.");
                        }
                        else if(targetObject == focusedObject) {
                            ShowMessage("The selected object is already the target object");
                        }
                        else {
                            compactMenu = focusedObject;
                            HighlightObject(compactMenuHighlighter, compactMenu);
                            ShowMessage("Compact variant selected, click Next to continue.");
                        }
                        break;
                }
            }          
        }

        public void OnPointerDown(MixedRealityPointerEventData eventData) {
            //Do nothing
        }

        public void OnPointerDragged(MixedRealityPointerEventData eventData) {
            //Do nothing
        }

        public void OnPointerUp(MixedRealityPointerEventData eventData) {
            //Do nothing
        }

        // Show a message on the panel.
        private void ShowMessage(string message) {
            messageText.text = message;
        }

        // Select the object on focus and recognize it as a certain variant or target object based on current step.
        private void GetFocusedObject() {
            GameObject onFocus = focusProvider.GetFocusedObject(focusProvider.PrimaryPointer);
            //Only consider objects that are on focus, not menus that have MenuHandler, and not the configure panel it self.
            if (onFocus && onFocus.transform.root.gameObject != gameObject && onFocus.transform.root.gameObject.GetComponent<MenuHandler>() == null) {
                if (currentPhase == Phase.FloatingVariant || currentPhase == Phase.CompactVariant) {
                    //Only consider objects on the runtime menu layer.
                    if ((1 << onFocus.transform.root.gameObject.layer & placementService.RuntimeMenuLayer) > 0) {
                        focusedObject = onFocus.transform.root.gameObject;
                        HighlightObject(focusedObjectHighlighter, focusedObject);
                    }                  
                }
                else {
                    // currentPhase == Phase.TargetObject
                    focusedObject = onFocus.transform.root.gameObject;
                    HighlightObject(focusedObjectHighlighter, focusedObject);
                }
                
            }
            //There is no focusedObject currently
            else {               
                if (focusedObject) {
                    //Unhighlight the last marked object.
                    UnhighlightObject(focusedObjectHighlighter);
                }
                focusedObject = null;
            }
        }

        // Use Line renderer to draw the bound of the selected object
        private void HighlightObject(GameObject highlighter, GameObject objectToHighlight) {
            LineRenderer renderer;
            if (highlighter.GetComponent<LineRenderer>()) {
                renderer = highlighter.GetComponent<LineRenderer>();
            }
            else {
                renderer = highlighter.AddComponent<LineRenderer>();
                renderer.material = highlightWireMaterial;
                renderer.startWidth = 0.005f;
                renderer.endWidth = 0.005f;
                renderer.useWorldSpace = true;
                renderer.loop = false;
                renderer.numCapVertices = 10;
            }
            RenderBound(renderer, CalculateBounds(objectToHighlight));
        }

        private void UnhighlightObject(GameObject highlighter) {
            highlighter.GetComponent<LineRenderer>().positionCount = 0;
        }

        //Set points for LineRenderer
        private void RenderBound(LineRenderer renderer, Bounds bound) {
            List<Vector3> vertices = new List<Vector3>();
            // draw the bottom
            vertices.Add(bound.min);
            vertices.Add(new Vector3(bound.max.x, bound.min.y, bound.min.z));
            vertices.Add(new Vector3(bound.max.x, bound.min.y, bound.max.z));
            vertices.Add(new Vector3(bound.min.x, bound.min.y, bound.max.z));
            vertices.Add(bound.min);
            // draw the top and the line between bottom and top
            vertices.Add(new Vector3(bound.min.x, bound.max.y, bound.min.z));
            vertices.Add(new Vector3(bound.max.x, bound.max.y, bound.min.z));
            vertices.Add(new Vector3(bound.max.x, bound.min.y, bound.min.z));
            vertices.Add(new Vector3(bound.max.x, bound.max.y, bound.min.z));
            vertices.Add(bound.max);
            vertices.Add(new Vector3(bound.max.x, bound.min.y, bound.max.z));
            vertices.Add(bound.max);
            vertices.Add(new Vector3(bound.min.x, bound.max.y, bound.max.z));
            vertices.Add(new Vector3(bound.min.x, bound.min.y, bound.max.z));
            vertices.Add(new Vector3(bound.min.x, bound.max.y, bound.max.z));
            vertices.Add(new Vector3(bound.min.x, bound.max.y, bound.min.z));
            renderer.positionCount = vertices.Count;
            renderer.SetPositions(vertices.ToArray());
        }

        //calculate bounds based on renderers
        private Bounds CalculateBounds(GameObject selectedObject) {
            List<Bounds> allBounds = new List<Bounds>();
            Bounds bound = new Bounds(selectedObject.transform.position, Vector3.zero);
            foreach (Renderer r in selectedObject.GetComponentsInChildren<Renderer>()) {
                allBounds.Add(r.bounds);
            }
            foreach (Bounds b in allBounds) {
                bound.Encapsulate(b);
            }
            return bound;
        }

    }
}

