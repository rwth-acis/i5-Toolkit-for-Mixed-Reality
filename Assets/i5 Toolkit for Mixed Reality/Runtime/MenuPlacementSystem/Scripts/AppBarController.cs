using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using UnityEngine;

namespace i5.Toolkit.MixedReality.MenuPlacementSystem {
    /// <summary>
    /// This script extends the functionalities of the App Bar from VIAProMa
    /// If more functionalities are required, it is recommanded to write another script inherits this script or an additional script added on the App Bar object.
    /// </summary>
    public class AppBarController : MonoBehaviour {

        //The menu object
        private GameObject targetMenuObject;
        private MenuHandler handler;
        private MenuPlacementService placementService;
        private PlacementMessage message = new PlacementMessage();
        [SerializeField] private GameObject slider;
        public Vector3 StartPosition { get; private set; }
        public Quaternion StartRotation { get; private set; }
        public Vector3 StartScale { get; private set; }

        public float StartSliderValue { get; private set; }

        public GameObject TargetMenuObject
        {
            get => targetMenuObject;
            set
            {
                targetMenuObject = value;
            }
        }

        // Start is called before the first frame update
        void Start() {
            targetMenuObject.GetComponent<BoxCollider>().enabled = false;
            gameObject.GetComponent<AppBar>().Target = targetMenuObject.GetComponent<BoundsControl>();
            handler = targetMenuObject.GetComponent<MenuHandler>();
            placementService = ServiceManager.GetService<MenuPlacementService>();
            (targetMenuObject.GetComponent<ObjectManipulator>() ?? targetMenuObject.AddComponent<ObjectManipulator>()).enabled = false;
            //targetMenuObject.GetComponent<ObjectManipulator>().OnManipulationStarted.AddListener(delegate () { targetMenuObject.GetComponent<BoundsControl>().HighlightWires; });          
            if (!targetMenuObject.GetComponent<NearInteractionGrabbable>()) {
                targetMenuObject.AddComponent<NearInteractionGrabbable>();
            }
            if (handler.ConstantViewSizeEnabled) {
                slider.GetComponent<PinchSlider>().SliderValue = targetMenuObject.GetComponent<FinalPlacementOptimizer>().TargetViewPercentV;
            }
            
        }

        void Update() {
            if(placementService.PlacementMode != MenuPlacementMode.Adjustment) {
                targetMenuObject.GetComponent<BoxCollider>().enabled = false;
            }            
        }

        void LateUpdate() {
            transform.rotation = targetMenuObject.transform.rotation;
        }

        public void OnAppBarExpand() {
            StartPosition = targetMenuObject.transform.position;
            StartRotation = targetMenuObject.transform.rotation;
            StartScale = targetMenuObject.transform.localScale;
            slider.GetComponent<PinchSlider>().SliderValue = targetMenuObject.GetComponent<FinalPlacementOptimizer>().TargetViewPercentV;
            StartSliderValue = slider.GetComponent<PinchSlider>().SliderValue;
            targetMenuObject.GetComponent<BoxCollider>().enabled = true;
            placementService.EnterAdjustmentMode();
        }

        

        public void OnAppBarCollapse() {
            targetMenuObject.GetComponent<BoxCollider>().enabled = false;
            placementService.ExitAdjustmentMode();
        }

        public void Retrieve() {
            handler.Retrieve();
            StartPosition = targetMenuObject.transform.position;
            StartRotation = targetMenuObject.transform.rotation;
            StartScale = targetMenuObject.transform.localScale;
            slider.GetComponent<PinchSlider>().SliderValue = targetMenuObject.GetComponent<FinalPlacementOptimizer>().TargetViewPercentV;
            StartSliderValue = slider.GetComponent<PinchSlider>().SliderValue;
        }

        public void Close() {         
            handler.Close();
            placementService.ExitAdjustmentMode();
            Destroy(gameObject);
        }

        public void SwitchVariant() {
            if(placementService.PreviousMode == MenuPlacementMode.Manual) {
                handler.ExitManualMode();
                if (handler.VariantType == VariantType.Compact) {
                    message.switchType = PlacementMessage.SwitchType.CompactToFloating;
                }
                else {
                    message.switchType = PlacementMessage.SwitchType.FloatingToCompact;
                }
                placementService.UpdatePlacement(message, targetMenuObject);
                message.switchType = PlacementMessage.SwitchType.NoSwitch;
            }
            else {
                Component suggestionPanel = Dialog.Open(placementService.SuggestionPanel, DialogButtonType.OK, "Not in manual mode", 
                    "You are currently not in manual mode, so you cannot switch to another variant. Please first switch to manual mode", true);
                suggestionPanel.gameObject.GetComponent<Follow>().MinDistance = 0.4f;
                suggestionPanel.gameObject.GetComponent<Follow>().MaxDistance = 0.4f;
                suggestionPanel.gameObject.GetComponent<Follow>().DefaultDistance = 0.4f;
                suggestionPanel.gameObject.transform.forward = CameraCache.Main.transform.forward;
            }
        }

        public void OnSliderValueUpdate() {            
            if(placementService.PlacementMode == MenuPlacementMode.Adjustment && slider.activeSelf) {
                targetMenuObject.GetComponent<FinalPlacementOptimizer>().TargetViewPercentV = slider.GetComponent<PinchSlider>().SliderValue;
                targetMenuObject.transform.localScale = StartScale * (slider.GetComponent<PinchSlider>().SliderValue / StartSliderValue);
            }
        }

        public void SwitchReferenceType() {
            if (placementService.PreviousMode == MenuPlacementMode.Manual) {
                // Set to user-referenced (lock)
                if (targetMenuObject.transform.parent == null) {
                    targetMenuObject.transform.parent = CameraCache.Main.transform;
                    ButtonConfigHelper helper = transform.Find("BaseRenderer/ButtonParent/Lock&Unlock").gameObject.GetComponent<ButtonConfigHelper>();
                    helper.MainLabelText = "Unlock";
                    helper.SeeItSayItLabelText = "Say \"Unlock\"";
                    helper.SetQuadIconByName("Unlock");                   
                }
                // Set to world-referenced (unlock)
                else {
                    targetMenuObject.transform.parent = null;
                    ButtonConfigHelper helper = transform.Find("BaseRenderer/ButtonParent/Lock&Unlock").gameObject.GetComponent<ButtonConfigHelper>();
                    helper.MainLabelText = "Lock";
                    helper.SeeItSayItLabelText = "Say \"Lock\"";
                    helper.SetQuadIconByName("Lock");
                }
            }
            else {
                Component suggestionPanel = Dialog.Open(placementService.SuggestionPanel, DialogButtonType.OK, "Not in manual mode",
                    "You are currently not in manual mode, so you cannot change the reference type. Please first switch to manual mode", true);
                suggestionPanel.gameObject.GetComponent<Follow>().MinDistance = 0.4f;
                suggestionPanel.gameObject.GetComponent<Follow>().MaxDistance = 0.4f;
                suggestionPanel.gameObject.GetComponent<Follow>().DefaultDistance = 0.4f;
                suggestionPanel.gameObject.transform.forward = CameraCache.Main.transform.forward;
            }          
        }

        public void OnAdjustment() {
            if(placementService.PreviousMode == MenuPlacementMode.Manual) {
                targetMenuObject.GetComponent<BoundsControl>().ScaleHandlesConfig.ShowScaleHandles = true;
            }
            if (handler.ConstantViewSizeEnabled && placementService.PreviousMode == MenuPlacementMode.Automatic) {
                slider.SetActive(true);
            }
            targetMenuObject.GetComponent<ObjectManipulator>().enabled = true;            
            StartPosition = targetMenuObject.transform.position;
            StartRotation = targetMenuObject.transform.rotation;
            StartScale = targetMenuObject.transform.localScale;
            StartSliderValue = slider.GetComponent<PinchSlider>().SliderValue;
        }

        public void AdjustmentEnd() {
            slider.SetActive(false);
            targetMenuObject.GetComponent<ObjectManipulator>().enabled = false;
            if (placementService.PreviousMode == MenuPlacementMode.Manual) {
                targetMenuObject.GetComponent<BoundsControl>().ScaleHandlesConfig.ShowScaleHandles = false;
            }
            if (StartPosition != targetMenuObject.transform.position || StartRotation != targetMenuObject.transform.rotation || StartScale != targetMenuObject.transform.localScale) {
                Tuple<Vector3, Quaternion, Vector3, float> lastOffsets = new Tuple<Vector3, Quaternion, Vector3, float>(StartPosition, StartRotation, StartScale, StartSliderValue);
                Tuple<Vector3, Quaternion, Vector3, float> newOffsets = new Tuple<Vector3, Quaternion, Vector3, float>(targetMenuObject.transform.position, targetMenuObject.transform.rotation, targetMenuObject.transform.localScale, slider.GetComponent<PinchSlider>().SliderValue);
                handler.SaveOffsetBeforeManipulation(lastOffsets);
                handler.UpdateOffset(newOffsets, lastOffsets);
            }
        }
    }
}

