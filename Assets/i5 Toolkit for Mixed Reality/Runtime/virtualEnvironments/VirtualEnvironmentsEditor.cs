using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;

[CustomEditor(typeof(VirtualEnvironmentsManager))]
public class VirtualEnvironmentsEditor : Editor

{

    private SerializedProperty serverEnvironmentsFromInspector;
    private ReorderableList serverEnvironmentLoadingList;

    private SerializedProperty localEnvironmentsFromInspector;
    private ReorderableList localEnvironmentLoadingList;

    private AnimBool defaultValues;
    private AnimBool serverLoadingSettings;
    private AnimBool localLoadingSettings;

    private Material defaultSkybox;
    private GameObject defaultModel;
    private Sprite defaultPreviewImage;
    private string defaultCredits;

    private string serverBaseUrl;

    private string localBasePath;

    public void OnEnable()
    {
        serverEnvironmentsFromInspector = serializedObject.FindProperty("serverEnvironmentsFromInspector");
        serverEnvironmentLoadingList = new ReorderableList(serializedObject, serverEnvironmentsFromInspector, true, true, true, true);
        serverEnvironmentLoadingList.drawElementCallback = DrawListItemsServer;
        serverEnvironmentLoadingList.drawHeaderCallback = DrawHeaderServer;

        localEnvironmentsFromInspector = serializedObject.FindProperty("localEnvironmentsFromInspector");
        localEnvironmentLoadingList = new ReorderableList(serializedObject, localEnvironmentsFromInspector, true, true, true, true);
        localEnvironmentLoadingList.drawElementCallback = DrawListItemsLocal;
        localEnvironmentLoadingList.drawHeaderCallback = DrawHeaderLocal;

        defaultValues = new AnimBool(false);
        defaultValues.valueChanged.AddListener(Repaint);

        serverLoadingSettings = new AnimBool(false);
        serverLoadingSettings.valueChanged.AddListener(Repaint);

        localLoadingSettings = new AnimBool(false);
        localLoadingSettings.valueChanged.AddListener(Repaint);

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label("Default Virtual Environment", EditorStyles.boldLabel);

        defaultValues.target = EditorGUILayout.ToggleLeft("Set Default Evironment from Inspector", defaultValues.target);

        if (EditorGUILayout.BeginFadeGroup(defaultValues.faded))
        {
            EditorGUI.indentLevel++;

            defaultSkybox = (Material) EditorGUILayout.ObjectField("Default Skybox", null, typeof(Material), true);
            defaultModel = (GameObject) EditorGUILayout.ObjectField("Default Model Prefab", null, typeof(GameObject), true);
            defaultCredits = EditorGUILayout.TextField("Creator Credit", defaultCredits);
            defaultPreviewImage = (Sprite) EditorGUILayout.ObjectField("Default Preview Image", null, typeof(Sprite), true);


            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFadeGroup();

        EditorGUILayout.Space();
        GUILayout.Label("Loading Settings", EditorStyles.boldLabel);

        serverLoadingSettings.target = EditorGUILayout.ToggleLeft("Load Environments from Server", serverLoadingSettings.target);

        if (EditorGUILayout.BeginFadeGroup(serverLoadingSettings.faded))
        {
            EditorGUI.indentLevel++;

            serverBaseUrl = EditorGUILayout.TextField("Base URL of Server", serverBaseUrl);
            serializedObject.Update();
            serverEnvironmentLoadingList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFadeGroup();


        localLoadingSettings.target = EditorGUILayout.ToggleLeft("Load Environments from Local", localLoadingSettings.target);

        if (EditorGUILayout.BeginFadeGroup(localLoadingSettings.faded))
        {
            EditorGUI.indentLevel++;

            localBasePath = EditorGUILayout.TextField("Path to Root Folder", localBasePath);
            serializedObject.Update();
            localEnvironmentLoadingList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFadeGroup();
    }


    void DrawListItemsServer(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = serverEnvironmentLoadingList.serializedProperty.GetArrayElementAtIndex(index);


        // The 'level' property
        // The label field for level (width 100, height of a single line)
        EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "Name");

        // The property field for level. Since we do not need so much space in an int, width is set to 20, height of a single line.
        EditorGUI.PropertyField(
            new Rect(rect.x + 60, rect.y, 140, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("Name"),
            GUIContent.none
        );

        // The 'quantity' property
        // The label field for quantity (width 100, height of a single line)
        EditorGUI.LabelField(new Rect(rect.x + 200, rect.y, 100, EditorGUIUtility.singleLineHeight), "Loading Path");

        // The property field for quantity (width 20, height of a single line)
        EditorGUI.PropertyField(
            new Rect(rect.x + 300, rect.y, 300, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("LoadingPath"),
            GUIContent.none
        );

    }

    void DrawListItemsLocal(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty element = localEnvironmentLoadingList.serializedProperty.GetArrayElementAtIndex(index);


        // The 'level' property
        // The label field for level (width 100, height of a single line)
        EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "Name");

        // The property field for level. Since we do not need so much space in an int, width is set to 20, height of a single line.
        EditorGUI.PropertyField(
            new Rect(rect.x + 60, rect.y, 140, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("Name"),
            GUIContent.none
        );

        // The 'quantity' property
        // The label field for quantity (width 100, height of a single line)
        EditorGUI.LabelField(new Rect(rect.x + 200, rect.y, 100, EditorGUIUtility.singleLineHeight), "Loading Path");

        // The property field for quantity (width 20, height of a single line)
        EditorGUI.PropertyField(
            new Rect(rect.x + 300, rect.y, 300, EditorGUIUtility.singleLineHeight),
            element.FindPropertyRelative("LoadingPath"),
            GUIContent.none
        );

    }

    void DrawHeaderServer(Rect rect)
    {
        EditorGUI.LabelField(rect, "Environments from Server");
    }

    void DrawHeaderLocal(Rect rect)
    {
        EditorGUI.LabelField(rect, "Environments from Local");
    }
}
