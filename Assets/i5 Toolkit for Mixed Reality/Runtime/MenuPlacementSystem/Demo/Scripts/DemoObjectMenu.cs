using i5.Toolkit.MixedReality.MenuPlacementSystem;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class DemoObjectMenu : MenuBase {

    private ObjectManipulator manipulator;
    private GameObject targetObject;


    public void AllowMoveOperation() {
        if (targetObject.GetComponent<MoveAxisConstraint>().enabled == true) {
            targetObject.GetComponent<MinMaxScaleConstraint>().enabled = true;
            targetObject.GetComponent<RotationAxisConstraint>().enabled = true;
            targetObject.GetComponent<MoveAxisConstraint>().enabled = false;
            manipulator.ManipulationType = ManipulationHandFlags.OneHanded | ManipulationHandFlags.TwoHanded;
        }
    }



    public void AllowRotateOperation() {
        if (targetObject.GetComponent<RotationAxisConstraint>().enabled == true) {
            targetObject.GetComponent<MinMaxScaleConstraint>().enabled = true;
            targetObject.GetComponent<MoveAxisConstraint>().enabled = true;
            targetObject.GetComponent<RotationAxisConstraint>().enabled = false;
            manipulator.ManipulationType = ManipulationHandFlags.TwoHanded | ManipulationHandFlags.OneHanded;
            manipulator.OneHandRotationModeFar = ObjectManipulator.RotateInOneHandType.RotateAboutObjectCenter;
            manipulator.OneHandRotationModeNear = ObjectManipulator.RotateInOneHandType.RotateAboutObjectCenter;
            manipulator.TwoHandedManipulationType = TransformFlags.Rotate;
        }
    }

    public void AllowScaleOperation() {
        if (targetObject.GetComponent<MinMaxScaleConstraint>().enabled == true) {
            targetObject.GetComponent<RotationAxisConstraint>().enabled = true;
            targetObject.GetComponent<MoveAxisConstraint>().enabled = true;
            targetObject.GetComponent<MinMaxScaleConstraint>().enabled = false;
            manipulator.ManipulationType = ManipulationHandFlags.TwoHanded;
            manipulator.TwoHandedManipulationType = TransformFlags.Scale;
        }

    }


    public override void Initialize() {
        targetObject = GetComponent<MenuHandler>().TargetObject;
        manipulator = targetObject.GetComponent<ObjectManipulator>();
    }

    public override void OnClose() {
        targetObject = GetComponent<MenuHandler>().TargetObject;
        manipulator = targetObject.GetComponent<ObjectManipulator>();
        targetObject.GetComponent<PointerHandler>().enabled = true;
        targetObject.GetComponent<MinMaxScaleConstraint>().enabled = true;
        targetObject.GetComponent<MoveAxisConstraint>().enabled = true;
        targetObject.GetComponent<RotationAxisConstraint>().enabled = true;
        manipulator.ManipulationType = 0;
        manipulator.TwoHandedManipulationType = 0;
    }
}
