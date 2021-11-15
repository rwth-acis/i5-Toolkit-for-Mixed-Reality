using System;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// A collection of helper functions for virtual tool actions
    /// </summary>
    public class ActionHelperFunctions
    {
        /// <summary>
        /// Iterates upwards in the hirachy of gameObject and returns the first GameObject that has a script of type typeToSearch attached. Returns null if nothing is found.
        /// </summary>
        /// <param name="gameObject"></param> The gameObject on which the search should start
        /// <param name="typeToSearch"></param> The searched types
        /// <param name="typesToExclude"></param> Can be used as a filter. A GameObject can be ignored if it has a object contianed in typesToExclude above or below it
        /// <param name="checkAbove"></param> Check above in the hirachy for filterd types
        /// <param name="checkBelow"></param> Check below in the hirachy for filterd types
        /// <returns></returns>
        public static GameObject GetGameobjectOfTypeFromHirachy(GameObject gameObject, Type typeToSearch, Type[] typesToExclude = null, bool checkAbove = false, bool checkBelow = false)
        {
            ActionHelperFunctionsShell actionHelperFunctionsShell = new ActionHelperFunctionsShell();
            actionHelperFunctionsShell.gameObject = gameObject;

            ActionHelperFunctionsCore.GetGameobjectOfTypeFromHirachy(actionHelperFunctionsShell, typeToSearch, typesToExclude , checkAbove, checkBelow);

            return actionHelperFunctionsShell.gameObject;
        }

        /// <summary>
        /// Get the virtual tool that is attached to the pointer
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        public static ViveWandToolShell GetVirtualTool(IMixedRealityPointer pointer)
        {
            return pointer?.Controller?.Visualizer?.GameObjectProxy?.GetComponentInChildren<ViveWandToolShell>();
        }

        /// <summary>
        /// Get the virtual tool that is attached to the input source
        /// </summary>
        /// <param name="inputSource"></param>
        /// <returns></returns>
        public static ViveWandToolShell GetVirtualTool(IMixedRealityInputSource inputSource)
        {
            foreach (var pointer in inputSource.Pointers)
            {
                ViveWandToolShell tool = GetVirtualTool(pointer);
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
