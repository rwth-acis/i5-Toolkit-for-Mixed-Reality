using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public class ViveWandTeleporterShell : ViveWandShell
    {
        

        // Start is called before the first frame update
        void OnEnable()
        {
            core = new ViveWandTeleporterCore();
            core.shell = this;
            ((ViveWandTeleporterCore)core).OnEnable();
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
