using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public class ActionHelperFunctionsCore
    {
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
