using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.MixedReality.PieMenu;
using i5.Toolkit.Core.ServiceCore;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI;

public class ManipulationActionWrapper : MonoBehaviour
{
    /// <summary>
    /// Create a ManiplulationAction, save the corresponding data from the visualisation the tool currently points at in it and push it on the undo stack
    /// </summary>
    /// <param name="data"></param> The data from the corresponding input event
    public void RecordPosition(BaseInputEventData data)
    {
        GameObject target = ActionHelperFunctions.GetTargetFromInputSource(data.InputSource);

        IObjectTransformer objectTransformer = FindObjectOfType<ObjectTransformer>().GetComponent<ObjectTransformer>();
        target = objectTransformer.transformObject(target, "Manipulate");
        if (target != null)
        {
            ManiplulationAction ManiplulationAction = new ManiplulationAction();
            ManiplulationAction.target = target;
            ManiplulationAction.startPosition = target.transform.position;
            ManiplulationAction.startRotation = target.transform.rotation;
            ManiplulationAction.startScalation = target.transform.localScale;
            ServiceManager.GetService<CommandStackService>().AddAction(ManiplulationAction);
        }
    }

    ManipulationInformation[] objectsThatCanBeManipulated;

    /// <summary>
    /// Activates the BoundingBox from all visualisations in the scene
    /// </summary>
    public void StartAdjusting()
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
    public void StopAdjusting()
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
