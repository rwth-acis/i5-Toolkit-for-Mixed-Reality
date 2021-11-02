using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.Core.ServiceCore;
using TMPro;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public interface IViveWandShell
    {
        void SetGameObjectActive(string key, bool active);
        bool ToolSetupExists();
        void DisableDescriptionTextCoroutine(bool start);
        void AddGameobjectToBuffer(string name, string key);
        bool RemoveGameobjectFromBuffer(string key);
        void SetTMPText(string key, string text);
        HashSet<IMixedRealityInputSource> GetInputSources();
        bool GameObjectProxyEqualsOwnObject(IMixedRealityControllerVisualizer visualizer);
        void SetOwnSource();
        void RegisterHandler<T>() where T : UnityEngine.EventSystems.IEventSystemHandler;
        void UnregisterHandler<T>() where T : UnityEngine.EventSystems.IEventSystemHandler;
        PieMenuSetup GetPieMenuSetup();
        void SetIcon(string key, Sprite icon);
        void InvokeEvent<T>(UnityEvent<T> inputEvent, T eventData) where T : BaseEventData;
    }
}
