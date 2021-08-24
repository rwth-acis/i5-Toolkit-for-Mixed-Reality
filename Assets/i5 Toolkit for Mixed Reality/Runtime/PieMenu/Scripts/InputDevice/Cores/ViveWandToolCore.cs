﻿using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public class ViveWandToolCore : ViveWandCore
    {
        MenuEntry defaultEntry;

        //The last recorded position on the touchpad
        public Vector2 thumbPosition;


        /// <summary>
        /// Activates the description texts for descriptionShowTime seconds.
        /// </summary>
        public void SetupTool(MenuEntry newEntry)
        {
            MenuEntry currentEntry = ((ViveWandToolShell)shell).currentEntry;

            if (currentEntry.toolSpecificevents.OnToolDestroyed != null)
            {
                currentEntry.toolSpecificevents.OnToolDestroyed.Invoke(null);
            }

            defaultEntry = shell.GetPieMenuSetup().defaultEntry;

            //set the new icons
            SetIcon("ToolIconCanvas", newEntry.toolSettings.iconTool, defaultEntry.toolSettings.iconTool);
            SetIcon("TouchpadRightIcon", newEntry.touchpadRightSettings.iconTouchpadRight, defaultEntry.touchpadRightSettings.iconTouchpadRight);
            SetIcon("TouchpadUpIcon", newEntry.touchpadUpSettings.iconTouchpadUp, defaultEntry.touchpadUpSettings.iconTouchpadUp);
            SetIcon("TouchpadLeftIcon", newEntry.touchpadLeftSettings.iconTouchpadLeft, defaultEntry.touchpadLeftSettings.iconTouchpadLeft);
            SetIcon("TouchpadDownIcon", newEntry.TouchpadDownSettings.iconTouchpadDown, defaultEntry.TouchpadDownSettings.iconTouchpadDown);

            if (newEntry.toolSpecificevents.OnToolCreated != null)
            {
                newEntry.toolSpecificevents.OnToolCreated.Invoke(null);
            }

            //Enable the button explain text
            ActivateDescriptionTexts();

            SetText("TouchpadRightText", newEntry.touchpadRightSettings.textTouchpadRight, defaultEntry.touchpadRightSettings.textTouchpadRight);
            SetText("TouchpadDownText", newEntry.TouchpadDownSettings.textTouchpadDown, defaultEntry.TouchpadDownSettings.textTouchpadDown);
            SetText("TouchpadLeftText", newEntry.touchpadLeftSettings.textTouchpadLeft, defaultEntry.touchpadLeftSettings.textTouchpadLeft);
            SetText("TouchpadUpText", newEntry.touchpadUpSettings.textTouchpadUp, defaultEntry.touchpadUpSettings.textTouchpadUp);
            SetText("TriggerText", newEntry.triggerSettings.textTrigger, defaultEntry.triggerSettings.textTrigger);
            SetText("GripText", newEntry.gripSettings.textGrip, defaultEntry.gripSettings.textGrip);
            SetText("GripText", "", shell.GetPieMenuSetup().textGrip);

            shell.DisableDescriptionTextCoroutine(false);
            //Waits descriptionShowTime befor disabling the descriptions
            shell.DisableDescriptionTextCoroutine(true);
            ((ViveWandToolShell)shell).currentEntry = newEntry;
        }

        // Sets the icon on the coresponding canvas on the tool.
        private void SetIcon(string canvasName, Sprite icon, Sprite defaultIcon)
        {
            //Transform iconCanvas = transform.Find(canvasName);
            shell.AddGameobjectToBuffer(canvasName, canvasName);
            shell.SetGameObjectActive(canvasName, true);

            if (icon != null)
            {
                shell.SetIcon(canvasName, icon);
            }
            else if (defaultIcon != null)
            {
                shell.SetIcon(canvasName, defaultIcon);
            }
            else
            {
                shell.SetGameObjectActive(canvasName, false);
            }
            shell.RemoveGameobjectFromBuffer(canvasName);
        }

        //Checks if the SetupService exists and if yes executes it. If not, waits a frame. This is only neccesarry in case the OneEnable of the tool
        //is called on the same frame with the ServiceRegister (for example on the first frame when both are instantiated in the scene on default)
        public IEnumerator SetupToolWaitForService()
        {
            if (shell.ToolSetupExists())
            {
                SetupTool(shell.GetToolSetup().defaultEntry);
            }
            else
            {
                yield return null;
                SetupTool(shell.GetToolSetup().defaultEntry);
            }
        }

        public void OnEnable()
        {
            shell.SetOwnSource();
            shell.RegisterHandler<IMixedRealityInputActionHandler>();
            shell.RegisterHandler<IMixedRealityInputHandler<Vector2>>();
            shell.RegisterHandler<IMixedRealityInputHandler<float>>();
            ((IViveWandToolShell)shell).SetupToolWaitForService();
        }

        public void OnDisable()
        {
            shell.UnregisterHandler<IMixedRealityInputActionHandler>();
            shell.UnregisterHandler<IMixedRealityInputHandler<Vector2>>();
            shell.UnregisterHandler<IMixedRealityInputHandler<float>>();
        }

        //MRTK Events

        /// <summary>
        /// Invokes the triggerInputAction from the current menu entry, when the trigger was pressed on this input source.
        /// </summary>
        /// <param name="eventData"></param>
        public void OnActionStarted(BaseInputEventData eventData)
        {
            IViveWandToolShell toolShell = (IViveWandToolShell)shell;
            if (eventData.MixedRealityInputAction == shell.GetToolSetup().triggerInputAction && IsInputSourceThis(eventData.InputSource))
            {
                //On Trigger event
                InvokeCurrentOrDefaultEvent(toolShell.currentEntry.triggerSettings.OnInputActionStartedTrigger, defaultEntry.triggerSettings.OnInputActionStartedTrigger, eventData);
            }
        }

        public void OnActionEnded(BaseInputEventData eventData)
        {
            if (IsInputSourceThis(eventData.InputSource))
            {
                IViveWandToolShell toolShell = (IViveWandToolShell)shell;
                MenuEntry currentEntry = toolShell.currentEntry;

                if (eventData.MixedRealityInputAction == shell.GetToolSetup().triggerInputAction)
                {
                    //The trigger was released
                    toolShell.InvokeEvent(toolShell.currentEntry.triggerSettings.OnInputActionEndedTrigger, eventData);
                }
                else if ((eventData.MixedRealityInputAction == shell.GetToolSetup().touchpadPressAction))
                {
                    //The touchpad was released
                    //Touchad
                    float angle = Vector2.SignedAngle(Vector2.right, thumbPosition);
                    if (angle > -45 && angle <= 45)
                    {
                        //Right press
                        if (currentEntry.touchpadRightSettings.OnInputActionEndedTouchpadRight.GetPersistentEventCount() > 0)
                        {
                            toolShell.InvokeEvent(currentEntry.touchpadRightSettings.OnInputActionEndedTouchpadRight, eventData);
                        }
                        else
                        {
                            toolShell.InvokeEvent(defaultEntry.touchpadRightSettings.OnInputActionEndedTouchpadRight, eventData);
                        }
                    }
                    else if (angle > 45 && angle <= 135)
                    {
                        //Up press
                        if (currentEntry.touchpadUpSettings.OnInputActionEndedTouchpadUp.GetPersistentEventCount() > 0)
                        {
                            toolShell.InvokeEvent(currentEntry.touchpadUpSettings.OnInputActionEndedTouchpadUp, eventData);
                        }
                        else
                        {
                            toolShell.InvokeEvent(defaultEntry.touchpadUpSettings.OnInputActionEndedTouchpadUp, eventData);
                        }
                    }
                    else if ((angle > 135 && angle <= 180) || (angle >= -180 && angle <= -135))
                    {
                        //Left press
                        if (currentEntry.touchpadLeftSettings.OnInputActionEndedTouchpadLeft.GetPersistentEventCount() > 0)
                        {
                            toolShell.InvokeEvent(currentEntry.touchpadLeftSettings.OnInputActionEndedTouchpadLeft, eventData);
                        }
                        else
                        {
                            toolShell.InvokeEvent(defaultEntry.touchpadLeftSettings.OnInputActionEndedTouchpadLeft, eventData);
                        }
                    }
                    else
                    {
                        //Down press
                        if (currentEntry.TouchpadDownSettings.OnInputActionEndedTouchpadDown.GetPersistentEventCount() > 0)
                        {
                            toolShell.InvokeEvent(currentEntry.TouchpadDownSettings.OnInputActionEndedTouchpadDown, eventData);
                        }
                        else
                        {
                            toolShell.InvokeEvent(defaultEntry.TouchpadDownSettings.OnInputActionEndedTouchpadDown, eventData);
                        }
                    }
                }
            }
        }

        private void InvokeCurrentOrDefaultEvent(InputActionUnityEvent fromCurrentEntry, InputActionUnityEvent fromDefaultEntry, BaseInputEventData eventData)
        {
            IViveWandToolShell toolShell = (IViveWandToolShell)shell;
            if (fromCurrentEntry.GetPersistentEventCount() > 0)
            {
                toolShell.InvokeEvent(fromCurrentEntry, eventData);
            }
            else
            {
                toolShell.InvokeEvent(fromDefaultEntry, eventData);
            }
        }

        public void OnInputChanged(InputEventData<Vector2> eventData)
        {
            if (eventData.MixedRealityInputAction == shell.GetToolSetup().touchpadTouchActionAction)
            {
                thumbPosition = eventData.InputData;
            }
        }
    }
}
