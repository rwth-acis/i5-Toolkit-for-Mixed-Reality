using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using i5.Toolkit.Core.Utilities;
using UnityEngine.UI;
using System.IO;
using UnityEditor;

namespace VirtualEnvironments
{
    [MenuItem("AssetDatabase/VirtualEnvironmentsCreationTool")]
    public class VirtualEnvironmentsCreationTool : MonoBehaviour
    {

        public Material SkyboxMaterial;
        public GameObject EnvironmentModelPrefab;
        public Sprite PreviewImageSprite;
        public string CreatorCredits;
        public string PathToTargetFolder = "D:/Documents/Unidokumente";
        public string AssetBundleName;

        public TextAsset ConvertStringToTxt(String content, String path)
        {
            File.WriteAllText(path + "EnvironmentCredits.txt", content);
            return Resources.Load(path + "EnvironmentCredits.txt") as TextAsset;
        }


        public void AssembleAssetBundleComponentsInFolder(Material skybox, GameObject prefab, Sprite preview, TextAsset credits, String path)
        {
            File.Move(skybox., path);
            //var skyboxPath = AssetDatabase.GUIDToAssetPath(skybox);
            //var assetName = AssetDatabase.LoadMainAssetAtPath(path).name;
            //AssetDatabase.MoveAsset(path, $"Assets/Scenes/{assetName}.unity");
            //UnityEngine.Object[] components = { skybox, prefab, preview, credits };
            //BuildPipeline.BuildAssetBundle(skybox, components, path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            //Selection.objects = components;
        }

        public void Start()
        {
            //AssembleAssetBundleComponents(SkyboxMaterial, EnvironmentModelPrefab, PreviewImageSprite, ConvertStringToTxt(CreatorCredits, PathToTargetFolder), PathToTargetFolder);
        }


    }
}
