using UnityEngine;
using UnityEditor;
using VirtualEnvironments;

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

    public void MarkAsAssetBundleComponent(Material skybox, GameObject prefab, Sprite preview, TextAsset credits)
    {
        string skyboxPath = null;
        string prefabPath = null;
        string previewPath = null;
        string creditsPath = null;

        if (skybox != null)
        {
            skyboxPath = AssetDatabase.GetAssetPath(skybox);
            AssetImporter.GetAtPath(skyboxPath).SetAssetBundleNameAndVariant(_target.EnvironmentName, "");
        }

        if (prefab != null)
        {
            prefabPath = AssetDatabase.GetAssetPath(prefab);
            AssetImporter.GetAtPath(prefabPath).SetAssetBundleNameAndVariant(_target.EnvironmentName, "");
        }

        if (preview != null)
        {
            previewPath = AssetDatabase.GetAssetPath(preview);
            AssetImporter.GetAtPath(previewPath).SetAssetBundleNameAndVariant(_target.EnvironmentName, "");
        }

        if (credits != null)
        {
            creditsPath = AssetDatabase.GetAssetPath(credits);
            AssetImporter.GetAtPath(creditsPath).SetAssetBundleNameAndVariant(_target.EnvironmentName, "");
        }
    }

    public void OnButtonPressed()
    {
        MarkAsAssetBundleComponent(_target.SkyboxMaterial, _target.EnvironmentModelPrefab, _target.PreviewImageSprite, _target.CreatorCredits);
        BuildAssetBundles(_target.PathToTargetFolder);
    }

    static void BuildAssetBundles(string path)
    {
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }
}
