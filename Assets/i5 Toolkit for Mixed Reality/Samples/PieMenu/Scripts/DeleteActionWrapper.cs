using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.MixedReality.PieMenu;
using i5.Toolkit.Core.ServiceCore;

public class DeleteActionWrapper : MonoBehaviour
{
    public void Delete(BaseInputEventData data)
    {
        GameObject target = ActionHelperFunctions.GetTargetFromInputSource(data.InputSource);
        IObjectTransformer objectTransformer = FindObjectOfType<ObjectTransformer>().GetComponent<ObjectTransformer>();
        //string toolName = ActionHelperFunctions.GetVirtualToolFromPointer(data.InputSource.Pointers[0]).currentEntry.toolSettings.toolName;
        target = objectTransformer.TransformObject(target, "Delete");

        if (target != null)
        {
            DeleteAction deleteAction = new DeleteAction(target);
            ServiceManager.GetService<CommandStackService>().AddAndPerformAction(deleteAction);
        }
    }
}
