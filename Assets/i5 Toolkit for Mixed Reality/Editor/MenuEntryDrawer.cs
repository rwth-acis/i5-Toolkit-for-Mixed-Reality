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
    bool showTouchpadRightSetup = false;
    bool showTouchpadLeftSetup = false;
    bool showTouchpadUpSetup = false;
    bool showTouchpadDownSetup = false;
    bool showToolSpecificEvents = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.indentLevel++;

        showToolSetup = ShowPropertysInFoldout("General Tool Settings", property, new PropertyInfos[] {             new PropertyInfos("toolName", "Tool Name"),
                                                                                                                    new PropertyInfos("iconTool", "Tool Icon") }, showToolSetup);

        showTriggerSetup = ShowPropertysInFoldout("Trigger Settings", property, new PropertyInfos[] {               new PropertyInfos("textTrigger", "Description Text for the Trigger"),
                                                                                                                    new PropertyInfos("OnInputActionStartedTrigger", "On Trigger Press Started"),
                                                                                                                    new PropertyInfos("OnInputActionEndedTrigger","On Trigger Press Ended")}, showTriggerSetup);

        showGripSetup = ShowPropertysInFoldout("Grip Settings", property, new PropertyInfos[] {                     new PropertyInfos("textGrip", "Description Text for the Grip Buttons"),
                                                                                                                    new PropertyInfos("OnInputActionStartedTrigger", "On Grip Press Started"),
                                                                                                                    new PropertyInfos("OnInputActionEndedTrigger","On Grip Press Ended")}, showGripSetup);

        showTouchpadRightSetup = ShowPropertysInFoldout("Touchpad Right Settings", property, new PropertyInfos[]{   new PropertyInfos("textTouchpadRight", "Description Text for Touchpad Right"),
                                                                                                                    new PropertyInfos("iconTouchpadRight", "Icon Displayed on Touchpad"),
                                                                                                                    new PropertyInfos("OnInputActionEndedTouchpadRight","On Touchpad Press Ended")}, showTouchpadRightSetup);

        showTouchpadLeftSetup = ShowPropertysInFoldout("Touchpad Left Settings", property, new PropertyInfos[]{     new PropertyInfos("textTouchpadLeft", "Description Text for Touchpad Left"),
                                                                                                                    new PropertyInfos("iconTouchpadLeft", "Icon Displayed on Touchpad"),
                                                                                                                    new PropertyInfos("OnInputActionEndedTouchpadLeft","On Touchpad Press Ended")}, showTouchpadLeftSetup);

        showTouchpadUpSetup = ShowPropertysInFoldout("Touchpad Up Settings", property, new PropertyInfos[]{         new PropertyInfos("textTouchpadUp", "Description Text for Touchpad Up"),
                                                                                                                    new PropertyInfos("iconTouchpadUp", "Icon Displayed on Touchpad"),
                                                                                                                    new PropertyInfos("OnInputActionEndedTouchpadUp","On Touchpad Press Ended")}, showTouchpadUpSetup);

        showTouchpadDownSetup = ShowPropertysInFoldout("Touchpad Down Settings", property, new PropertyInfos[]{     new PropertyInfos("textTouchpadDown", "Description Text for Touchpad Down"),
                                                                                                                    new PropertyInfos("iconTouchpadDown", "Icon Displayed on Touchpad"),
                                                                                                                    new PropertyInfos("OnInputActionEndedTouchpadDown","On Touchpad Press Ended")}, showTouchpadDownSetup);

        showToolSpecificEvents = ShowPropertysInFoldout("Tool Specific Events", property, new PropertyInfos[]{      new PropertyInfos("OnToolCreated", ""),
                                                                                                                    new PropertyInfos("OnToolDestroyed", ""),
                                                                                                                    new PropertyInfos("OnHoverOverTargetStart",""),
                                                                                                                    new PropertyInfos("OnHoverOverTargetActive",""),
                                                                                                                    new PropertyInfos("OnHoverOverTargetStop", "")}, showToolSpecificEvents);
        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }


    private static bool ShowPropertysInFoldout(string lable, SerializedProperty property, PropertyInfos[] properties, bool status)
    {
        GUIStyle borders = new GUIStyle(EditorStyles.helpBox);
        EditorGUILayout.BeginVertical(borders);
        status = EditorGUILayout.Foldout(status, lable);
        EditorGUI.indentLevel++;

        if (status)
        {
            foreach (var propertyToShow in properties)
            {
                if(propertyToShow.text != "")
                    EditorGUILayout.PropertyField(property.FindPropertyRelative(propertyToShow.path), new GUIContent(propertyToShow.text));
                else
                    EditorGUILayout.PropertyField(property.FindPropertyRelative(propertyToShow.path));
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
