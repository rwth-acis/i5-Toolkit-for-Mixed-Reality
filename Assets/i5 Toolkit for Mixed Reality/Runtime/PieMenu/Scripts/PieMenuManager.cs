using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit;


namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// Manages the input events to open and close the pie menu
    /// </summary>
    public class PieMenuManager : MonoBehaviour, IMixedRealityInputActionHandler
    {
        [SerializeField]
        GameObject pieMenuPrefab;

        GameObject instantiatedPieMenu;


        IMixedRealityPointer pointer;
        IMixedRealityInputSource invokingSource;


        void IMixedRealityInputActionHandler.OnActionStarted(BaseInputEventData eventData)
        {
            if (!eventData.used && eventData.MixedRealityInputAction == ServiceManager.GetService<ToolSetupService>().toolSetup.menuAction)
            {
                MenuOpen(eventData);
                eventData.Use();
            }
        }

        void IMixedRealityInputActionHandler.OnActionEnded(BaseInputEventData eventData)
        {
            if (!eventData.used && eventData.MixedRealityInputAction == ServiceManager.GetService<ToolSetupService>().toolSetup.menuAction)
            {
                MenuClose(eventData);
                eventData.Use();
            }
        }

        // Registers the handlers in the input system. Otherwise, they will recive events only when a pointer has this object in focus.
        private void OnEnable()
        {
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
        }

        // Deregisters all handlers, otherwise it will recive events even after deactivcation.
        private void OnDisable()
        {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
        }

        /// <summary>
        /// Opens the PieMenu and sets it at the tip of the tool
        /// </summary>
        /// <param name="eventData"></param> The data from the corresponding input event
        public void MenuOpen(BaseInputEventData eventData)
        {
            //Check, if the Pie Menu was already opend by another controller
            if (instantiatedPieMenu == null)
            {
                pointer = eventData.InputSource.Pointers[0];
                invokingSource = eventData.InputSource;
                instantiatedPieMenu = Instantiate(pieMenuPrefab, pointer.Position, Quaternion.identity);
                instantiatedPieMenu.GetComponent<PieMenuRenderer>().Constructor(pointer);
            }
        }

        /// <summary>
        /// Closes the PieMenu and sets the current menu entry of the tool that opend it to the currently selected entry
        /// </summary>
        /// <param name="eventData"></param> The data from the corresponding input event
        public void MenuClose(BaseInputEventData eventData)
        {
            //Only the input source that opend the menu can close it again
            if (eventData.InputSource == invokingSource && instantiatedPieMenu != null)
            {
                ViveWandVirtualTool virtualTool = eventData.InputSource.Pointers[0].Controller.Visualizer.GameObjectProxy.GetComponentInChildren<ViveWandVirtualTool>();
                MenuEntry currentEntry = ServiceManager.GetService<ToolSetupService>().toolSetup.menuEntries[instantiatedPieMenu.GetComponent<PieMenuRenderer>().currentlyHighlighted];
                virtualTool.SetupTool(currentEntry);
                Destroy(instantiatedPieMenu);
                invokingSource = null;
            }
        }
    } 
}
