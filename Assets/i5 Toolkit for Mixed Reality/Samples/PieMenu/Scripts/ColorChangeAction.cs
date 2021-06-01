using UnityEngine;
using i5.Toolkit.MixedReality.PieMenu;

public class ColorChangeAction : IToolAction
{
    GameObject target;
    Color oldColor;
    Color newColor;

    public ColorChangeAction(GameObject target, Color newColor)
    {
        this.target = target;
        this.oldColor = target.GetComponent<Renderer>().material.color;
        this.newColor = newColor;
    }

    void IToolAction.DoAction()
    {
        target.GetComponent<Renderer>().material.color = newColor;
    }

    void IToolAction.UndoAction()
    {
        target.GetComponent<Renderer>().material.color = oldColor;
    }

}
