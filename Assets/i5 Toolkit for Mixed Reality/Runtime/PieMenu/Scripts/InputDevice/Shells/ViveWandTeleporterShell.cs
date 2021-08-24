using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.PieMenu
{
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
