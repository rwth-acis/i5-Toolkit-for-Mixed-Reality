using UnityEngine.UI;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using i5.Toolkit.Core.ServiceCore;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public class ViveWandTeleporter : ViveWand, IMixedRealityInputHandler<float>
    {
        /// <summary>
        /// Activates the description texts for descriptionShowTime seconds.
        /// </summary>
        public void SetupTool()
        {
            //Enable the button explain text
            GameObject menuButton = transform.Find("ButtonDescriptions").gameObject;
            menuButton.SetActive(true);

            SetText("GripText", "", ServiceManager.GetService<ToolSetupService>().toolSetup.textGrip);

            StopCoroutine(DisableDescriptions());
            //Waits descriptionShowTime befor disabling the descriptions
            StartCoroutine(DisableDescriptions());
        }

        // Registers the handlers in the input system. Otherwise, they will recive events only when a pointer has this object in focus.
        private void OnEnable()
        {
            StartCoroutine(SetOwnSource());
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler<float>>(this);
            SetupTool();
        }

        // Deregisters all handlers, otherwise it will recive events even after deactivcation.
        private void OnDisable()
        {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler<float>>(this);
        }

        // Triggerd when an input action of type float changes its value. Used for the grip button.
        void IMixedRealityInputHandler<float>.OnInputChanged(InputEventData<float> eventData)
        {
            if (IsInputSourceThis(eventData.InputSource) && eventData.MixedRealityInputAction == ServiceManager.GetService<ToolSetupService>().toolSetup.gripPressAction)
            {
                if (eventData.InputData > 0.5)
                {
                    ServiceManager.GetService<ToolSetupService>().toolSetup.OnInputActionStartedGrip.Invoke(eventData);
                }
                else
                {
                    ServiceManager.GetService<ToolSetupService>().toolSetup.OnInputActionEndedGrip.Invoke(eventData);
                }
            }
        }
    } 
}
