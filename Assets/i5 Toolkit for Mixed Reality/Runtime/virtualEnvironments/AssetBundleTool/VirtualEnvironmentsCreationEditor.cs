using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using VirtualEnvironments;
using System;
using System.IO;

[CustomEditor(typeof(VirtualEnvironmentsCreationTool))]
[CanEditMultipleObjects]
public class VirtualEnvironmentsCreationEditor : Editor
{

    private SerializedProperty EnvironmentName;

    private SerializedProperty SkyboxMaterial;
    private SerializedProperty EnvironmentModelPrefab;
    private SerializedProperty PreviewImageSprite;
    private SerializedProperty CreatorCredits;

    private SerializedProperty PathToTargetFolder;

    public void OnEnable()
    {
        EnvironmentName = serializedObject.FindProperty("EnvironmentName");
        SkyboxMaterial = serializedObject.FindProperty("SkyboxMaterial");
        EnvironmentModelPrefab = serializedObject.FindProperty("EnvironmentModelPrefab");
        PreviewImageSprite = serializedObject.FindProperty("PreviewImageSprite");
        CreatorCredits = serializedObject.FindProperty("CreatorCredits");
        PathToTargetFolder = serializedObject.FindProperty("PathToTargetFolder");
    }

    //TODO BUTTON
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        GUIStyle borders = new GUIStyle(EditorStyles.helpBox);
        GUILayout.Label("Bundle Information", EditorStyles.boldLabel);

        serializedObject.Update();
        EditorGUILayout.PropertyField(EnvironmentName);
        serializedObject.ApplyModifiedProperties();

        serializedObject.Update();
        EditorGUILayout.PropertyField(PathToTargetFolder);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        EditorGUI.indentLevel++;

        GUILayout.Label("Virtual Environment Components", EditorStyles.boldLabel);

        serializedObject.Update();
        EditorGUILayout.PropertyField(SkyboxMaterial);
        serializedObject.ApplyModifiedProperties();

        serializedObject.Update();
        EditorGUILayout.PropertyField(EnvironmentModelPrefab);
        serializedObject.ApplyModifiedProperties();

        serializedObject.Update();
        EditorGUILayout.PropertyField(PreviewImageSprite);
        serializedObject.ApplyModifiedProperties();

        serializedObject.Update();
        EditorGUILayout.PropertyField(CreatorCredits);
        serializedObject.ApplyModifiedProperties();

        EditorGUI.indentLevel--;
    }

    public TextAsset ConvertStringToTxt(String content, String name, String path)
    {
        File.WriteAllText(path + name + "Credits.txt", content);
        return Resources.Load(path + name + "Credits.txt") as TextAsset;
    }

    public void MarkAsAssetBundleComponent(Material skybox, GameObject prefab, Sprite preview, TextAsset credits, String path)
    {
        //File.Move(skybox., path);
        //var skyboxPath = AssetDatabase.GUIDToAssetPath(skybox);
        //var assetName = AssetDatabase.LoadMainAssetAtPath(path).name;
        //AssetDatabase.MoveAsset(path, $"Assets/Scenes/{assetName}.unity");
        //UnityEngine.Object[] components = { skybox, prefab, preview, credits };
        //BuildPipeline.BuildAssetBundle(skybox, components, path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        //Selection.objects = components;
    }

    public void OnButtonPressed()
    {
        //AssembleAssetBundleComponents(SkyboxMaterial, EnvironmentModelPrefab, PreviewImageSprite, ConvertStringToTxt(CreatorCredits, PathToTargetFolder), PathToTargetFolder);
    }
}
