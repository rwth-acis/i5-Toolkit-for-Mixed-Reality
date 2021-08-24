using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public class ViveWandToolShell : ViveWandShell, IViveWandToolShell, IMixedRealityInputActionHandler, IMixedRealityInputHandler<Vector2>
    {

        public MenuEntry currentEntry { get; set; }
        //The last recorded position on the touchpad
        public Vector2 thumbPosition { get; set; }

        public void SetupToolWaitForService()
        {
            StartCoroutine(((ViveWandToolCore)core).SetupToolWaitForService());
        }

        public void InvokeEvent<T>(UnityEvent<T> inputEvent, T eventData) where T : BaseEventData
        {
            inputEvent?.Invoke(eventData);
        }

        private void OnEnable()
        {
            core = new ViveWandToolCore();
            core.shell = this;
            ((ViveWandToolCore)core).OnEnable();
        }

        private void OnDisable()
        {
            ((ViveWandToolCore)core).OnDisable();
        }

        private void Update()
        {
            ((ViveWandToolCore)core).UpdateHoverEvents();
        }

        public void SetupTool(MenuEntry newEntry)
        {
            ((ViveWandToolCore)core).SetupTool(newEntry);
        }


        //MRTK events

        void IMixedRealityInputActionHandler.OnActionStarted(BaseInputEventData eventData)
        {
            ((ViveWandToolCore)core).OnActionStarted(eventData);
        }

        void IMixedRealityInputActionHandler.OnActionEnded(BaseInputEventData eventData)
        {
            ((ViveWandToolCore)core).OnActionEnded(eventData);
        }

        void IMixedRealityInputHandler<Vector2>.OnInputChanged(InputEventData<Vector2> eventData)
        {
            ((ViveWandToolCore)core).OnInputChanged(eventData);
        }

        //Hover events
        GameObject target;
        GameObject oldTarget;

        public void SetTarget(IPointerResult pointerResult)
        {
            target = pointerResult.CurrentPointerTarget;
        }

        public bool TargetEqualsOldTarget()
        {
            return target == oldTarget;
        }

        public FocusEventData GenerateFocusEventData()
        {
            FocusEventData data = new FocusEventData(EventSystem.current);
            data.Initialize(core.ownSource.Pointers[0], oldTarget, target);
            return data;
        }

        public bool OldFocusTargetIsNull()
        {
            return oldTarget == null;
        }

        public bool TargetIsNull()
        {
            return target == null;
        }

        public void SetOldTarget()
        {
            oldTarget = target;
        }
    }
}
