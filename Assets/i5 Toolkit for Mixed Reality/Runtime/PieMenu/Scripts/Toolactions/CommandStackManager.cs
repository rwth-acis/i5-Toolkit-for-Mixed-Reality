using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.Core.ServiceCore;

/// <summary>
/// Manages the undo and redo stack for the tool actions
/// </summary>
public class CommandStackService : IService
{
    public Stack undoActionStack;
    public Stack redoActionStack;

    void IService.Initialize(IServiceManager owner)
    {
        undoActionStack = new Stack();
        redoActionStack = new Stack();
    }

    void IService.Cleanup()
    {
        undoActionStack = null;
        redoActionStack = null;
    }

    public void Initialise()
    {
        undoActionStack = new Stack();
        redoActionStack = new Stack();
    }
}
