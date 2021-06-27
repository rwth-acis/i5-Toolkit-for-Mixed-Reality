using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.MixedReality.PieMenu;
using NUnit.Framework;
using FakeItEasy;
using Microsoft.MixedReality.Toolkit.Input;

namespace i5.Toolkit.MixedReality.Tests.PieMenu
{

    public class PieMenuManagerTests
    {
        PieMenuManagerCore core;
        IPieMenuManagerShell shell;
        ToolSetupService toolSetupService;
        MixedRealityInputAction menuAction;
        Vector3 controllerPosition;

        [SetUp]
        public void setup()
        {
            core = new PieMenuManagerCore();
            shell = A.Fake<IPieMenuManagerShell>();
            core.shell = shell;
            menuAction = new MixedRealityInputAction(1,"Menu action");
            PieMenuSetup toolSetup = new PieMenuSetup {menuAction = menuAction };
            toolSetupService = new ToolSetupService(toolSetup);
            controllerPosition = new Vector3(4, 5, 3);
            toolSetup.menuEntries = new List<MenuEntry>();
            for (int i = 0; i < 3; i++)
            {
                toolSetup.menuEntries.Add(new MenuEntry { toolSettings = new GeneralToolSettings { toolName = "Test Tool " + i } });
            }
            
        }

        #region Menu Open
        [Test]
        public void Can_open_menu_with_menu_action_and_not_open_already()
        {
            InputEventData eventData = EditorTestUtilitys.GetFakeInputEventData(menuAction, controllerPosition);
            IMixedRealityInputSource invokingSource = A.Fake<IMixedRealityInputSource>();

            core.MenuOpen(eventData, false, toolSetupService, ref invokingSource);

            A.CallTo(() => shell.InstantiatePieMenu(new Vector3(), new Quaternion(), A.Fake<IMixedRealityPointer>())).WhenArgumentsMatch(
                args =>
                //The menu sould spawn at the controller position
                args.Get<Vector3>("position") == controllerPosition &&
                //The pointer should be the one from the event
                args.Get<IMixedRealityPointer>("pointer") == eventData.InputSource.Pointers[0]
                ).
                //Only one menu should spawn
                MustHaveHappenedOnceExactly();
            
            //The invoking source should be set to the one, that invoked the event
            Assert.AreEqual(invokingSource, eventData.InputSource);
        }

        [Test]
        public void Cannot_open_menu_with_menu_action_and_open_already()
        {
            InputEventData eventData = EditorTestUtilitys.GetFakeInputEventData(menuAction, controllerPosition);
            IMixedRealityInputSource invokingSource = null;

            core.MenuOpen(eventData, true, toolSetupService, ref invokingSource);

            A.CallTo(() => shell.InstantiatePieMenu(new Vector3(), new Quaternion(), A.Fake<IMixedRealityPointer>())).MustNotHaveHappened();

            //The invoking source should not be altered
            Assert.IsNull(invokingSource);
        }

        [Test]
        public void Cannot_open_menu_with_other_action()
        {
            InputEventData eventData = EditorTestUtilitys.GetFakeInputEventData(new MixedRealityInputAction(), controllerPosition);
            IMixedRealityInputSource invokingSource = null;

            core.MenuOpen(eventData, false, toolSetupService, ref invokingSource);

            A.CallTo(() => shell.InstantiatePieMenu(new Vector3(), new Quaternion(), A.Fake<IMixedRealityPointer>())).MustNotHaveHappened();

            //The invoking source should not be altered
            Assert.IsNull(invokingSource);
        }

        #endregion
        #region Menu Close
        [Test]
        public void Can_close_menu_with_menu_action_and_menu_open()
        {

            for (int i = 0; i < 3; i++)
            {
                InputEventData eventData = EditorTestUtilitys.GetFakeInputEventData(menuAction, controllerPosition);
                IMixedRealityInputSource invokingSource = eventData.InputSource;
                core.MenuClose(eventData, true, toolSetupService, i, ref invokingSource);
                A.CallTo(() => shell.SetupTool(new MenuEntry(), null)).WhenArgumentsMatch
                (
                args =>
                args.Get<MenuEntry>("currentEntry").Equals(toolSetupService.toolSetup.menuEntries[i])
                ).MustHaveHappenedOnceExactly();

                //The invoking source should be null, because the menu is now closed
                Assert.IsNull(invokingSource);
            }

            A.CallTo(() => shell.DestroyPieMenu()).MustHaveHappened(3, Times.Exactly);

        }

        [Test]
        public void Cannot_close_menu_with_wrong_inputsource()
        {
            InputEventData eventData = EditorTestUtilitys.GetFakeInputEventData(menuAction, controllerPosition);
            IMixedRealityInputSource invokingSource = A.Fake<IMixedRealityInputSource>();
            IMixedRealityInputSource invokingSourceCopy = invokingSource;

            core.MenuClose(eventData, true, toolSetupService, 0, ref invokingSource);

            A.CallTo(() => shell.SetupTool(new MenuEntry(), null)).MustNotHaveHappened();

            A.CallTo(() => shell.DestroyPieMenu()).MustNotHaveHappened();

            //The invoking source should be null, because the menu is now closed
            Assert.AreEqual(invokingSource, invokingSourceCopy);
        }

        [Test]
        public void Cannot_close_menu_when_not_open()
        {
            InputEventData eventData = EditorTestUtilitys.GetFakeInputEventData(menuAction, controllerPosition);
            IMixedRealityInputSource invokingSource = eventData.InputSource;
            IMixedRealityInputSource invokingSourceCopy = invokingSource;

            core.MenuClose(eventData, false, toolSetupService, 0, ref invokingSource);

            A.CallTo(() => shell.SetupTool(new MenuEntry(), null)).MustNotHaveHappened();

            A.CallTo(() => shell.DestroyPieMenu()).MustNotHaveHappened();

            //The invoking source should be null, because the menu is now closed
            Assert.AreEqual(invokingSource, invokingSourceCopy);
        }
        #endregion
    }

}