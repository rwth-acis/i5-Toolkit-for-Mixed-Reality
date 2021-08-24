using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public interface IViveWandToolShell : IViveWandShell
    {
        MenuEntry currentEntry { get; set; }
        void SetupToolWaitForService();
        void InvokeEvent(InputActionUnityEvent inputEvent, BaseInputEventData eventData);
        void SetupTool(MenuEntry newEntry);
    }
}
