using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public class ViveWandToolShell : ViveWand
    {
        ViveWandToolCore core;

        public MenuEntry currentEntry;


        // Start is called before the first frame update
        private void OnEnable()
        {
            core = new ViveWandToolCore();

        }
    }
}
