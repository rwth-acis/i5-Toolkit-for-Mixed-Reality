using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public interface IViveWandToolShell : IViveWandShell
    {
        MenuEntry currentEntry { get; set; }
        void SetupToolWaitForService();
        
        void SetupTool(MenuEntry newEntry);

        //Methods for managing the hover events
        void SetTarget(IPointerResult pointerResult);
        bool TargetEqualsOldTarget();
        FocusEventData GenerateFocusEventData();
        bool OldFocusTargetIsNull();
        bool TargetIsNull();
        void SetOldTarget();
        Vector2 thumbPosition { get; set; }
    }
}
