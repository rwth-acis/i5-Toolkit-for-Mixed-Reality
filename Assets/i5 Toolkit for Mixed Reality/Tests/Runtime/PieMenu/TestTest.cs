using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using FakeItEasy;
using UnityEngine.TestTools;
using i5.Toolkit.Core.TestHelpers;

public class TestTest
{
    [SetUp] //NUnit
    public void LoadScene()
    {
        A.Fake<>
        Debug.Log("Setup");
    }

    [TearDown] //NUinit
    public void TearDownScene()
    {
        Debug.Log("Teardown");
    }

    [UnityTest] //TestTools
    public IEnumerator Runner_RunnerGameObjectDestroyed_CreatesNewRunner()
    {
        Debug.Log("Test");
        yield return null;
        Assert.True(true);
    }
}
