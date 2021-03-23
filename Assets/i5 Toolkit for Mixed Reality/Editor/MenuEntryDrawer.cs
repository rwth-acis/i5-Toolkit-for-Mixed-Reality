using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MenuEntry))]
public class MenuEntryDrawer : PropertyDrawer
{
    bool showToolSetup = false;
    bool showTriggerSetup = false;
    bool showGripSetup = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.indentLevel++;

        showToolSetup = ShowConentInFoldout("General Tool Settings", property, new PropertyInfos[] {new PropertyInfos("toolName", "Tool Name"), new PropertyInfos("iconTool", "Tool Icon") }, showToolSetup);

        showTriggerSetup = ShowConentInFoldout("Trigger Settings", property, new PropertyInfos[] {  new PropertyInfos("textTrigger", "Description Text for the Trigger"),
                                                                                                    new PropertyInfos("OnInputActionStartedTrigger", "On Trigger Press Started"),
                                                                                                    new PropertyInfos("OnInputActionEndedTrigger","On Trigger Press Ended")}, showTriggerSetup);

        showGripSetup = ShowConentInFoldout("Grip Settings", property, new PropertyInfos[] {        new PropertyInfos("textGrip", "Description Text for the Grip Buttons"),
                                                                                                    new PropertyInfos("OnInputActionStartedTrigger", "On Grip Press Started"),
                                                                                                    new PropertyInfos("OnInputActionEndedTrigger","On Grip Press Ended")}, showGripSetup);

        showGripSetup = ShowConentInFoldout("Touchpad Right Settings", property,new PropertyInfos[]{new PropertyInfos("textTouchpadRight", "Description Text for the Grip Buttons"),
                                                                                                    new PropertyInfos("OnInputActionStartedTrigger", "On Grip Press Started"),
                                                                                                    new PropertyInfos("OnInputActionEndedTrigger","On Grip Press Ended")}, showGripSetup);

    }


    private static bool ShowConentInFoldout(string lable, SerializedProperty property, PropertyInfos[] properties, bool status)
    {
        GUIStyle borders = new GUIStyle(EditorStyles.helpBox);
        EditorGUILayout.BeginVertical(borders);
        status = EditorGUILayout.Foldout(status, lable);
        EditorGUI.indentLevel++;

        if (status)
        {
            foreach (var propertie in properties)
            {
                if(propertie.text != "")
                    EditorGUILayout.PropertyField(property.FindPropertyRelative(propertie.path), new GUIContent(propertie.text));
                else
                    EditorGUILayout.PropertyField(property.FindPropertyRelative(propertie.path));
            }
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();

        return status;
    }
}

struct PropertyInfos
{
    public string path;
    public string text;
    public PropertyInfos(string path, string text)
    {
        this.path = path;
        this.text = text;
    }
}
