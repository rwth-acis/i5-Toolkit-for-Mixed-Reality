using System;
using System.IO;
using UnityEngine;

namespace VirtualEnvironments
{
    public class VirtualEnvironmentsCreationTool : MonoBehaviour
    {

        public string EnvironmentName;

        public Material SkyboxMaterial;
        public GameObject EnvironmentModelPrefab;
        public Sprite PreviewImageSprite;
        public TextAsset CreatorCredits;

        public string PathToTargetFolder;

        public TextAsset ConvertStringToTxt(String content, String name, String path)
        {
            File.WriteAllText(path + name + "Credits.txt", content);
            return Resources.Load(path + name + "Credits.txt") as TextAsset;
        }

    }

}
