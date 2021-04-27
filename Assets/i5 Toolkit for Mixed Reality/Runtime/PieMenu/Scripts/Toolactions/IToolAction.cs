using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public interface IToolAction
    {
        void DoAction();
        void UndoAction();
    }
}
