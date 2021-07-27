using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionHelperFunctionsShell
{
    GameObject gameObject { get; set; }
    bool GameObjectsHasComponentOfType(Type type);
    bool GameObectIsNull();
    void GoToParentOfGameObject();
    void SetGameObjectNull();
}
