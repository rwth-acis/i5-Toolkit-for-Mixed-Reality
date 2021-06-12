using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.MixedReality.PieMenu;
using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI;

public class ManipulationActionWrapper : MonoBehaviour
{

    ManiplulationAction curentManiplulationAction;

    /// <summary>
    /// Create a ManiplulationAction and save the corresponding data from the visualisation the tool currently points at in it in curentManiplulationAction
    /// </summary>
    /// <param name="data"></param> The data from the corresponding input event
    public void StartPositionRecording(BaseInputEventData data)
    {
        GameObject target = ActionHelperFunctions.GetTargetFromInputSource(data.InputSource);

        IObjectTransformer objectTransformer = FindObjectOfType<ObjectTransformer>().GetComponent<ObjectTransformer>();
        target = objectTransformer.TransformObject(target, "Manipulate");
        if (target != null)
        {
            curentManiplulationAction = new ManiplulationAction();
            curentManiplulationAction.target = target;
            curentManiplulationAction.startPosition = target.transform.localPosition;
            curentManiplulationAction.startRotation = target.transform.localRotation;
            curentManiplulationAction.startScalation = target.transform.localScale;
        }
    }


    public void EndPositionRecording(BaseInputEventData data)
    {
        GameObject target = ActionHelperFunctions.GetTargetFromInputSource(data.InputSource);

        IObjectTransformer objectTransformer = FindObjectOfType<ObjectTransformer>().GetComponent<ObjectTransformer>();
        target = objectTransformer.TransformObject(target, "Manipulate");
        if (target != null && target == curentManiplulationAction.target)
        {
            curentManiplulationAction.endPosition = target.transform.localPosition;
            curentManiplulationAction.endRotation = target.transform.localRotation;
            curentManiplulationAction.endScalation = target.transform.localScale;
            ServiceManager.GetService<CommandStackService>().AddAction(curentManiplulationAction);
        }
    }

    ManipulationInformation[] objectsThatCanBeManipulated;

    /// <summary>
    /// Activates the BoundingBox from all visualisations in the scene
    /// </summary>
    public void StartManipulating()
    {
        objectsThatCanBeManipulated = FindObjectsOfType<ManipulationInformation>();
        foreach (var objectToManipulate in objectsThatCanBeManipulated)
        {
            if (objectToManipulate.manipulationPossible)
            {
                objectToManipulate.gameObject.GetComponentInChildren<BoundsControl>().enabled = true;
                objectToManipulate.gameObject.GetComponentInChildren<ObjectManipulator>().enabled = true;
            }
        }
    }

    /// <summary>
    /// Deactivates the BoundingBox that were activated by StartAdjusting()
    /// </summary>
    public void StopManipulating()
    {
        foreach (var objectToManipulate in objectsThatCanBeManipulated)
        {
            if (objectToManipulate != null && objectToManipulate.manipulationPossible)
            {
                objectToManipulate.gameObject.GetComponentInChildren<BoundsControl>().enabled = false;
                objectToManipulate.gameObject.GetComponentInChildren<ObjectManipulator>().enabled = false;
            }
        }
    }
}
