using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.MixedReality.PieMenu;

public class ObjectTransformer : MonoBehaviour, IObjectTransformer
{
    GameObject IObjectTransformer.transformObject(GameObject objectToTransform, string toolName)
    {
        switch (toolName)
        {
            case "Delete":
                return ActionHelperFunctions.GetGameobjectOfTypeFromHirachy(objectToTransform, typeof(SampleObject));
            default:
                return null;

        }
    }
}
