using UnityEngine;
using System;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.Events;
using System.Collections.Generic;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// Contains all information needed to setup a PieMenu
    /// </summary>
    [Serializable]
    public class PieMenuSetup
    {
        //Events and action for both controllers
        public MixedRealityInputAction triggerInputAction;
        public MixedRealityInputAction touchpadTouchActionAction;
        public MixedRealityInputAction touchpadPressAction;
        public MixedRealityInputAction gripPressAction;
        public MixedRealityInputAction menuAction;

        //The colors for the menu
        public Color pieMenuPieceNormalColor;
        public Color pieMenuPieceHighlighColor;

        [Tooltip("Number of seconds, the descriptions will be shown after a tool switch")]
        public float descriptionShowTime;

        //The menu entries for the virtual tool controller
        public List<MenuEntry> menuEntries;

        //The default entry for the virtual tool controller
        public MenuEntry defaultEntry;

        //The grip actions and text for the teleporter
        public string textGrip;
        public InputActionUnityEvent OnInputActionStartedGrip;
        public InputActionUnityEvent OnInputActionEndedGrip;
    } 
}
