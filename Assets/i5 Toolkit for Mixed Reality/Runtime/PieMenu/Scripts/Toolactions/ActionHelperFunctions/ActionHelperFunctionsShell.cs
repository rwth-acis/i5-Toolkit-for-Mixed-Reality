﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// The needed callback functions for the ActionHelperFunctions core.
    /// </summary>
    public class ActionHelperFunctionsShell : IActionHelperFunctionsShell
    {
        public GameObject gameObject { get; set; }

        public bool GameObjectsHasComponentOfType(Type type)
        {
            return gameObject.GetComponentInChildren(type, true) != null;
        }

        public bool GameObjectIsOfType(Type type)
        {
            return gameObject.GetComponent(type) != null;
        }

        public bool GameObectIsNull()
        {
            return gameObject == null;
        }

        public void GoToParentOfGameObject()
        {
            gameObject = gameObject.transform.parent?.gameObject;
        }

        public void SetGameObjectNull()
        {
            gameObject = null;
        }
    }
}
