using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.Events;

namespace i5.Toolkit.MixedReality.PieMenu
{
    [Serializable]
    public struct GeneralToolSettings
    {
        public Sprite iconTool;
        public string toolName;
    }

    [Serializable]
    public struct TriggerSettings
    {
        public string textTrigger;
        public InputActionUnityEvent OnInputActionStartedTrigger;
        public InputActionUnityEvent OnInputActionEndedTrigger;
    }

    [Serializable]
    public struct GripSettings
    {
        public string textGrip;
        public InputActionUnityEvent OnInputActionStartedGrip;
        public InputActionUnityEvent OnInputActionEndedGrip;
    }

    [Serializable]
    public struct TouchpadUpSettings
    {
        public string textTouchpadUp;
        public Sprite iconTouchpadUp;
        public InputActionUnityEvent OnInputActionEndedTouchpadUp;
    }

    [Serializable]
    public struct TouchpadRightSettings
    {
        public string textTouchpadRight;
        public Sprite iconTouchpadRight;
        public InputActionUnityEvent OnInputActionEndedTouchpadRight;
    }

    [Serializable]
    public struct TouchpadDownSettings
    {
        public string textTouchpadDown;
        public Sprite iconTouchpadDown;
        public InputActionUnityEvent OnInputActionEndedTouchpadDown;
    }

    [Serializable]
    public struct TouchpadLeftSettings
    {
        public string textTouchpadLeft;
        public Sprite iconTouchpadLeft;
        public InputActionUnityEvent OnInputActionEndedTouchpadLeft;
    }

    [Serializable]
    public struct ToolSpecificevents
    {
        //Tool specific events
        public InputActionUnityEvent OnToolCreated;
        public InputActionUnityEvent OnToolDestroyed;
        public VirtualToolFocusEvent OnHoverOverTargetStart;
        public VirtualToolFocusEvent OnHoverOverTargetActive;
        public VirtualToolFocusEvent OnHoverOverTargetStop;
    }

    /// <summary>
    /// Contains everything needed to define a menu entry
    /// </summary>
    [Serializable]
    public struct MenuEntry
    {
        public GeneralToolSettings toolSettings;

        public TriggerSettings triggerSettings;

        public GripSettings gripSettings;

        //Touchpad Parameters
        public TouchpadUpSettings touchpadUpSettings;

        public TouchpadRightSettings touchpadRightSettings;

        public TouchpadDownSettings TouchpadDownSettings;

        public TouchpadLeftSettings touchpadLeftSettings;


        public ToolSpecificevents toolSpecificevents;
    }

    [Serializable]
    public class VirtualToolFocusEvent : UnityEvent<FocusEventData> { } 
}
