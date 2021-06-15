using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;
using FakeItEasy;
using UnityEngine.TestTools;
using i5.Toolkit.Core.TestHelpers;
using i5.Toolkit.MixedReality.PieMenu;
using i5.Toolkit.Core.ServiceCore;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using Microsoft.MixedReality.Toolkit;

namespace i5.Toolkit.MixedReality.Tests.PieMenu {
    public class PieMenuTests
    {
        [SetUp]
        public void LoadScene()
        {
            var sceneParameter = new LoadSceneParameters { loadSceneMode = LoadSceneMode.Single, localPhysicsMode = LocalPhysicsMode.Physics3D};
            SceneManager.LoadScene("PieMenuTestScene");
        }

        [TearDown]
        public void TearDownScene()
        {
            SceneManager.UnloadScene("PieMenuTestScene");
        }

        [UnityTest]
        public IEnumerator ServiceManager_can_return_toolSetupService()
        {
            yield return null;
            var toolSetupService = ServiceManager.GetService<ToolSetupService>();
            Assert.IsNotNull(toolSetupService, "ToolSetupService");
        }

        [UnityTest]
        public IEnumerator Can_spawn_PieMenu()
        {
            var pieMenuManager = GameObject.FindObjectOfType<PieMenuManager>();
            //pieMenuManager.op
            var test = GameObject.FindObjectOfType<MixedRealityToolkit>();
            
            

            return null;
        }

        
    }
}
