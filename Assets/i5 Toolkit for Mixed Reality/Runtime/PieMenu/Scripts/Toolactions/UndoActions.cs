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
            ServiceManager.GetService<CommandStackService>().UndoAction();
        }

        /// <summary>
        /// Redo the last action and put in on the undo stack
        /// </summary>
        public void RedoToolAction()
        {

            ServiceManager.GetService<CommandStackService>().RedoAction();
        }
    }
}
