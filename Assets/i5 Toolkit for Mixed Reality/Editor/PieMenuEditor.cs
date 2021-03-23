using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PieMenuServiceBootstraper))]
public class PieMenuEditor : Editor
{
    private PieMenuInsepectorState state = PieMenuInsepectorState.Apperance;
    public override void OnInspectorGUI()
    {
        PieMenuServiceBootstraper pieMenu = (PieMenuServiceBootstraper)target;

        GUIStyle borders = new GUIStyle(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(borders, GUILayout.MaxWidth(30));

        if (GUILayout.Button("Apperance"))
        {
            state = PieMenuInsepectorState.Apperance;
        }
        if (GUILayout.Button("Actions"))
        {
            state = PieMenuInsepectorState.Actions;
        }
        if (GUILayout.Button("Default Behavior"))
        {
            state = PieMenuInsepectorState.DefaultBehavior;
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(borders);

        switch (state)
        {
            case PieMenuInsepectorState.Apperance:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.pieMenuPieceNormalColor"), new GUIContent("Color of the PieMenu"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.pieMenuPieceHighlighColor"), new GUIContent("Color of highlighted pieces"));
                break;
            case PieMenuInsepectorState.Actions:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.menuAction"), new GUIContent("Menu action"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.triggerInputAction"), new GUIContent("Trigger input action"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.touchpadTouchActionAction"), new GUIContent("Touchpad touch action"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.touchpadPressAction"), new GUIContent("Touchpad press action"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.gripPressAction"), new GUIContent("Grip press action"));
                break;
            case PieMenuInsepectorState.DefaultBehavior:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.defaultEntry"), true);
                serializedObject.ApplyModifiedProperties();
                break;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
}

public enum PieMenuInsepectorState
{
    Apperance,
    Actions,
    DefaultBehavior
}
