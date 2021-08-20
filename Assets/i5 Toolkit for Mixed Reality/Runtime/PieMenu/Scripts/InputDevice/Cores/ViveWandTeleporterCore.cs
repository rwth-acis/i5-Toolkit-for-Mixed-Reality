using UnityEngine.UI;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using i5.Toolkit.Core.ServiceCore;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public class ViveWandTeleporterCore : ViveWandCore
    {
        /// <summary>
        /// Activates the description texts for descriptionShowTime seconds.
        /// </summary>
        public void SetupTool()
        {
            //Enable the button explain text
            ActivateDescriptionTexts();

            SetText("GripText", "", ServiceManager.GetService<ToolSetupService>().toolSetup.textGrip);

            shell.DisableDescriptionTextCoroutine(false);
            //Waits descriptionShowTime befor disabling the descriptions
            shell.DisableDescriptionTextCoroutine(true);
        }

        // Registers the handlers in the input system. Otherwise, they will recive events only when a pointer has this object in focus.
        public void OnEnable()
        {
            //StartCoroutine(SetOwnSource());
            shell.SetOwnSource();
            shell.RegisterHandlers();
            SetupTool();
        }

        // Deregisters all handlers, otherwise it will recive events even after deactivcation.
        private void OnDisable()
        {
            shell.UnregisterHandlers();
        }
    }
}
