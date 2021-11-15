using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// The needed callback functions for the PieMenuManagerCore
    /// </summary>
    public interface IPieMenuManagerShell
    {
        void InstantiatePieMenu(Vector3 position, Quaternion rotation, IMixedRealityPointer pointer);
        void DestroyPieMenu();
        void SetupTool(MenuEntry currentEntry, IMixedRealityControllerVisualizer vizualizer);
    }
}
