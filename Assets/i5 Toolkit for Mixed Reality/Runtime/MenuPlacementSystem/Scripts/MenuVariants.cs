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
            return "Floating Variant: " + floatingMenu + "; Compact Variant: " + compactMenu; 
        }

        public MenuVariants(GameObject floatingMenu, GameObject compactMenu) {
            this.floatingMenu = floatingMenu;
            this.compactMenu = compactMenu;
        }

        public MenuVariants() {
            
        }

    }
}