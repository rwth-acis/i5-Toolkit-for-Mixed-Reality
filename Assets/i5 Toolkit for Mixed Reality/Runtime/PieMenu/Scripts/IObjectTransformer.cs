using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public interface IObjectTransformer
    {
        GameObject transformObject(GameObject objectToTransform, string toolName);
    }
}
