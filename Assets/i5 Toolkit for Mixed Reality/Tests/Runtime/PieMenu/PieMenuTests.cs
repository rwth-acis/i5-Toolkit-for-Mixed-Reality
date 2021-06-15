using System.Collections;
using UnityEngine;
using NUnit.Framework;
using FakeItEasy;
using UnityEngine.TestTools;
using i5.Toolkit.Core.TestHelpers;
using i5.Toolkit.MixedReality.PieMenu;
using i5.Toolkit.Core.ServiceCore;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

namespace i5.Toolkit.MixedReality.Tests.PieMenu {
    public class PieMenuTests
    {
        private ViveWandVirtualTool fakeTool;
        private ToolSetupService setupService;

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
            setupService = ServiceManager.GetService<ToolSetupService>();
            Assert.IsNotNull(setupService, "ToolSetupService");
        }

        [UnityTest]
        public IEnumerator Can_spawn_PieMenu()
        {
            yield return null;
            //fakeTool = PlayModeTestUtilities.GetFakeController();
            setupService = ServiceManager.GetService<ToolSetupService>();
            var pieMenuManager = GameObject.FindObjectOfType<PieMenuManager>();
            //ViveWandVirtualTool tool = PlayModeTestUtilities.GetFakeController();

            //PlayModeTestUtilities.GetFakeInputSource();

            InputEventData fakeData = PlayModeTestUtilities.GetFakeInputEventData(setupService.toolSetup.menuAction, new Vector3(0,0,0));
            pieMenuManager.MenuOpen(fakeData);

            yield return null;

            //Try to find the menu
            var menu = GameObject.FindObjectOfType<PieMenuRenderer>();
            Assert.IsNotNull(menu);
        }

        
    }
}
