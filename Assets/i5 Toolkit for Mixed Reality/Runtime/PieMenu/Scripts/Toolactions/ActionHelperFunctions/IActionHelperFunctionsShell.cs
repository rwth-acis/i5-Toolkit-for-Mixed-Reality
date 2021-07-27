using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionHelperFunctionsShell
{
    bool GameObjectsHasComponentOfType(Type type);
    bool GameObjectIsOfType(Type type);
    bool GameObectIsNull();
    void GoToParentOfGameObject();
    void SetGameObjectNull();
}
