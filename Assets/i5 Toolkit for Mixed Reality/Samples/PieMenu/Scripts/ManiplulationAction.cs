using UnityEngine;
using i5.Toolkit.MixedReality.PieMenu;

public class ManiplulationAction : IToolAction
{
    public GameObject target;
    //Start 6 DOF
    public Vector3 startPosition;
    public Quaternion startRotation;
    public Vector3 startScalation;

    //End 6 DOF
    public Vector3 endPosition;
    public Quaternion endRotation;
    public Vector3 endScalation;

    /// <summary>
    /// Move the target to the end position and rotation
    /// </summary>
    void IToolAction.DoAction()
    {
        target.transform.SetPositionAndRotation(endPosition, endRotation);
        target.transform.localScale = endScalation;
    }

    /// <summary>
    /// Save the current position in the end position and move the target to the start position and rotation
    /// </summary>
    void IToolAction.UndoAction()
    {
        target.transform.SetPositionAndRotation(startPosition, startRotation);
        target.transform.localScale = startScalation;
    }
}