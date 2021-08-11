using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit;

namespace i5.Toolkit.MixedReality.PieMenu
{

    public class PieMenuManagerShell : MonoBehaviour, IPieMenuManagerShell, IMixedRealityInputActionHandler, IMixedRealityGestureHandler, IMixedRealityHandJointHandler
    {
        [SerializeField]
        GameObject pieMenuPrefab;

        GameObject instantiatedPieMenu;

        IMixedRealityInputSource invokingSource;

        IPieMenuManagerCore core;

        // Registers the handlers in the input system. Otherwise, they will recive events only when a pointer has this object in focus.
        private void OnEnable()
        {
            core = new PieMenuManagerCore();
            core.shell = this;
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityGestureHandler>(this);
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityHandJointHandler>(this);
        }

        // Deregisters all handlers, otherwise it will recive events even after deactivcation.
        private void OnDisable()
        {
            core = null;
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
        }

        public void InstantiatePieMenu(Vector3 position, Quaternion rotation, IMixedRealityPointer pointer)
        {
            instantiatedPieMenu = Instantiate(pieMenuPrefab, position, rotation);
            instantiatedPieMenu.GetComponent<IPieMenuRendererShell>().Constructor(pointer);
        }

        public void DestroyPieMenu()
        {
            Destroy(instantiatedPieMenu);
        }

        public void SetupTool(MenuEntry currentEntry, IMixedRealityControllerVisualizer vizualizer)
        {
            ViveWandVirtualTool virtualTool = vizualizer.GameObjectProxy.GetComponentInChildren<ViveWandVirtualTool>();
            virtualTool.SetupTool(currentEntry);
        }


        void IMixedRealityInputActionHandler.OnActionStarted(BaseInputEventData eventData)
        {
            ToolSetupService toolSetup = ServiceManager.GetService<ToolSetupService>();
            core.MenuOpen(eventData, instantiatedPieMenu != null, toolSetup,  ref invokingSource);
        }

        void IMixedRealityInputActionHandler.OnActionEnded(BaseInputEventData eventData)
        {
            ToolSetupService toolSetup = ServiceManager.GetService<ToolSetupService>();
            int currentlyHighlighted = instantiatedPieMenu != null ? instantiatedPieMenu.GetComponent<IPieMenuRendererShell>().getCurrentlyHighlighted() : -1;
            core.MenuClose(eventData, instantiatedPieMenu != null, toolSetup, currentlyHighlighted, ref invokingSource);
        }

        //Gesture events
        void IMixedRealityGestureHandler.OnGestureCanceled(InputEventData eventData)
        {
            Debug.Log("Cancle");
        }

        void IMixedRealityGestureHandler.OnGestureCompleted(InputEventData eventData)
        {
            Debug.Log("Complete");
        }

        void IMixedRealityGestureHandler.OnGestureStarted(InputEventData eventData)
        {
            Debug.Log("Start");
        }

        void IMixedRealityGestureHandler.OnGestureUpdated(InputEventData eventData)
        {
            Debug.Log("Update");
        }

        void IMixedRealityHandJointHandler.OnHandJointsUpdated(InputEventData<System.Collections.Generic.IDictionary<Microsoft.MixedReality.Toolkit.Utilities.TrackedHandJoint, Microsoft.MixedReality.Toolkit.Utilities.MixedRealityPose>> eventData)
        {
            Debug.Log(eventData.MixedRealityInputAction.Description);
        }
    }
}
