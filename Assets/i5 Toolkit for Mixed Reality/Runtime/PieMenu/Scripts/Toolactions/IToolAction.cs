using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// An interface for toolactions that shall be usable by the command stack manager in order to be un- and redoable
    /// </summary>
    public interface IToolAction
    {
        void DoAction();
        void UndoAction();
    }
}
