using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// Replaces the default inspector by one that looks similar to the Microsoft MRTK inspector
    /// </summary>
    [CustomEditor(typeof(PieMenuServiceBootstraper))]
    public class PieMenuEditor : Editor
    {
        private PieMenuInsepectorState state = PieMenuInsepectorState.Apperance;
        public override void OnInspectorGUI()
        {
            PieMenuServiceBootstraper pieMenu = (PieMenuServiceBootstraper)target;

            GUIStyle borders = new GUIStyle(EditorStyles.helpBox);
            GUIStyle buttonMarked = new GUIStyle(GUI.skin.GetStyle("Button"));
            buttonMarked.normal = new GUIStyleState { background = buttonMarked.active.background };

            GUIStyle buttonNormal = new GUIStyle(GUI.skin.GetStyle("Button"));


            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(borders, GUILayout.MaxWidth(30));

            if (GUILayout.Button("Appearance", state == PieMenuInsepectorState.Appearance ? buttonMarked : buttonNormal))
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
            if (GUILayout.Button("Debug", state == PieMenuInsepectorState.Debug ? buttonMarked : buttonNormal))
            {
                state = PieMenuInsepectorState.Debug;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(borders);

            switch (state)
            {
                case PieMenuInsepectorState.Apperance:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.pieMenuPieceNormalColor"), new GUIContent("Color of the PieMenu"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.pieMenuPieceHighlighColor"), new GUIContent("Color of highlighted pieces"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.descriptionShowTime"), new GUIContent("Description show time in Seconds"));
                    break;

                case PieMenuInsepectorState.Actions:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.menuAction"), new GUIContent("Menu action"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.triggerInputAction"), new GUIContent("Trigger input action"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.touchpadTouchActionAction"), new GUIContent("Touchpad touch action"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.touchpadPressAction"), new GUIContent("Touchpad press action"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.gripPressAction"), new GUIContent("Grip press action"));
                    break;

                case PieMenuInsepectorState.DefaultBehavior:
                    EditorGUILayout.HelpBox("The default behavior will always be used when the currently selected tool doesn't specify an action for a binding.", MessageType.Info);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.defaultEntry"), true);
                    EditorGUI.indentLevel--;
                    break;

                case PieMenuInsepectorState.MenuEntries:
                    EditorGUI.indentLevel++;
                    SerializedProperty menuEntries = serializedObject.FindProperty("toolSetup.menuEntries");
                    int entryToDelete = -1;

                    int size = menuEntries.arraySize;

                    for (int i = 0; i < size; i++)
                    {
                        EditorGUILayout.BeginVertical(borders);
                        EditorGUILayout.PropertyField(menuEntries.GetArrayElementAtIndex(i), true);
                        if (GUILayout.Button("Delete this entry"))
                        {
                            entryToDelete = i;
                        }
                        EditorGUILayout.EndVertical();
                    }

                    if (entryToDelete >= 0)
                    {
                        menuEntries.DeleteArrayElementAtIndex(entryToDelete);
                    }

                    if (GUILayout.Button("Add Entry"))
                    {
                        menuEntries.InsertArrayElementAtIndex(menuEntries.arraySize);
                    }
                    EditorGUI.indentLevel--;
                    break;
                case PieMenuInsepectorState.Debug:
                    EditorGUI.indentLevel++;

                    void OverrideThumbPosition(Vector2 position)
                    {
                        foreach (var source in CoreServices.InputSystem.DetectedInputSources)
                        {
                            foreach (var p in source.Pointers)
                            {
                                ViveWandVirtualTool tool = ActionHelperFunctions.GetVirtualTool(p);
                                if (tool != null)
                                {
                                    tool.thumbPosition = position;
                                    break;
                                }
                            }
                        }
                    }

                    if (GUILayout.Button("Override Thumposition Right"))
                    {
                        OverrideThumbPosition(new Vector2(1,0));
                    }
                    if (GUILayout.Button("Override Thumposition Up"))
                    {
                        OverrideThumbPosition(new Vector2(0, 1));
                    }
                    if (GUILayout.Button("Override Thumposition Left"))
                    {
                        OverrideThumbPosition(new Vector2(-1, 0));
                    }
                    if (GUILayout.Button("Override Thumposition Down"))
                    {
                        OverrideThumbPosition(new Vector2(0, -1));
                    }

                    EditorGUI.indentLevel--;
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
        MenuEntries,
        Debug
    } 
}
