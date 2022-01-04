using UnityEditor;
using UnityEngine;

namespace i5.Toolkit.MixedReality.MenuPlacementSystem {
    [CustomEditor(typeof(MenuHandler))]
    [CanEditMultipleObjects]
    public class MenuHandlerInspector : Editor {

        private SerializedProperty menuVariantType;
        private SerializedProperty compact;
        private SerializedProperty menuID;

        //General Properties
        private SerializedProperty inactivityDetectionEnabled;
        private SerializedProperty manipulationEnabled;
        private SerializedProperty boundingBoxType;
        private SerializedProperty menuOrientationType;
        private SerializedProperty constantViewSizeEnabled;
        private SerializedProperty dominantHand;

        //Thresholds
        private SerializedProperty updateTimeInterval;
        private SerializedProperty inactivityTimeThreshold;
        private SerializedProperty suggestionTimeInterval;
        private SerializedProperty retrieveBufferSize;

        //Global Offsets
        private SerializedProperty maxFloatingDistance;
        private SerializedProperty minFloatingDistance;
        private SerializedProperty defaultFloatingDistance;

        //ConstantViewSize Offsets
        private SerializedProperty defaultTargetViewPercentV;

        //Main Menu Offsets
        private SerializedProperty followOffset;
        private SerializedProperty followMaxViewHorizontalDegrees;
        private SerializedProperty followMaxViewVerticalDegrees;
        private SerializedProperty surfaceMagnetismSafetyOffset;

        //Object Menu Offsets
        private SerializedProperty orbitalOffset;
        private SerializedProperty manipulationLogic;

        bool constantViewSizeEnabledFoldout = true;
        bool mainMenuOffsetFoldout = true;
        bool objectMenuOffsetFoldout = true;

        public void OnEnable() {
            menuVariantType = serializedObject.FindProperty("menuVariantType");
            compact = serializedObject.FindProperty("compact");
            menuID = serializedObject.FindProperty("menuID");

            inactivityDetectionEnabled = serializedObject.FindProperty("inactivityDetectionEnabled");
            manipulationEnabled = serializedObject.FindProperty("manipulationEnabled");
            boundingBoxType = serializedObject.FindProperty("boundingBoxType");
            menuOrientationType = serializedObject.FindProperty("menuOrientationType");
            constantViewSizeEnabled = serializedObject.FindProperty("constantViewSizeEnabled");
            dominantHand = serializedObject.FindProperty("dominantHand");

            updateTimeInterval = serializedObject.FindProperty("updateTimeInterval");
            inactivityTimeThreshold = serializedObject.FindProperty("inactivityTimeThreshold");
            suggestionTimeInterval = serializedObject.FindProperty("suggestionTimeInterval");
            retrieveBufferSize = serializedObject.FindProperty("retrieveBufferSize");

            maxFloatingDistance = serializedObject.FindProperty("maxFloatingDistance");
            minFloatingDistance = serializedObject.FindProperty("minFloatingDistance");
            defaultFloatingDistance = serializedObject.FindProperty("defaultFloatingDistance");

            defaultTargetViewPercentV = serializedObject.FindProperty("defaultTargetViewPercentV");

            followOffset = serializedObject.FindProperty("followOffset");
            followMaxViewHorizontalDegrees = serializedObject.FindProperty("followMaxViewHorizontalDegrees");
            followMaxViewVerticalDegrees = serializedObject.FindProperty("followMaxViewVerticalDegrees");
            surfaceMagnetismSafetyOffset = serializedObject.FindProperty("surfaceMagnetismSafetyOffset");

            orbitalOffset = serializedObject.FindProperty("orbitalOffset");
            manipulationLogic = serializedObject.FindProperty("manipulationLogic");

        }

        public override void OnInspectorGUI() {

            EditorGUILayout.PropertyField(menuVariantType);
            EditorGUILayout.PropertyField(compact);
            EditorGUILayout.PropertyField(menuID);

            EditorGUILayout.PropertyField(inactivityDetectionEnabled);
            EditorGUILayout.PropertyField(manipulationEnabled);
            EditorGUILayout.PropertyField(constantViewSizeEnabled);
            EditorGUILayout.PropertyField(boundingBoxType);
            EditorGUILayout.PropertyField(menuOrientationType);
            EditorGUILayout.PropertyField(dominantHand);

            EditorGUILayout.PropertyField(updateTimeInterval);
            EditorGUILayout.PropertyField(inactivityTimeThreshold);
            EditorGUILayout.PropertyField(suggestionTimeInterval);
            EditorGUILayout.PropertyField(retrieveBufferSize);

            EditorGUILayout.PropertyField(maxFloatingDistance);
            EditorGUILayout.PropertyField(minFloatingDistance);
            EditorGUILayout.PropertyField(defaultFloatingDistance);

            EditorGUILayout.Space();

            GUIStyle style = EditorStyles.foldout;
            FontStyle previousStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;

            constantViewSizeEnabledFoldout = EditorGUILayout.Foldout(constantViewSizeEnabledFoldout, "ConstantViewSize Settings", true);
            if (constantViewSizeEnabledFoldout) {
                if (constantViewSizeEnabled.boolValue == true) {
                    EditorGUILayout.PropertyField(defaultTargetViewPercentV);
                }
                else {
                    EditorGUILayout.HelpBox("The ConstantViewSize solver is currently not enabled.", MessageType.Info);
                }
            }

            EditorGUILayout.Space();

            mainMenuOffsetFoldout = EditorGUILayout.Foldout(mainMenuOffsetFoldout, "Main Menu Settings", true);
            if (mainMenuOffsetFoldout) {
                //0 for main menu, 1 for object menu
                if (menuVariantType.enumValueIndex == 0) {
                    EditorGUILayout.PropertyField(followOffset);
                    EditorGUILayout.PropertyField(followMaxViewHorizontalDegrees);
                    EditorGUILayout.PropertyField(followMaxViewVerticalDegrees);
                    EditorGUILayout.PropertyField(surfaceMagnetismSafetyOffset);
                }
                else {
                    EditorGUILayout.HelpBox("The menu is an object menu, so you don't need to set the properties for a main menu.", MessageType.Info);
                }
            }
            EditorGUILayout.Space();

            objectMenuOffsetFoldout = EditorGUILayout.Foldout(objectMenuOffsetFoldout, "Object Menu Settings", true);
            if (objectMenuOffsetFoldout) {
                //0 for main menu, 1 for object menu
                if (menuVariantType.enumValueIndex == 1) {
                    EditorGUILayout.PropertyField(orbitalOffset);
                    EditorGUILayout.PropertyField(manipulationLogic);
                }
                else {
                    EditorGUILayout.HelpBox("The menu is a main menu, so you don't need to set the properties for an object menu.", MessageType.Info);
                }
            }

            style.fontStyle = previousStyle;
            serializedObject.ApplyModifiedProperties();
        }
    }
}


