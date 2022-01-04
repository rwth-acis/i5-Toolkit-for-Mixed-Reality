using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using UnityEngine;

/// <summary>
/// This solver should only be used for the Menu Placement System and it fine-tunes the transform of the menu based on several offsets.
/// Be sure to order this at the lowest position of all solvers except ConstantViewSize.
/// Its modification of scales will be overwritten by ConstantViewSize, if enabled.
/// </summary>

namespace i5.Toolkit.MixedReality.MenuPlacementSystem {
    public class FinalPlacementOptimizer : Solver {
        [Tooltip("XYZ offset for this object oriented with the TrackedObject/TargetTransform's forward. X offset will be minus if this object should be placed on the left side of the TrackedObject/TargetTransform")]
        [SerializeField] private Vector3 orbitalOffset = Vector3.zero;

        private Vector3 positionOffset = Vector3.zero;
        private Vector3 rotationOffset = Vector3.zero;
        private Vector3 scaleOffset = Vector3.one;


        /// <summary>
        /// How should the menu be oriented
        /// </summary>
        public MenuHandler.MenuOrientationType OrientationType { get; set; }
        /// <summary>
        /// local scale of the target object
        /// </summary>
        public Vector3 OriginalScale { get; set; }
        /// <summary>
        /// the orbital offset set in the inspector of the menu
        /// </summary>
        public Vector3 OrbitalOffset
        {
            get { return orbitalOffset; }
            set { orbitalOffset = value; }
        }

        /// <summary>
        /// position offset set by manipulation
        /// </summary>
        public Vector3 PositionOffset
        {
            get => positionOffset;
            set { positionOffset = value; }
        }

        /// <summary>
        /// rotation offset set by manipulation
        /// </summary>
        public Vector3 RotationOffset
        {
            get => rotationOffset;
            set { rotationOffset = value; }
        }

        /// <summary>
        /// scale offset set by manpulation
        /// </summary>
        public Vector3 ScaleOffset
        {
            get => scaleOffset;
            set { scaleOffset = value; }
        }

        /// <summary>
        /// Fine-tune the transform of the menu based on several offsets.
        /// </summary>
        public override void SolverUpdate() {
            Camera head = CameraCache.Main;           
            if (gameObject.GetComponent<MenuHandler>().menuVariantType == MenuHandler.MenuVariantType.MainMenu) {
                if (gameObject.GetComponent<MenuHandler>().compact) {
                    GoalPosition += head.transform.right * positionOffset.x + head.transform.up * positionOffset.y + head.transform.forward * positionOffset.z;
                }
            }
            else {
                if (gameObject.GetComponent<Orbital>().enabled) {
                    Vector3 headToObject = Vector3.Normalize(SolverHandler.TransformTarget.position - head.transform.position);
                    Vector3 forward = Vector3.Normalize(new Vector3(headToObject.x, 0, headToObject.z));
                    Vector3 right = Vector3.Normalize(Vector3.ProjectOnPlane(new Vector3(head.transform.right.x, 0, head.transform.right.z), headToObject));
                    Vector3 up = Vector3.up;
                    //If we look at the object's LEFT side, than the angle between head.transform.right and headToObject is an acute angle, i.e. Vector3.Dot(headToObject, head.transform.right) > 0.
                    //If we look at the object's RIGHT side, than the angle between head.transform.right and headToObject is an obtuse angle, i.e. Vector3.Dot(headToObject, head.transform.right) < 0. 
                    bool rightSide = Vector3.Dot(headToObject, head.transform.right) < 0 ? true : false;
                    if (rightSide) {
                        Vector3 finalOffset = right * orbitalOffset.x + up * orbitalOffset.y + forward * orbitalOffset.z + head.transform.right * positionOffset.x + head.transform.up * positionOffset.y + head.transform.forward*positionOffset.z;
                        GoalPosition += finalOffset;
                    }
                    else {
                        Vector3 finalOffset = - right * orbitalOffset.x + up * orbitalOffset.y + forward * orbitalOffset.z - head.transform.right * positionOffset.x + head.transform.up * positionOffset.y + head.transform.forward * positionOffset.z;
                        GoalPosition += finalOffset;
                    }
                }
                else {
                    GoalPosition += head.transform.right * positionOffset.x + head.transform.up * positionOffset.y + head.transform.forward * positionOffset.z;
                }
            }
            switch (gameObject.GetComponent<MenuHandler>().menuOrientationType) {
                case MenuHandler.MenuOrientationType.CameraAligned:
                    GoalRotation = Quaternion.Euler(head.transform.rotation.eulerAngles + RotationOffset);
                    break;
                case MenuHandler.MenuOrientationType.Unmodified:
                    GoalRotation = GoalRotation;
                    break;
            }


            GoalScale = new Vector3(OriginalScale.x * ScaleOffset.x, OriginalScale.y * ScaleOffset.y, OriginalScale.z * ScaleOffset.z);
            
        }
    }
}
