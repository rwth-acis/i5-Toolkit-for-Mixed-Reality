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

        [SetUp]
        public void setup()
        {
            core = new PieMenuManagerCore();
            shell = A.Fake<IPieMenuManagerShell>();
            core.shell = shell;
            PieMenuSetup toolSetup = new PieMenuSetup {menuAction = menuAction };
            toolSetupService = new ToolSetupService(toolSetup);
        }

        [Test]
        public void Can_open_menu_with_menu_action_and_not_open_already()
        {
            Vector3 controllerPosition = new Vector3(4,5,3);
            InputEventData eventData = EditorTestUtilitys.GetFakeInputEventData(menuAction, controllerPosition);
            IMixedRealityInputSource invokingSource = A.Fake<IMixedRealityInputSource>();

            core.MenuOpen(eventData, false, toolSetupService, ref invokingSource);

            A.CallTo(() => shell.instantiatePieMenu(new Vector3(), new Quaternion(), A.Fake<IMixedRealityPointer>())).WhenArgumentsMatch(
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
            Vector3 controllerPosition = new Vector3(4, 5, 3);
            InputEventData eventData = EditorTestUtilitys.GetFakeInputEventData(menuAction, controllerPosition);
            IMixedRealityInputSource invokingSource = null;

            core.MenuOpen(eventData, true, toolSetupService, ref invokingSource);

            A.CallTo(() => shell.instantiatePieMenu(new Vector3(), new Quaternion(), A.Fake<IMixedRealityPointer>())).MustNotHaveHappened();

            //The invoking source should not be altered
            Assert.IsNull(invokingSource);
        }
    }

}