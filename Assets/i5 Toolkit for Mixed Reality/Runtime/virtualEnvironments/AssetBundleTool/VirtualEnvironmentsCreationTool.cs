using System;
using System.IO;
using UnityEngine;

namespace VirtualEnvironments
{
    /// <summary>
    /// Provides a AssetBundle build tool for Virtual Environments.
    /// </summary>
    public class VirtualEnvironmentsCreationTool : MonoBehaviour
    {
        /// <summary>
        /// Name and Path of bundle that is to be created.
        /// </summary>
        public String EnvironmentName;
        public String PathToTargetFolder;

        /// <summary>
        /// Components of the environment that should be bundled. The environment is given by a skybox and a 3D model as prefab, as well as a preview image and credits.
        /// </summary>
        public Material SkyboxMaterial;
        public GameObject EnvironmentModelPrefab;
        public Sprite PreviewImageSprite;
        public TextAsset CreatorCredits;

        /// <summary>
        /// Converts a string into a txt file and stores it at the given path with the given name. Returns the file itself.
        /// </summary>
        /// <param name="content">The content of the file.</param>
        /// <param name="name">The name of the file.</param>
        /// <param name="path">The path of the file.</param>
        public TextAsset ConvertStringToTxt(String content, String name, String path)
        {
            File.WriteAllText(path + name + "Credits.txt", content);
            return Resources.Load(path + name + "Credits.txt") as TextAsset;
        }
    }
}
