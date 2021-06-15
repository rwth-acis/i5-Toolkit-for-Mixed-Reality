using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit.Input;

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

    public static BaseInputEventData FakeBaseInputEvent()
    {
        return null;
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
