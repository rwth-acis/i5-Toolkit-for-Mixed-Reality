using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// A collection of helper functions for virtual tool actions
    /// </summary>
    public class ActionHelperFunctions : MonoBehaviour
    {
        /// <summary>
        /// Iterates upwards in the hirachy of gameObject and returns the first GameObject that has a script of type typeToSearch attached. Returns null if nothing is found.
        /// </summary>
        /// <param name="gameObject"></param> The gameObject on which the search should start
        /// <param name="typesToExclude"></param> Can be used as a filter. A GameObject can be ignored if it has a object contianed in typesToExclude above or below it
        /// <param name="checkAbove"></param> Check above in the hirachy for filterd types
        /// <param name="checkBelow"></param>Check below in the hirachy for filterd types
        /// <returns></returns>
        public static GameObject GetGameobjectOfTypeFromHirachy(GameObject gameObject, Type typeToSearch, Type[] typesToExclude = null, bool checkAbove = false, bool checkBelow = false)
        {
            //If wished, check if any of the children of the target is of a type that should be excluded
            if (typesToExclude != null && checkBelow)
            {
                foreach (Type type in typesToExclude)
                {
                    if (gameObject.GetComponentInChildren(type, true) != null)
                    {
                        return null;
                    }
                }
            }

            if (gameObject != null)
            {
                while (gameObject != null && gameObject.GetComponent(typeToSearch) == null)
                {
                    //If wished, check if the current object (i.e. a object above in the hirachy of the original target) is of a type that should be excluded
                    if (typesToExclude != null && checkAbove)
                    {
                        foreach (Type type in typesToExclude)
                        {
                            if (gameObject.GetComponent(type) != null)
                            {
                                return null;
                            }
                        }
                    }

                    gameObject = gameObject.transform.parent?.gameObject;
                }
                if (gameObject != null && gameObject.GetComponent(typeToSearch) != null)
                {
                    return gameObject;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the virtual tool that is attached to the pointer
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static ViveWandVirtualTool GetVirtualTool(IMixedRealityPointer pointer)
        {
            return pointer?.Controller?.Visualizer?.GameObjectProxy?.GetComponentInChildren<ViveWandVirtualTool>();
        }

        /// <summary>
        /// Get the virtual tool that is attached to the input source
        /// </summary>
        /// <param name="inputSource"></param>
        /// <returns></returns>
        public static ViveWandVirtualTool GetVirtualTool(IMixedRealityInputSource inputSource)
        {
            foreach (var pointer in inputSource.Pointers)
            {
                ViveWandVirtualTool tool = GetVirtualTool(pointer);
                if (tool != null)
                {
                    return tool;
                }
            }
            return null;
        }

        /// <summary>
        /// Rotates the object on the XZ plane towards the main camera and sets it to new position
        /// </summary>
        /// <param name="objectToRotate"></param> The object to move and rotate
        /// <param name="newPosition"></param> The new position
        public static void RotateToCameraOnXZPlane(GameObject objectToRotate, Vector3 newPosition)
        {
            objectToRotate.transform.LookAt(Camera.main.transform);
            //Set the x and y rotation to 0 and flip it around because with LookAt, the prefab faces away from the camera and is tilted strangly
            Vector3 rotation = objectToRotate.transform.eulerAngles;
            objectToRotate.transform.SetPositionAndRotation(newPosition, Quaternion.Euler(0, 180 + rotation.y, 0));
        }

        /// <summary>
        /// When the input source focuses a target, the target is returned. Otherwise null is returned.
        /// </summary>
        /// <param name="inputSource"></param> The input source
        /// <returns></returns>
        public static GameObject GetTargetFromInputSource(IMixedRealityInputSource inputSource)
        {
            foreach (var pointer in inputSource.Pointers)
            {
                GameObject target = pointer.Result?.CurrentPointerTarget;
                if (target != null)
                {
                    return target;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the name of the currently selected tool
        /// </summary>
        /// <param name="inputSource"></param>
        /// <returns></returns>
        public static string GetCurrentToolName(IMixedRealityInputSource inputSource)
        {
            return GetVirtualTool(inputSource)?.currentEntry.toolSettings.toolName;
        }
    } 
}
