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
        public IEnumerator Can_spawn_PieMenu_0_entrys()
        {
            yield return null;

            SpawnPieMenu();

            yield return null;

            //Try to find the menu
            var menu = GameObject.FindObjectOfType<PieMenuRenderer>();
            Assert.IsNotNull(menu, "PieMenu not found");
        }

        [UnityTest]
        public IEnumerator Can_spawn_PieMenu_1_entrys()
        {
            yield return null;

            AddEmptyMenuEntry(1);

            SpawnPieMenu();

            yield return null;

            //Try to find the menu
            var menu = GameObject.FindObjectOfType<PieMenuRenderer>();
            Assert.IsNotNull(menu);

            if (menu != null)
            {
                CheckPieMenu(1,menu);
            }
        }

        [UnityTest]
        public IEnumerator Can_spawn_PieMenu_2_entrys()
        {
            yield return null;

            AddEmptyMenuEntry(2);

            SpawnPieMenu();

            yield return null;

            //Try to find the menu
            var menu = GameObject.FindObjectOfType<PieMenuRenderer>();
            Assert.IsNotNull(menu);

            if (menu != null)
            {
                CheckPieMenu(2, menu);
            }
        }

        [UnityTest]
        public IEnumerator Can_select_tool()
        {
            yield return null;

            AddEmptyMenuEntry(3);

            //Pie
        }

        private static void CheckPieMenu(int expectedMenuEntrys, PieMenuRenderer menu)
        {
            Assert.AreEqual(expectedMenuEntrys, menu.menuEntries.Count, "Menu uses " + menu.menuEntries.Count + " entries, but should use " + expectedMenuEntrys);
            Assert.AreEqual(expectedMenuEntrys, menu.pieMenuPieces.Count, "Menu generated " + menu.pieMenuPieces.Count + " pieces, but should have generated" + expectedMenuEntrys);
        }



        private void SpawnPieMenu()
        {
            setupService = ServiceManager.GetService<ToolSetupService>();
            var pieMenuManager = GameObject.FindObjectOfType<PieMenuManager>();

            InputEventData fakeData = PlayModeTestUtilities.GetFakeInputEventData(setupService.toolSetup.menuAction, new Vector3(0, 0, 0));
            pieMenuManager.MenuOpen(fakeData);
        }

        private static void AddEmptyMenuEntry(int count)
        {
            PieMenuSetup setupInformation = ServiceManager.GetService<ToolSetupService>().toolSetup;
            for (int i = 0; i < count; i++)
            {
                MenuEntry entry = new MenuEntry
                {
                    toolSettings = new GeneralToolSettings
                    {
                        toolName = i.ToString()
                    }
                };
                setupInformation.menuEntries.Add(entry);
            }
        }

    }
}
