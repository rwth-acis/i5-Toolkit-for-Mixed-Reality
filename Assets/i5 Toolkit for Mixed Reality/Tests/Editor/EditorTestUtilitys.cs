using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.MixedReality.PieMenu;
using Microsoft.MixedReality.Toolkit;
using FakeItEasy;

namespace i5.Toolkit.MixedReality.Tests
{

    public static class EditorTestUtilitys
    {
        public static InputEventData GetFakeInputEventData(MixedRealityInputAction inputAction, Vector3 controllerPosition)
        {
            return GetFakeInputEventData(inputAction, GetFakeInputSource(controllerPosition));
        }

        public static InputEventData GetFakeInputEventData(MixedRealityInputAction inputAction, IMixedRealityInputSource inputSource)
        {
            InputEventData data = new InputEventData(UnityEngine.EventSystems.EventSystem.current);
            data.Initialize(inputSource, Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right, inputAction);
            return data;
        }

        //public static IMixedRealityInputSource GetFakeController(Vector3 position)
        //{
        //    IMixedRealityInputSource inputSource = GetFakeInputSource(position);
        //    var controller = GameObject.FindObjectOfType<ViveWandVirtualTool>();
        //    controller.ownSource = inputSource;
        //    return controller;
        //}

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
}
