using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PieMenuServiceBootstraper))]
public class PieMenuEditor : Editor
{
    int state = 0;
    public override void OnInspectorGUI()
    {
        PieMenuServiceBootstraper pieMenu = (PieMenuServiceBootstraper)target;

        GUIStyle borders = new GUIStyle(EditorStyles.helpBox);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(borders, GUILayout.MaxWidth(30));

        if (GUILayout.Button("Apperance"))
        {
            state = 0;
        }
        if (GUILayout.Button("Actions"))
        {
            state = 1;
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(borders);

        if (state == 0)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.pieMenuPieceNormalColor"), new GUIContent("Color of the PieMenu"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.pieMenuPieceHighlighColor"), new GUIContent("Color of highlighted pieces"));
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.triggerInputAction"), new GUIContent("Trigger input action"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.touchpadTouchActionAction"), new GUIContent("Touchpad touch action"));
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }
}

public enum PieInsepectoState
{
    Apperace,
    Actions
}
