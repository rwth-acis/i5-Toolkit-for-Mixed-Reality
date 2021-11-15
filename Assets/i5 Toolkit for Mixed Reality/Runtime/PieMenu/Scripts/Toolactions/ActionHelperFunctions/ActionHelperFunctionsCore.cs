using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// Implements the functionality for the ActionHelperFunctions. Needs a ActionHelperFunctionShell to be connected with the Unity environment.
    /// </summary>
    public class ActionHelperFunctionsCore
    {
        /// <summary>
        /// Iterates upwards in the hirachy of gameObject and returns the first GameObject that has a script of type typeToSearch attached. Returns null if nothing is found.
        /// </summary>
        /// <param name="typeToSearch"></param> The searched types
        /// <param name="typesToExclude"></param> Can be used as a filter. A GameObject can be ignored if it has a object contianed in typesToExclude above or below it
        /// <param name="checkAbove"></param> Check above in the hirachy for filterd types
        /// <param name="checkBelow"></param>Check below in the hirachy for filterd types
        /// <returns></returns>
        public static void GetGameobjectOfTypeFromHirachy(IActionHelperFunctionsShell shell, Type typeToSearch, Type[] typesToExclude = null, bool checkAbove = false, bool checkBelow = false)
        {
            //If wished, check if any of the children of the target is of a type that should be excluded
            if (typesToExclude != null && checkBelow)
            {
                foreach (Type type in typesToExclude)
                {
                    if (shell.GameObjectsHasComponentOfType(type))
                    {
                        shell.SetGameObjectNull();
                        return;
                    }
                }
            }

            if (!shell.GameObectIsNull())
            {
                while (!shell.GameObectIsNull() && !shell.GameObjectIsOfType(typeToSearch))
                {
                    //If wished, check if the current object (i.e. a object above in the hirachy of the original target) is of a type that should be excluded
                    if (typesToExclude != null && checkAbove)
                    {
                        foreach (Type type in typesToExclude)
                        {
                            if (shell.GameObjectIsOfType(type))
                            {
                                shell.SetGameObjectNull();
                                return;
                            }
                        }
                    }

                    shell.GoToParentOfGameObject();
                }
                if (!shell.GameObectIsNull() && shell.GameObjectIsOfType(typeToSearch))
                {
                    return;
                }
            }
        }
    }
}
