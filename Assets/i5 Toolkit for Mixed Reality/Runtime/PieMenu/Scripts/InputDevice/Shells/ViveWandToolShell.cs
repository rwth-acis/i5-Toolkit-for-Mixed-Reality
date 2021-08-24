using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public class ViveWandToolShell : ViveWandShell, IViveWandToolShell, IMixedRealityInputActionHandler, IMixedRealityInputHandler<Vector2>
    {

        public MenuEntry currentEntry { get; set; }

        public void SetupToolWaitForService()
        {
            StartCoroutine(((ViveWandToolCore)core).SetupToolWaitForService());
        }

        public void InvokeEvent(InputActionUnityEvent inputeEvent, BaseInputEventData eventData)
        {
            inputeEvent?.Invoke(eventData);
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
    }
}
