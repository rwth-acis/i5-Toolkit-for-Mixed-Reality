using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.Core.ServiceCore;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// Manages the undo and redo stack for the tool actions
    /// </summary>
    public class CommandStackService : IService
    {
        Stack<IToolAction> undoActionStack;
        Stack<IToolAction> redoActionStack;

        void IService.Initialize(IServiceManager owner)
        {
            undoActionStack = new Stack<IToolAction>();
            redoActionStack = new Stack<IToolAction>();
        }

        void IService.Cleanup()
        {
            undoActionStack = null;
            redoActionStack = null;
        }

        public void Initialise()
        {
            undoActionStack = new Stack<IToolAction>();
            redoActionStack = new Stack<IToolAction>();
        }

        public void AddAndPerformAction(IToolAction action)
        {
            action.DoAction();
            undoActionStack.Push(action);
        }

        public void AddAction(IToolAction action)
        {
            undoActionStack.Push(action);
        }

        public void UndoAction()
        {
            if (undoActionStack.Count > 0)
            {
                IToolAction action = undoActionStack.Pop();
                action.UndoAction();
                redoActionStack.Push(action);
            }
        }

        public void RedoAction()
        {
            if (redoActionStack.Count > 0)
            {
                IToolAction action = redoActionStack.Pop();
                action.DoAction();
                undoActionStack.Push(action);
            }
        }
    }
}
