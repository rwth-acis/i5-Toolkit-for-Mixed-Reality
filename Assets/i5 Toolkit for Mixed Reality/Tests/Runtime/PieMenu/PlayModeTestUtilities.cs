using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.MixedReality.PieMenu;
using Microsoft.MixedReality.Toolkit;
using FakeItEasy;

/// <summary>
/// Theroratically inckuded in the i5 toolkit. Only included here as workaround.
/// </summary>
public static class PlayModeTestUtilities
{
    public static void LoadEmptyTestScene()
    {
        CheckPlayMode();
        Scene playScene = SceneManager.CreateScene("PlayTest Scene");
        SceneManager.SetActiveScene(playScene);
    }

    public static void UnloadTestScene()
    {
        CheckPlayMode();
        SceneManager.UnloadScene("PlayTest Scene");
    }

    private static void CheckPlayMode()
    {
        if (!Application.isPlaying)
        {
            throw new InvalidPlatformException("Play Mode Test Utilities can only be used in PlayMode");
        }
    }

    public static InputEventData GetFakeInputEventData(MixedRealityInputAction inputAction, Vector3 controllerPosition)
    {
        return GetFakeInputEventData(inputAction, GetFakeController(controllerPosition));
    }

    public static InputEventData GetFakeInputEventData(MixedRealityInputAction inputAction, ViveWandVirtualTool viveWand)
    {
        InputEventData data = new InputEventData(UnityEngine.EventSystems.EventSystem.current);
        data.Initialize(viveWand.ownSource, Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right, inputAction);
        return data;
    }

    public static ViveWandVirtualTool GetFakeController(Vector3 position)
    {
        IMixedRealityInputSource inputSource = GetFakeInputSource(position);
        var controller = GameObject.FindObjectOfType<ViveWandVirtualTool>();
        controller.ownSource = inputSource;
        return controller;
    }

    public static IMixedRealityInputSource GetFakeInputSource(Vector3 position)
    {
        var inputSource = A.Fake<IMixedRealityInputSource>();

        var pointer = A.Fake<IMixedRealityPointer>();
        A.CallTo(() => pointer.Position).Returns(position);

        var pointerArray = new IMixedRealityPointer[1];
        pointerArray[0] = pointer;

        A.CallTo(() => inputSource.Pointers).Returns(pointerArray);
        return inputSource;
    }
}

public class InvalidPlatformException : Exception
{
    public InvalidPlatformException()
    {
    }

    public InvalidPlatformException(string message) : base(message)
    {
    }

    public InvalidPlatformException(string message, Exception inner) : base(message, inner)
    {
    }
}
