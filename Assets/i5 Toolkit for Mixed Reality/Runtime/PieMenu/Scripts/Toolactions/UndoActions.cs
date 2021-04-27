using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.Core.ServiceCore;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// The undo and redo action
    /// </summary>
    public class UndoActions : MonoBehaviour
    {
        /// <summary>
        /// Undo the last action and put it on the redo stack
        /// </summary>
        public void UndoToolAction()
        {
            if (ServiceManager.GetService<CommandStackService>().undoActionStack.Count > 0)
            {
                IToolAction action = (IToolAction)ServiceManager.GetService<CommandStackService>().undoActionStack.Pop();
                action.UndoAction();
                ServiceManager.GetService<CommandStackService>().redoActionStack.Push(action);
            }
        }

        /// <summary>
        /// Redo the last action and put in on the undo stack
        /// </summary>
        public void RedoToolAction()
        {

            if (ServiceManager.GetService<CommandStackService>().redoActionStack.Count > 0)
            {
                IToolAction action = (IToolAction)ServiceManager.GetService<CommandStackService>().redoActionStack.Pop();
                action.DoAction();
                ServiceManager.GetService<CommandStackService>().undoActionStack.Push(action);
            }
        }
    }
}
