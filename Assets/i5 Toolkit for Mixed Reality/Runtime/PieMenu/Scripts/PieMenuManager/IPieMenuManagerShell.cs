using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public interface IPieMenuManagerShell
    {
        void instantiatePieMenu(Vector3 position, Quaternion rotation, IMixedRealityPointer pointer);
        void destroyPieMenu();
    }
}
