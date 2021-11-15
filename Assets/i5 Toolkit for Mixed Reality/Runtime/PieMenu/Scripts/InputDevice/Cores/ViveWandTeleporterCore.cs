﻿using UnityEngine.UI;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using i5.Toolkit.Core.ServiceCore;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// Implements the setup of the description texts for the teleporter.
    /// </summary>
    public class ViveWandTeleporterCore : ViveWandCore
    {
        /// <summary>
        /// Activates the description texts for descriptionShowTime seconds.
        /// </summary>
        public void SetupTool()
        {
            //Enable the button explain text
            ActivateDescriptionTexts();

            SetText("GripText", "", shell.GetPieMenuSetup().defaultEntryTeleporter.gripSettings.textGrip);
            shell.DisableDescriptionTextCoroutine(false);
            //Waits descriptionShowTime befor disabling the descriptions
            shell.DisableDescriptionTextCoroutine(true);
        }

        /// <summary>
        /// Registers the handlers in the input system and setups the tool.
        /// </summary>
        public void OnEnable()
        {
            shell.SetOwnSource();
            shell.RegisterHandler<IMixedRealityInputHandler<float>>();
            SetupTool();
        }
        /// <summary>
        /// Deregisters all handlers, otherwise it will recive events even after deactivcation.
        /// </summary>
        public void OnDisable()
        {
            shell.UnregisterHandler<IMixedRealityInputHandler<float>>();
        }
    }
}
