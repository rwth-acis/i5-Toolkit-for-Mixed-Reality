using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FakeItEasy;

public class FakeGameObject
{
    public FakeGameObject parent;
    public List<FakeGameObject> childs;
    public Type type;

    public FakeGameObject()
    {
        childs = new List<FakeGameObject>();
    }

    public FakeGameObject GetComponentInChildren(Type type)
    {
        if (this.type == type)
        {
            return this;
        }
        else
        {
            foreach (var child in childs)
            {
                var result = child.GetComponentInChildren(type);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}

public class FakeActionHelperFunctionsShell : IActionHelperFunctionsShell
{
    public FakeGameObject fakeGameObject;

    public bool GameObjectsHasComponentOfType(Type type)
    {
        return fakeGameObject.GetComponentInChildren(type) != null;
    }

    public bool GameObjectIsOfType(Type type)
    {
        return fakeGameObject.type == type;
    }

    public bool GameObectIsNull()
    {
        return fakeGameObject == null;
    }

    public void GoToParentOfGameObject()
    {
        fakeGameObject = fakeGameObject.parent;
    }

    public void SetGameObjectNull()
    {
        fakeGameObject = null;
    }
}

public class ActionHelperFunctionsTests
{
    //GetGameobjectOfTypeFromHirachy tests

    [Test]
    public void Search_for_existing_object_without_filter()
    {
        //Can't actually be faked using FakeItEasy, because the type of proxy UnityEngine.GameObject is sealed
        FakeGameObject entryObject = new FakeGameObject();

        FakeActionHelperFunctionsShell shell = new FakeActionHelperFunctionsShell();

        Type typeToSearch = typeof(MonoBehaviour);

        //Fake the hirachy

        FakeGameObject currentObject = entryObject;
        FakeGameObject searchedNode;

        //Layer 2
        FakeGameObject parent = new FakeGameObject();
        currentObject.parent = parent;
        FakeGameObject sibling = new FakeGameObject();
        sibling.parent = parent;
        parent.childs = new List<FakeGameObject>{sibling, currentObject };

        currentObject = parent;

        //Layer 1
        parent = new FakeGameObject();
        parent.type = typeof(MonoBehaviour); //The searched node
        searchedNode = parent;
        currentObject.parent = parent;
        sibling = new FakeGameObject();
        sibling.parent = parent;
        parent.childs = new List<FakeGameObject> { sibling, currentObject };

        currentObject = parent;

        //Layer 0
        parent = new FakeGameObject();
        currentObject.parent = parent;
        parent.childs = new List<FakeGameObject> {currentObject};

        shell.fakeGameObject = entryObject;

        ActionHelperFunctionsCore.GetGameobjectOfTypeFromHirachy(shell, typeToSearch);
        Assert.AreEqual(shell.fakeGameObject, searchedNode);
    }
}
