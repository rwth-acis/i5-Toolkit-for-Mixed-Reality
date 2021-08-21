using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.Core.ServiceCore;
using TMPro;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public interface IViveWandShell
    {
        void SetGameObjectActive(string key, bool active);
        PieMenuSetup GetToolSetup();
        void DisableDescriptionTextCoroutine(bool start);
        void AddGameobjectToBuffer(string name, string key);
        bool RemoveGameobjectFromBuffer(string key);
        void SetTMPText(string key, string text);
        HashSet<IMixedRealityInputSource> GetInputSources();
        bool GameObjectProxyEqualsOwnObject(IMixedRealityControllerVisualizer visualizer);
        void SetOwnSource();
        void RegisterHandlers();
        void UnregisterHandlers();
        PieMenuSetup GetPieMenuSetup();
        void SetIcon(string key, Sprite icon);
    }
}
