using UnityEngine;
using i5.Toolkit.MixedReality.PieMenu;
using NUnit.Framework;
using FakeItEasy;
using Microsoft.MixedReality.Toolkit.Input;

namespace i5.Toolkit.MixedReality.Tests.PieMenu
{
    public class VieveWandToolTest
    {
        [Test]
        public void TriggerTest()
        {
            IViveWandToolShell shell = A.Fake<IViveWandToolShell>();
            ViveWandToolCore core = new ViveWandToolCore();
            core.shell = shell;
            MixedRealityInputAction action = new MixedRealityInputAction();
            InputEventData eventData = EditorTestUtilitys.GetFakeInputEventData(action,Vector3.zero);

            PieMenuSetup setup = new PieMenuSetup();
            setup.triggerInputAction = action;
            core.ownSource = eventData.InputSource;

            InputActionUnityEvent inputEvent = A.Fake<InputActionUnityEvent>();
            MenuEntry currentEntry = new MenuEntry();
            currentEntry.triggerSettings.OnInputActionStartedTrigger = inputEvent;
            A.CallTo(() => shell.currentEntry).Returns(currentEntry);

            A.CallTo(() => shell.GetToolSetup()).Returns(setup);

            core.OnActionStarted(eventData);

            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                             args.Get<InputActionUnityEvent>("inputEvent") == inputEvent &&
                                                                             args.Get<BaseInputEventData>("eventData") == eventData).MustHaveHappenedOnceExactly();
        }
    }
}
