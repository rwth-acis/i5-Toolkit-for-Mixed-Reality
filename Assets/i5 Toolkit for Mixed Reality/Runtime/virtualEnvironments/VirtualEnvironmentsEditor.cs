using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(VirtualEnvironmentsManager))]
public class VirtualEnvironmentsEditor : Editor

{
    private AnimBool defaultValues;

    private Material defaultSkybox;
    private GameObject defaultModel;
    private Sprite defaultPreviewImage;
    private string defaultCredits;

    public void OnEnable()
    {
        defaultValues = new AnimBool(false);
        defaultValues.valueChanged.AddListener(Repaint);
      
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label("Default Virtual Environment", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        defaultValues.target = EditorGUILayout.ToggleLeft("Set Default Evironment from Inspector", defaultValues.target);

        if (EditorGUILayout.BeginFadeGroup(defaultValues.faded))
        {
            EditorGUI.indentLevel++;

            defaultSkybox = (Material)EditorGUILayout.ObjectField("Default Skybox", defaultSkybox, typeof(Material));
            defaultModel = (GameObject)EditorGUILayout.ObjectField("Default Model Prefab", defaultModel, typeof(GameObject));
            defaultCredits = EditorGUILayout.TextField("Creator Credit", defaultCredits);
            defaultPreviewImage = (Sprite)EditorGUILayout.ObjectField("Default Preview Image", defaultSkybox, typeof(Sprite));

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFadeGroup();
    }
}
