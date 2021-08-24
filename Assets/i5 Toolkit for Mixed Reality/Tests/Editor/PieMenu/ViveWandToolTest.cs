using UnityEngine;
using i5.Toolkit.MixedReality.PieMenu;
using NUnit.Framework;
using FakeItEasy;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.Events;

namespace i5.Toolkit.MixedReality.Tests.PieMenu
{
    public class VieveWandToolTest
    {
        IViveWandToolShell shell;
        ViveWandToolCore core;
        PieMenuSetup setup;
        MenuEntry currentEntry;

        [SetUp]
        public void SetUp()
        {
            shell = A.Fake<IViveWandToolShell>();
            core = new ViveWandToolCore();
            core.shell = shell;

            setup = new PieMenuSetup();
            A.CallTo(() => shell.GetToolSetup()).Returns(setup);
        }

        [Test]
        public void TriggerTest()
        {

            MixedRealityInputAction action = new MixedRealityInputAction();       
            setup.triggerInputAction = action;
            
            InputActionUnityEvent inputEvent = A.Fake<InputActionUnityEvent>();
            //Add a persitient listener, to emulate a currentEntry with actions assigned to the trigger events
            UnityEditor.Events.UnityEventTools.AddPersistentListener(inputEvent);
            MenuEntry currentEntry = new MenuEntry();     
            currentEntry.triggerSettings.OnInputActionStartedTrigger = inputEvent;
            A.CallTo(() => shell.currentEntry).Returns(currentEntry);

            InputEventData eventData = EditorTestUtilitys.GetFakeInputEventData(action, Vector3.zero);
            core.ownSource = eventData.InputSource;
            core.OnActionStarted(eventData);

            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == inputEvent &&
                                                                             args.Get<BaseInputEventData>("eventData") == eventData).MustHaveHappenedOnceExactly();
        }


        //Hover event tests

        private VirtualToolFocusEvent createEntryWithHoverEvent()
        {
            VirtualToolFocusEvent focusEvent = A.Fake<VirtualToolFocusEvent>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(focusEvent);
            return focusEvent;
        }

        struct events
        {
            public VirtualToolFocusEvent focusEventStart;
            public VirtualToolFocusEvent focusEventActive;
            public VirtualToolFocusEvent focusEventStop;
        }

        private events CreateEvents()
        {
            events Events = new events();
            //Setup OnHoverStart
            MenuEntry entry = new MenuEntry();

            Events.focusEventStart = createEntryWithHoverEvent();
            entry.toolSpecificevents.OnHoverOverTargetStart = Events.focusEventStart;

            Events.focusEventActive = createEntryWithHoverEvent();
            entry.toolSpecificevents.OnHoverOverTargetActive = Events.focusEventActive;

            Events.focusEventStop = createEntryWithHoverEvent();
            entry.toolSpecificevents.OnHoverOverTargetStop = Events.focusEventStop;

            A.CallTo(() => shell.currentEntry).Returns(entry);

            return Events;
        }

        private void SetCallbackFunctionsReturnValues(bool oldFocusTargetIsNull, bool targetIsNull, bool targetEqualsOldTarget)
        {
            A.CallTo(() => shell.OldFocusTargetIsNull()).Returns(oldFocusTargetIsNull);
            A.CallTo(() => shell.TargetIsNull()).Returns(targetIsNull);
            A.CallTo(() => shell.TargetEqualsOldTarget()).Returns(targetEqualsOldTarget);
        }

        [Test]
        public void Hover_from_nothing_to_target_test()
        {
            core.ownSource = EditorTestUtilitys.GetFakeInputSource(Vector3.zero);
            SetCallbackFunctionsReturnValues(true,false,false);

            events Events = CreateEvents();

            core.UpdateHoverEvents();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventStart).
                                                                                             MustHaveHappenedOnceExactly();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventActive).
                                                                                             MustHaveHappenedOnceExactly();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventStop).
                                                                                             MustNotHaveHappened();
        }

        [Test]
        public void Hover_from_targt_to_nothing_test()
        {
            core.ownSource = EditorTestUtilitys.GetFakeInputSource(Vector3.zero);
            SetCallbackFunctionsReturnValues(false, true, false);

            events Events = CreateEvents();

            core.UpdateHoverEvents();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventStart).
                                                                                             MustNotHaveHappened();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventActive).
                                                                                             MustNotHaveHappened();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventStop).
                                                                                             MustHaveHappenedOnceExactly();
        }

        [Test]
        public void Hover_on_same_target_test()
        {
            core.ownSource = EditorTestUtilitys.GetFakeInputSource(Vector3.zero);
            SetCallbackFunctionsReturnValues(false, false, true);

            events Events = CreateEvents();

            core.UpdateHoverEvents();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventStart).
                                                                                             MustNotHaveHappened();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventActive).
                                                                                             MustHaveHappenedOnceExactly();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventStop).
                                                                                             MustNotHaveHappened();
        }

        [Test]
        public void Hover_on_nothing_test()
        {
            core.ownSource = EditorTestUtilitys.GetFakeInputSource(Vector3.zero);
            SetCallbackFunctionsReturnValues(true, true, true);

            events Events = CreateEvents();

            core.UpdateHoverEvents();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventStart).
                                                                                             MustNotHaveHappened();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventActive).
                                                                                             MustNotHaveHappened();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventStop).
                                                                                             MustNotHaveHappened();
        }

        [Test]
        public void Hover_from_target_to_other_target()
        {
            core.ownSource = EditorTestUtilitys.GetFakeInputSource(Vector3.zero);
            SetCallbackFunctionsReturnValues(false, false, false);

            events Events = CreateEvents();

            core.UpdateHoverEvents();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventStart).
                                                                                             MustHaveHappenedOnceExactly();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventActive).
                                                                                             MustHaveHappenedOnceExactly();

            A.CallTo(() => shell.InvokeEvent<FocusEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<FocusEventData>>("inputEvent") == Events.focusEventStop).
                                                                                             MustHaveHappenedOnceExactly();
        }
    }
}
