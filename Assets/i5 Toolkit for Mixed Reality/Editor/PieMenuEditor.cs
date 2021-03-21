using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PieMenuServiceBootstraper))]
public class PieMenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        PieMenuServiceBootstraper pieMenu = (PieMenuServiceBootstraper)target;
        EditorGUILayout.LabelField("Color Options");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.pieMenuPieceNormalColor"),new GUIContent("Color of the PieMenu"));
        EditorGUILayout.LabelField("Actions");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("toolSetup.triggerInputAction"), new GUIContent("Trigger Input Action"));
    }
}
