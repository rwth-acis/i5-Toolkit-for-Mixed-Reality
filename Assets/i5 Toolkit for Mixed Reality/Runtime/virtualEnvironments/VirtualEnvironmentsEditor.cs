using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using VirtualEnvironments;

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

    private SerializedProperty defaultSkybox;
    private SerializedProperty defaultModel;
    private SerializedProperty defaultPreviewImage;
    private SerializedProperty defaultCredits;

    private SerializedProperty serverBaseUrl;
    private SerializedProperty localBasePath;

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

        defaultSkybox = serializedObject.FindProperty("defaultSkybox");
        defaultModel = serializedObject.FindProperty("defaultModel");
        defaultPreviewImage = serializedObject.FindProperty("defaultPreviewImage");
        defaultCredits = serializedObject.FindProperty("defaultCredits");

        serverBaseUrl = serializedObject.FindProperty("serverLoadingBaseURL");
        localBasePath = serializedObject.FindProperty("localLoadingBasePath");

        defaultValues = new AnimBool(false);
        defaultValues.valueChanged.AddListener(Repaint);

        serverLoadingSettings = new AnimBool(false);
        serverLoadingSettings.valueChanged.AddListener(Repaint);

        localLoadingSettings = new AnimBool(false);
        localLoadingSettings.valueChanged.AddListener(Repaint);

    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        GUIStyle borders = new GUIStyle(EditorStyles.helpBox);

        GUILayout.Label("Default Virtual Environment", EditorStyles.boldLabel);

        defaultValues.target = EditorGUILayout.Foldout(defaultValues.target, "Set Default Environment from Inspector");

        if (EditorGUILayout.BeginFadeGroup(defaultValues.faded))
        {
            EditorGUI.indentLevel++;

            serializedObject.Update();
            EditorGUILayout.PropertyField(defaultSkybox);
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();
            EditorGUILayout.PropertyField(defaultModel);
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();
            EditorGUILayout.PropertyField(defaultCredits);
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();
            EditorGUILayout.PropertyField(defaultPreviewImage);
            serializedObject.ApplyModifiedProperties();
     

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFadeGroup();

        EditorGUILayout.Space();
        GUILayout.Label("Loading Settings", EditorStyles.boldLabel);

        serverLoadingSettings.target = EditorGUILayout.Foldout(serverLoadingSettings.target, "Load Environments from Server");

        if (EditorGUILayout.BeginFadeGroup(serverLoadingSettings.faded))
        {
            EditorGUI.indentLevel++;

            serializedObject.Update();
            EditorGUILayout.PropertyField(serverBaseUrl);
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();
            serverEnvironmentLoadingList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFadeGroup();


        localLoadingSettings.target = EditorGUILayout.Foldout(localLoadingSettings.target, "Load Environments from Local");

        if (EditorGUILayout.BeginFadeGroup(localLoadingSettings.faded))
        {
            EditorGUI.indentLevel++;

            serializedObject.Update();
            EditorGUILayout.PropertyField(localBasePath);
            serializedObject.ApplyModifiedProperties();

            serializedObject.Update();
            localEnvironmentLoadingList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFadeGroup();
    }


    void DrawListItemsServer(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty serverItemList = serverEnvironmentLoadingList.serializedProperty.GetArrayElementAtIndex(index);

        EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "Name");
        EditorGUI.PropertyField(new Rect(rect.x + 60, rect.y, 140, EditorGUIUtility.singleLineHeight), serverItemList.FindPropertyRelative("Name"), GUIContent.none);
        EditorGUI.LabelField(new Rect(rect.x + 200, rect.y, 100, EditorGUIUtility.singleLineHeight), "Loading Path");
        EditorGUI.PropertyField(new Rect(rect.x + 300, rect.y, 300, EditorGUIUtility.singleLineHeight), serverItemList.FindPropertyRelative("LoadingPath"), GUIContent.none);

    }

    void DrawListItemsLocal(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty localItemList = localEnvironmentLoadingList.serializedProperty.GetArrayElementAtIndex(index);

        EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "Name");
        EditorGUI.PropertyField(new Rect(rect.x + 60, rect.y, 140, EditorGUIUtility.singleLineHeight), localItemList.FindPropertyRelative("Name"), GUIContent.none);
        EditorGUI.LabelField(new Rect(rect.x + 200, rect.y, 100, EditorGUIUtility.singleLineHeight), "Loading Path");
        EditorGUI.PropertyField(new Rect(rect.x + 300, rect.y, 300, EditorGUIUtility.singleLineHeight), localItemList.FindPropertyRelative("LoadingPath"), GUIContent.none);

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
