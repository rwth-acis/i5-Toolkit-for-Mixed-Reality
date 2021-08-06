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

    public void addChilds(int number)
    {
        for (int i = 0; i < number; i++)
        {
            FakeGameObject child = new FakeGameObject();
            child.parent = this;
            childs.Add(child);
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
    FakeGameObject searchedNode;
    Type typeToSearch = typeof(int);

    [SetUp]
    public void Setup()
    {
        shell = new FakeActionHelperFunctionsShell();


        //Fake the hirachy. Can't be faked using FakeItEasy, because the type of proxy UnityEngine.GameObject is sealed
        /*
         O
         |
         O (int)
         |____
         |    |
         O    O (string)
              |____
              |    |
              O    O (entryPoint)
                   |_________________
                   |   |             |
                   O   O (string)    O
         */


        //Layer 0
        FakeGameObject currentNode = new FakeGameObject();
        currentNode.addChilds(1);

        //Layer 1
        currentNode = currentNode.childs[0];
        currentNode.type = typeof(int);
        currentNode.addChilds(2);
        searchedNode = currentNode;

        //Layer 3
        currentNode = currentNode.childs[1];
        currentNode.type = typeof(string);
        currentNode.addChilds(2);

        //Layer 4
        currentNode = currentNode.childs[1];
        entryObject = currentNode;
        currentNode.addChilds(3);

        //Layer 5
        currentNode = currentNode.childs[1];
        currentNode.type = typeof(string);

        shell.fakeGameObject = entryObject;
    }



    [Test]
    public void Search_for_existing_object_without_filter()
    {
        ActionHelperFunctionsCore.GetGameobjectOfTypeFromHirachy(shell, typeToSearch);
        Assert.AreEqual(shell.fakeGameObject, searchedNode);
    }

    [Test]
    public void Search_for_existing_object_with_above_filter()
    {
        Type[] typesToExclude = new Type[]{ typeof(string) };
        ActionHelperFunctionsCore.GetGameobjectOfTypeFromHirachy(shell, typeToSearch, typesToExclude, true);
        Assert.IsNull(shell.fakeGameObject);
    }

    [Test]
    public void Search_for_existing_object_with_below_filter()
    {
        Type[] typesToExclude = new Type[] { typeof(string) };
        ActionHelperFunctionsCore.GetGameobjectOfTypeFromHirachy(shell, typeToSearch, typesToExclude, false, true);
        Assert.IsNull(shell.fakeGameObject);
    }
}
