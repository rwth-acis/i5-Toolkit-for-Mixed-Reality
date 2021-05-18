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
        IObjectTransformer objectTransformer = FindObjectOfType<ObjectTransformer>().GetComponent<ObjectTransformer>();
        GameObject target = ActionHelperFunctions.GetTargetFromInputSource(data.InputSource);
        //string toolName = ActionHelperFunctions.GetVirtualToolFromPointer(data.InputSource.Pointers[0]).currentEntry.toolSettings.toolName;
        target = objectTransformer.transformObject(target, "Delete");

        if (target != null)
        {
            ServiceManager.GetService<CommandStackService>().AddAndPerformAction(new DeleteAction(target));
        }
    }
}
