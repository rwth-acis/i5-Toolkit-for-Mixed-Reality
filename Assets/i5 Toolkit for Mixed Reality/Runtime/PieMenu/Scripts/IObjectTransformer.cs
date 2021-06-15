using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// Interface for creating a component, that checks if and which parts of an object can be affected by th ecurrent tool
    /// </summary>
    public interface IObjectTransformer
    {
        /// <summary>
        /// Returns null if the object can't be effected by the tool named toolName. Otherwise, returns which object in the hirachy can be effected by it.
        /// </summary>
        /// <param name="objectToTransform"></param>
        /// <param name="toolName"></param>
        /// <returns></returns>
        GameObject TransformObject(GameObject objectToTransform, string toolName);
    }
}
