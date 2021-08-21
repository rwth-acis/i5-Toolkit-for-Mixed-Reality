using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public class ViveWandToolCore : ViveWandCore
    {
        MenuEntry defaultEntry;
        

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
            shell.AddGameobjectToBuffer(canvasName,canvasName);
            shell.SetGameObjectActive(canvasName,true);

            if (icon != null)
            {
                //iconCanvas.GetComponentInChildren<Image>().sprite = icon;
                shell.SetIcon(canvasName,icon);
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
    }
}
