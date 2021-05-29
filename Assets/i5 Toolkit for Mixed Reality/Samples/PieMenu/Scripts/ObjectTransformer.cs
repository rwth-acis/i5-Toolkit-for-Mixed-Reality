using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.MixedReality.PieMenu;

public class ObjectTransformer : MonoBehaviour, IObjectTransformer
{
    GameObject IObjectTransformer.transformObject(GameObject objectToTransform, string toolName)
    {
        GameObject transformed = ActionHelperFunctions.GetGameobjectOfTypeFromHirachy(objectToTransform,
                                                                                      typeof(ManipulationInformation));
        if (transformed != null)
        {
            ManipulationInformation information = transformed.GetComponent<ManipulationInformation>();
            switch (toolName)
            {
                case "Delete":
                    if (information.deletePossible)
                    {
                        return transformed;
                    }
                    break;
            }
        }
        return null;
    }
}
