using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit;

namespace i5.Toolkit.MixedReality.PieMenu
{

    public class PieMenuManagerShell : MonoBehaviour, IPieMenuManagerShell, IMixedRealityInputActionHandler
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
            instantiatedPieMenu.GetComponent<PieMenuRenderer>().Constructor(pointer);
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
            int currentlyHighlighted = instantiatedPieMenu != null ? instantiatedPieMenu.GetComponent<PieMenuRenderer>().currentlyHighlighted : -1;
            core.MenuClose(eventData, instantiatedPieMenu != null, toolSetup, currentlyHighlighted, ref invokingSource);
        }


    }
}
