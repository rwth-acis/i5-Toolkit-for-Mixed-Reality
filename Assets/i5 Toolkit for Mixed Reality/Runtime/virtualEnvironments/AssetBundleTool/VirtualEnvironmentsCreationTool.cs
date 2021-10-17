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
    public class VirtualEnvironmentsCreationTool : MonoBehaviour
    {

        public Material SkyboxMaterial;
        public GameObject EnvironmentModelPrefab;
        public Sprite PreviewImageSprite;
        public string CreatorCredits;
        public string PathToTargetFolder = "D:/Documents/Unidokumente";

        public TextAsset ConvertStringToTxt(String content, String path)
        {
            File.WriteAllText(path + "EnvironmentCredits.txt", content);
            return Resources.Load(path + "EnvironmentCredits.txt") as TextAsset;
        }


        public void AssembleAssetBundleComponents(Material skybox, GameObject prefab, Sprite preview, TextAsset credits, String path)
        {
            UnityEngine.Object[] components = { skybox, prefab, preview, credits };
            BuildPipeline.BuildAssetBundle(skybox, components, path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
            Selection.objects = components;
        }

        public void Start()
        {
            AssembleAssetBundleComponents(SkyboxMaterial, EnvironmentModelPrefab, PreviewImageSprite, ConvertStringToTxt(CreatorCredits, PathToTargetFolder), PathToTargetFolder);
        }


    }
}
