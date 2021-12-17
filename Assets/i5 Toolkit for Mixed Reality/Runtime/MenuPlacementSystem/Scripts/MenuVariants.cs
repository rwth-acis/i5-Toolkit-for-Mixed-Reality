using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace i5.Toolkit.MixedReality.MenuPlacementSystem {

    [Serializable]
    public class MenuVariants {

        public GameObject floatingMenu;
        public GameObject compactMenu;

        public override string ToString() {
            return floatingMenu + " and " + compactMenu; 
        }

    }
}