using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

public class ToolSetupService : IService
{
    public ToolSetup toolSetup;

    public ToolSetupService(ToolSetup toolSetup)
    {
        this.toolSetup = toolSetup;
    }

    void IService.Initialize(IServiceManager owner)
    {
    }

    void IService.Cleanup()
    {
    }
}

public struct ToolSetup
{
    //Events and action for the thrigger
    public MixedRealityInputAction triggerInputAction;
    public MixedRealityInputAction touchpadTouchActionAction;
    public MixedRealityInputAction touchpadPressAction;
    public MixedRealityInputAction gripPressAction;

    public List<MenuEntry> menuEntries;

    public MenuEntry defaultEntry;
}
