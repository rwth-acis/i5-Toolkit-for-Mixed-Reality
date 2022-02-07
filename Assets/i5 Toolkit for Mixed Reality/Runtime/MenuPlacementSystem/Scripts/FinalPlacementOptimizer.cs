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

        private Camera head;

        private Vector3 positionOffset = Vector3.zero;
        private Vector3 rotationOffset = Vector3.zero;
        private Vector3 scaleOffset = Vector3.one;

        //ConstantViewSize properties:
        private float objectSize = 1f;
        private float targetViewPercentV = 0.5f;
        private float minScale = 0.01f;
        private float maxScale = 100f;

        /// <summary>
        /// Returns the scale to be applied based on the FOV. This scale will be multiplied by distance as part of
        /// the final scale calculation, so this is the ratio of vertical fov to distance.
        /// </summary>
        public float FovScale
        {
            get
            {
                //float cameraFovRadians = (CameraCache.Main.aspect * CameraCache.Main.fieldOfView) * Mathf.Deg2Rad;
                float cameraFovRadians = CameraCache.Main.fieldOfView * Mathf.Deg2Rad;
                float sinFov = Mathf.Sin(cameraFovRadians * 0.5f);
                return 2f * targetViewPercentV * sinFov / objectSize;
            }
        }

        public float TargetViewPercentV
        {
            get => targetViewPercentV;
            set
            {
                targetViewPercentV = value;
            }
        }

        public float MinScale
        {
            get => minScale;
            set
            {
                minScale = value;
            }
        }

        public float MaxScale
        {
            get => maxScale;
            set
            {
                maxScale = value;
            }
        }

        /// <summary>
        /// How should the menu be oriented
        /// </summary>
        public OrientationType OrientationType { get; set; }
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

        protected override void Start() {
            base.Start();
            RecalculateBounds();
            head = CameraCache.Main;
        }

        /// <summary>
        /// Fine-tune the transform of the menu based on several offsets.
        /// </summary>
        public override void SolverUpdate() {
            UpdatePosition();
            UpdateOrientation();
            UpdateScale();
        }

        //Compute the final position based on menu variant and solvers
        private void UpdatePosition() {
            if (gameObject.GetComponent<MenuHandler>().MenuType == MenuType.MainMenu) {
                if (gameObject.GetComponent<MenuHandler>().VariantType == VariantType.Compact) {
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
                        Vector3 finalOffset = right * orbitalOffset.x + up * orbitalOffset.y + forward * orbitalOffset.z + head.transform.right * positionOffset.x + head.transform.up * positionOffset.y + head.transform.forward * positionOffset.z;
                        GoalPosition += finalOffset;
                    }
                    else {
                        Vector3 finalOffset = -right * orbitalOffset.x + up * orbitalOffset.y + forward * orbitalOffset.z - head.transform.right * positionOffset.x + head.transform.up * positionOffset.y + head.transform.forward * positionOffset.z;
                        GoalPosition += finalOffset;
                    }
                }
                else {
                    GoalPosition += head.transform.right * positionOffset.x + head.transform.up * positionOffset.y + head.transform.forward * positionOffset.z;
                }
            }
        }
        
        //Compute the final orientation based on the MenuOrientationType
        private void UpdateOrientation() {            
            switch (gameObject.GetComponent<MenuHandler>().OrientationType) {
                case OrientationType.CameraAligned:
                    GoalRotation = head.transform.rotation;
                    break;
                case OrientationType.Unmodified:
                    GoalRotation = GoalRotation;
                    break;
                case OrientationType.CameraFacing:
                    GoalRotation = Quaternion.LookRotation(head.transform.position - transform.position);
                    break;
                case OrientationType.CameraFacingReverse:
                    GoalRotation = Quaternion.LookRotation(transform.position - head.transform.position);
                    break;
                case OrientationType.YawOnly:
                    GoalRotation = Quaternion.Euler(0f, head.transform.rotation.eulerAngles.y, 0f);
                    break;
                case OrientationType.FollowTargetObject:
                    if(gameObject.GetComponent<MenuHandler>().MenuType == MenuType.MainMenu) {
                        GoalRotation = head.transform.rotation;
                    }
                    else {
                        GoalRotation = gameObject.GetComponent<MenuHandler>().TargetObject.transform.rotation;
                    }
                    break;
                default:
                    GoalRotation = GoalRotation;
                    break;
            }
            GoalRotation = Quaternion.Euler(GoalRotation.eulerAngles + rotationOffset);
        }

        //Compute the final scale based on the offsets or ConstantViewSize
        private void UpdateScale() {
            if (gameObject.GetComponent<MenuHandler>().ConstantViewSizeEnabled) {
                //Partially borrowed and simplified from ConstantViewSize solver in MRTK under MIT License

                // Set the linked alt scale ahead of our work. This is an attempt to minimize jittering by having solvers work with an interpolated scale.
                SolverHandler.AltScale.SetGoal(transform.localScale);

                // Calculate scale based on distance from view.  Do not interpolate so we can appear at a constant size if possible.  Borrowed from greybox.
                Vector3 targetPosition = head.transform.position;
                float distance = Vector3.Distance(transform.position, targetPosition);
                float scale = Mathf.Clamp(FovScale * distance, minScale, maxScale);
                Vector3 originalRatio = new Vector3(OriginalScale.x / OriginalScale.y, 1, OriginalScale.z / OriginalScale.y);
                GoalScale = originalRatio * scale;
            }
            else {
                GoalScale = new Vector3(OriginalScale.x * ScaleOffset.x, OriginalScale.y * ScaleOffset.y, OriginalScale.z * ScaleOffset.z);
            }
        }
        

        /// <summary>
        /// Attempts to calculate the size of the bounds which contains all child renderers for attached GameObject. This information is used in the core solver calculations
        /// Borrowed from ConstantViewSize but modified
        /// </summary>
        private void RecalculateBounds() {
            float baseSize;
            Vector3 cachedScale = transform.root.localScale;
            transform.root.localScale = Vector3.one;

            var combinedBounds = new Bounds(transform.position, Vector3.zero);
            var renderers = GetComponentsInChildren<Renderer>();

            for (var i = 0; i < renderers.Length; i++) {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            //baseSize = combinedBounds.extents.magnitude;
            baseSize = combinedBounds.max.y - combinedBounds.min.y;
            transform.root.localScale = cachedScale;
            
            if (baseSize > 0) {
                objectSize = baseSize;
            }
            else {
                Debug.LogWarning("ConstantViewSize: Object base size calculate was 0, defaulting to 1");
                objectSize = 1f;
            }
        }
    }
}
