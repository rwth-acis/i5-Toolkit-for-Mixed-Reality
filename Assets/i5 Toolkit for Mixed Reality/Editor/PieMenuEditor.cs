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
        GUIStyle buttonMarked = new GUIStyle(GUI.skin.GetStyle("Button"));
        buttonMarked.normal = new GUIStyleState { background = buttonMarked.active.background};

        GUIStyle buttonNormal = new GUIStyle(GUI.skin.GetStyle("Button"));
        

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(borders, GUILayout.MaxWidth(30));

        if (GUILayout.Button("Apperance", state == PieMenuInsepectorState.Apperance ? buttonMarked : buttonNormal))
        {
            state = PieMenuInsepectorState.Apperance;
        }
        if (GUILayout.Button("Actions", state == PieMenuInsepectorState.Actions ? buttonMarked : buttonNormal))
        {
            state = PieMenuInsepectorState.Actions;
        }
        if (GUILayout.Button("Default Behavior", state == PieMenuInsepectorState.DefaultBehavior ? buttonMarked : buttonNormal))
        {
            state = PieMenuInsepectorState.DefaultBehavior;
        }
        if (GUILayout.Button("Menu Entries", state == PieMenuInsepectorState.MenuEntries ? buttonMarked : buttonNormal))
        {
            state = PieMenuInsepectorState.MenuEntries;
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(borders);

        switch (state)
        {
            case PieMenuInsepectorState.Apperance:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.pieMenuPieceNormalColor"), new GUIContent("Color of the PieMenu"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.pieMenuPieceHighlighColor"), new GUIContent("Color of highlighted pieces"));
                EditorGUILayout.TextField("asd");
                break;
            case PieMenuInsepectorState.Actions:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.menuAction"), new GUIContent("Menu action"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.triggerInputAction"), new GUIContent("Trigger input action"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.touchpadTouchActionAction"), new GUIContent("Touchpad touch action"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.touchpadPressAction"), new GUIContent("Touchpad press action"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.gripPressAction"), new GUIContent("Grip press action"));
                break;
            case PieMenuInsepectorState.DefaultBehavior:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.defaultEntry"));
                serializedObject.ApplyModifiedProperties();
                break;
            case PieMenuInsepectorState.MenuEntries:
                SerializedProperty list = serializedObject.FindProperty("toolSetup.menuEntries");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.menuEntries"),true);

                SerializedProperty menuEntries = serializedObject.FindProperty("toolSetup.menuEntries");
                //menuEntries.ClearArray();

                //if (GUILayout.Button("Clear"))
                //{
                //    menuEntries.ClearArray();
                //}

                //int size = menuEntries.arraySize;

                //for (int i = 0; i < size; i++)
                //{
                //    EditorGUILayout.PropertyField(menuEntries.GetArrayElementAtIndex(i));
                //}

                //if (GUILayout.Button("Add Entry"))
                //{
                //    menuEntries.InsertArrayElementAtIndex(menuEntries.arraySize);
                //}
                break;
        }
        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

}

public enum PieMenuInsepectorState
{
    Apperance,
    Actions,
    DefaultBehavior,
    MenuEntries
}
