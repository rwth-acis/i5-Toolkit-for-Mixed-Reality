using UnityEngine;
using System;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.Events;
using System.Collections.Generic;

[Serializable]
public struct ToolSetup
{
    //Events and action for both controllers
    public MixedRealityInputAction triggerInputAction;
    public MixedRealityInputAction touchpadTouchActionAction;
    public MixedRealityInputAction touchpadPressAction;
    public MixedRealityInputAction gripPressAction;
    public MixedRealityInputAction menuAction;

    public float descriptionShowTime;

    //The menu entries for the virtual tool controller
    public List<MenuEntry> menuEntries;

    //The default Entry for the virtual tool controller
    public MenuEntry defaultEntry;
}
