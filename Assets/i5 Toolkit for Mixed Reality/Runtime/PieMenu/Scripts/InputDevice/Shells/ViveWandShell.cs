using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.Core.ServiceCore;
using TMPro;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public class ViveWandShell : MonoBehaviour, IViveWandShell, IMixedRealityInputHandler<float>
    {
        Dictionary<string, GameObject> gameObjectBuffer = new Dictionary<string, GameObject>();
        protected ViveWandCore core;

        //Callback methods

        public void SetGameObjectActive(string key, bool active)
        {
            gameObjectBuffer[key].SetActive(active);
        }

        public bool ToolSetupExists()
        {
            return ServiceManager.ServiceExists<ToolSetupService>();
        }

        public void DisableDescriptionTextCoroutine(bool start)
        {
            if (start)
                StartCoroutine(core.DisableDescriptionsAfterShowTime());
            else
                StopCoroutine(core.DisableDescriptionsAfterShowTime());
        }

        public void AddGameobjectToBuffer(string name, string key)
        {
            GameObject toSearch = transform.Find(name).gameObject;
            gameObjectBuffer.Add(key,toSearch);
        }

        public bool RemoveGameobjectFromBuffer(string key)
        {
            return gameObjectBuffer.Remove(key);
        }

        public void SetTMPText(string key, string text)
        {
            gameObjectBuffer[key].GetComponentInChildren<TMP_Text>().text = text;
        }

        public HashSet<IMixedRealityInputSource> GetInputSources()
        {
            return CoreServices.InputSystem.DetectedInputSources;
        }

        public bool GameObjectProxyEqualsOwnObject(IMixedRealityControllerVisualizer visualizer)
        {
            return visualizer?.GameObjectProxy == gameObject;
        }

        public void SetOwnSource()
        {
            StartCoroutine(core.SetOwnSource());
        }

        //MR events
        public void RegisterHandler<T>() where T : UnityEngine.EventSystems.IEventSystemHandler
        {
            CoreServices.InputSystem?.RegisterHandler<T>(this);
        }

        public void UnregisterHandler<T>() where T : UnityEngine.EventSystems.IEventSystemHandler
        {
            CoreServices.InputSystem?.UnregisterHandler<T>(this);
        }

        public void OnInputChanged(InputEventData<float> eventData)
        {
            core.OnInputChanged(eventData);
        }

        public PieMenuSetup GetPieMenuSetup()
        {
            return ServiceManager.GetService<ToolSetupService>().toolSetup;
        }

        public void SetIcon(string key, Sprite icon)
        {
            gameObjectBuffer[key].GetComponentInChildren<Image>().sprite = icon;
        }

        public void InvokeEvent<T>(UnityEvent<T> inputEvent, T eventData) where T : BaseEventData
        {
            inputEvent?.Invoke(eventData);
        }
    }
}
