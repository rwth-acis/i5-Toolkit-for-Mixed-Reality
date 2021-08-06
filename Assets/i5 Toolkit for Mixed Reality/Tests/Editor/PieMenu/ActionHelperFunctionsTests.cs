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
    FakeActionHelperFunctionsShell shell;
    FakeGameObject entryObject;

    [SetUp]
    public void Setup()
    {
        shell = new FakeActionHelperFunctionsShell();
        entryObject = new FakeGameObject();

        //Fake the hirachy. Can't be faked using FakeItEasy, because the type of proxy UnityEngine.GameObject is sealed
        /*
         O
         |
         O (MonoBehaiviour)
         |____
         |    |
         O    O
              |____
              |    |
              O    O (entryPoint)
         */


        FakeGameObject currentObject = entryObject;
        FakeGameObject searchedNode;

        //Layer 2
        FakeGameObject parent = new FakeGameObject();
        currentObject.parent = parent;
        FakeGameObject sibling = new FakeGameObject();
        sibling.parent = parent;
        parent.childs = new List<FakeGameObject> { sibling, currentObject };

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
        parent.childs = new List<FakeGameObject> { currentObject };

        shell.fakeGameObject = entryObject;
    }

    [Test]
    public void Search_for_existing_object_without_filter()
    {
        Type typeToSearch = typeof(MonoBehaviour);

        FakeGameObject searchedNode = entryObject.parent.parent;

        ActionHelperFunctionsCore.GetGameobjectOfTypeFromHirachy(shell, typeToSearch);
        Assert.AreEqual(shell.fakeGameObject, searchedNode);
    }
}
