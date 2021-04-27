using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.Toolkit.MixedReality.PieMenu
{
    public class ExampleActions : ActionHelperFunctions
    {
        public void TouchPadRightTest()
        {
            Debug.Log("Tochpad Right Pressed");
        }

        public void TouchPadUpTest()
        {
            Debug.Log("Tochpad Up Pressed");
        }

        public void TouchPadLeftTest()
        {
            Debug.Log("Tochpad Left Pressed");
        }

        public void TouchPadDownTest()
        {
            Debug.Log("Tochpad Down Pressed");
        }

        public void TriggerPressTest()
        {
            Debug.Log("Trigger Pressed");
        }

        public void GripPressTest()
        {
            Debug.Log("Grip Pressed");
        }
    }
}
