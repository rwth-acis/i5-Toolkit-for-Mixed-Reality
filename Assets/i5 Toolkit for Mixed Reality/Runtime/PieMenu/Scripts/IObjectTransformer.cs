using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectTransformer
{
    GameObject transformObject(GameObject objectToTransform, string toolName);
}
