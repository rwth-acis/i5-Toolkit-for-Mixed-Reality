using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;


namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// Manages the input events to open and close the pie menu
    /// </summary>
    public class PieMenuManagerCore : IPieMenuManagerCore
    {
        public IPieMenuManagerShell shell { get; set; }

        /// <summary>
        /// Opens the PieMenu and sets it at the tip of the tool
        /// </summary>
        /// <param name="eventData"></param> The data from the corresponding input event
        public void MenuOpen(BaseInputEventData eventData, bool pieMenuInstatiated, ToolSetupService toolSetupService,
            ref IMixedRealityInputSource invokingSource)
        {
            //Check, if the Pie Menu was already opend by another controller
            if (!pieMenuInstatiated && !eventData.used && eventData.MixedRealityInputAction == toolSetupService.toolSetup.menuAction)
            {
                var pointer = eventData.InputSource.Pointers[0];
                invokingSource = eventData.InputSource;
                shell.InstantiatePieMenu(pointer.Position, Quaternion.identity, pointer);
                eventData.Use();
            }
        }

        /// <summary>
        /// Closes the PieMenu and sets the current menu entry of the tool that opend it to the currently selected entry
        /// </summary>
        /// <param name="eventData"></param> The data from the corresponding input event
        public void MenuClose(BaseInputEventData eventData, bool pieMenuInstatiated, ToolSetupService toolSetupService, 
            int currentlyHighlighted, ref IMixedRealityInputSource invokingSource)
        {
            //Only the input source that opend the menu can close it again
            if (eventData.InputSource == invokingSource && pieMenuInstatiated && !eventData.used && eventData.MixedRealityInputAction == toolSetupService.toolSetup.menuAction)
            {
                MenuEntry currentEntry = toolSetupService.toolSetup.menuEntries[currentlyHighlighted];
                shell.SetupTool(currentEntry, eventData.InputSource.Pointers[0].Controller.Visualizer);
                shell.DestroyPieMenu();
                invokingSource = null;
                eventData.Use();
            }
        }
    }
}
