using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// This needs to be added the scene, to use the ViveWand telporter functionality. Connects the ViveWandTeleporterCore with the Unity environment.
    /// </summary>
    public class ViveWandTeleporterShell : ViveWandShell
    {


        private void OnEnable()
        {
            core = new ViveWandTeleporterCore();
            core.shell = this;
            ((ViveWandTeleporterCore)core).OnEnable();
        }

        private void OnDisable()
        {
            ((ViveWandTeleporterCore)core).OnDisable();
        }
    }
}
