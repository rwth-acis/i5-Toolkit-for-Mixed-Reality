using UnityEngine;
using UnityEditor;
using VirtualEnvironments;
using System.IO;
using System;

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

    private VirtualEnvironmentsCreationTool _target;


    public void OnEnable()
    {
        EnvironmentName = serializedObject.FindProperty("EnvironmentName");
        SkyboxMaterial = serializedObject.FindProperty("SkyboxMaterial");
        EnvironmentModelPrefab = serializedObject.FindProperty("EnvironmentModelPrefab");
        PreviewImageSprite = serializedObject.FindProperty("PreviewImageSprite");
        CreatorCredits = serializedObject.FindProperty("CreatorCredits");
        PathToTargetFolder = serializedObject.FindProperty("PathToTargetFolder");
        _target = (VirtualEnvironmentsCreationTool) target;
    }


    public override void OnInspectorGUI()
    {
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

        EditorGUILayout.Space();

        if (GUILayout.Button("Build AssetBundle"))
        {
            OnButtonPressed();
        }
    }


    /// <summary>
    /// The selected components of the Virtual Environment are labelled with with the environment name and thus marked to be bundled together. Returns whether this step was successful.
    /// </summary>
    /// <param name="skybox">The skybox material of the environment.</param>
    /// <param name="prefab">The 3D model of the environment.</param>
    /// <param name="preview">The preview image of the environment.</param>
    /// <param name="credits">The credits of the creators of the environment components.</param>
    public bool MarkAsAssetBundleComponent(Material skybox, GameObject prefab, Sprite preview, TextAsset credits)
    {
        String skyboxPath = null;
        String prefabPath = null;
        String previewPath = null;
        String creditsPath = null;

        if (skybox != null)
        {
            skyboxPath = AssetDatabase.GetAssetPath(skybox);
            if(AssetImporter.GetAtPath(skyboxPath) == null)
            {
                Debug.Log("The selected skybox is not stored on your disk. Make sure it has a valid path.");
                return false;
            }
            else
            {
                AssetImporter.GetAtPath(skyboxPath).SetAssetBundleNameAndVariant(_target.EnvironmentName, "");
            }
        }

        if (prefab != null)
        {
            prefabPath = AssetDatabase.GetAssetPath(prefab);
            if (AssetImporter.GetAtPath(prefabPath) == null)
            {
                Debug.Log("The selected prefab of the 3D model is not stored on your disk. Make sure it has a valid path.");
                return false;
            }
            else
            {
                AssetImporter.GetAtPath(prefabPath).SetAssetBundleNameAndVariant(_target.EnvironmentName, "");
            }
        }

        if (preview != null)
        {
            previewPath = AssetDatabase.GetAssetPath(preview);
            if (AssetImporter.GetAtPath(previewPath) == null)
            {
                Debug.Log("The selected preview image is not stored on your disk. Make sure it has a valid path.");
                return false;
            }
            else
            {
                AssetImporter.GetAtPath(previewPath).SetAssetBundleNameAndVariant(_target.EnvironmentName, "");
            }
        }

        if (credits != null)
        {
            creditsPath = AssetDatabase.GetAssetPath(credits);
            if (AssetImporter.GetAtPath(creditsPath) == null)
            {
                Debug.Log("The selected text file of the credits is not stored on your disk. Make sure it has a valid path.");
            }
            else
            {
                AssetImporter.GetAtPath(creditsPath).SetAssetBundleNameAndVariant(_target.EnvironmentName, "");
            }
        }
        return true;
    }


    /// <summary>
    /// Once the build button has been pressed, it is checked whether all necessary components have been selected. If this is the case, the selected assets are labelled and then bundled in an AssetBundle if the labelling was successful.
    /// </summary>
    public void OnButtonPressed()
    {
        if(_target.SkyboxMaterial == null ||_target.PreviewImageSprite == null || _target.CreatorCredits == null)
        {
            Debug.Log("No Asset Bundle has been created. Make sure that all necessary components have been added. See the documentation of the Virtual Environment Feature for this.");
        }
        else
        {
            bool buildAssetBundle = MarkAsAssetBundleComponent(_target.SkyboxMaterial, _target.EnvironmentModelPrefab, _target.PreviewImageSprite, _target.CreatorCredits);
            if (buildAssetBundle)
            {
                BuildAssetBundles(_target.PathToTargetFolder);
                CleanUpEditor();
            }
            else
            {
                Debug.Log("No Asset Bundle has been created. Make sure that all necessary components that have been added are stored on your local disk.");
            }
        }
    }


    /// <summary>
    /// We build all AssetBundles based on the previously assigned labels. Afterwards the Assets are refreshed.
    /// </summary>
    /// <param name="path">The path of where the AssetBundle is to be stored.</param>
    static void BuildAssetBundles(string path)
    {
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        AssetDatabase.Refresh();
    }


    /// <summary>
    /// The Editor is emptied, such that the user has to reselect components for another AssetBundle.
    /// </summary>
    public void CleanUpEditor()
    {
        _target.EnvironmentName = null;
        _target.SkyboxMaterial = null;
        _target.EnvironmentModelPrefab = null;
        _target.PreviewImageSprite = null;
        _target.CreatorCredits = null;

        serializedObject.Update();
        EditorGUILayout.PropertyField(EnvironmentName);
        serializedObject.ApplyModifiedProperties();

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

    }
}
