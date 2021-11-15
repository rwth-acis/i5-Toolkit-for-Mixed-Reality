using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.MixedReality.PieMenu;

public class DeleteAction : IToolAction
{
    GameObject target;

    public DeleteAction(GameObject target)
    {
        this.target = target;
    }

    void IToolAction.DoAction()
    {
        target.SetActive(false);
    }

    void IToolAction.UndoAction()
    {
        target.SetActive(true);
    }
}
