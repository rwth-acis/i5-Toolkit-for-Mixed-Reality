using UnityEngine;
using i5.Toolkit.MixedReality.PieMenu;
using NUnit.Framework;
using FakeItEasy;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Microsoft.MixedReality.Toolkit.Utilities;

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

            var test = (IViveWandToolShell)core.shell;

            setup = new PieMenuSetup();
            A.CallTo(() => shell.GetPieMenuSetup()).Returns(setup);
        }

        private void CheckSetTextShellCallbacks(string key, string testText)
        {
            //TextObject must be set active
            A.CallTo(() => shell.SetGameObjectActive("", false)).WhenArgumentsMatch(args =>
                                                                                    args.Get<string>("key") == key &&
                                                                                    args.Get<bool>("active")).
                                                                                    MustHaveHappenedOnceExactly();
            //Only one SetTextCall for the TextField
            A.CallTo(() => shell.SetTMPText("", "")).WhenArgumentsMatch(args => args.Get<string>("key") == key).
                                                                        MustHaveHappenedOnceExactly();
            //The only SetTextCall for the TextField has to set the text to the testText
            A.CallTo(() => shell.SetTMPText("", "")).WhenArgumentsMatch(args => args.Get<string>("key") == key && 
                                                                                args.Get<string>("text") == testText).
                                                                                MustHaveHappenedOnceExactly();
        }

        readonly string[] keys = { "TouchpadRightText", "TouchpadDownText", "TouchpadLeftText", "TouchpadUpText", "TriggerText", "GripText" };

        [Test]
        public void Setup_tool_new_text_no_default_text()
        {
            
            MenuEntry newEntry = new MenuEntry();
            string testText = "ThisIsATestText";

            newEntry.touchpadRightSettings.textTouchpadRight = testText + keys[0];
            newEntry.TouchpadDownSettings.textTouchpadDown = testText + keys[1];
            newEntry.touchpadLeftSettings.textTouchpadLeft = testText + keys[2];
            newEntry.touchpadUpSettings.textTouchpadUp = testText + keys[3];
            newEntry.triggerSettings.textTrigger = testText + keys[4];
            newEntry.gripSettings.textGrip = testText + keys[5];

            core.SetupTool(newEntry);
            foreach (string key in keys)
            {
                CheckSetTextShellCallbacks(key, testText + key);
            }
        }

        [Test]
        public void Setup_tool_no_text()
        {
            core.SetupTool(new MenuEntry());
            foreach (string key in keys)
            {
                //Because the text in new Entry is empty and in the default entry also, the textGameObject must be disabled
                A.CallTo(() => shell.SetGameObjectActive("",false)).WhenArgumentsMatch(args => args.Get<string>("key") == key && 
                                                                                              !args.Get<bool>("active"))
                                                                                              .MustHaveHappenedOnceExactly();
            }
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

        struct hoverEvents
        {
            public VirtualToolFocusEvent focusEventStart;
            public VirtualToolFocusEvent focusEventActive;
            public VirtualToolFocusEvent focusEventStop;
        }

        private hoverEvents CreateHoverEvents()
        {
            hoverEvents events = new hoverEvents();
            MenuEntry entry = new MenuEntry();

            events.focusEventStart = createEntryWithHoverEvent();
            entry.toolSpecificevents.OnHoverOverTargetStart = events.focusEventStart;

            events.focusEventActive = createEntryWithHoverEvent();
            entry.toolSpecificevents.OnHoverOverTargetActive = events.focusEventActive;

            events.focusEventStop = createEntryWithHoverEvent();
            entry.toolSpecificevents.OnHoverOverTargetStop = events.focusEventStop;

            A.CallTo(() => shell.currentEntry).Returns(entry);

            return events;
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

            hoverEvents Events = CreateHoverEvents();

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

            hoverEvents Events = CreateHoverEvents();

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

            hoverEvents Events = CreateHoverEvents();

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

            hoverEvents Events = CreateHoverEvents();

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

            hoverEvents Events = CreateHoverEvents();

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

        //Touchpad tests

        struct TouchpadEvents
        {
            public InputActionUnityEvent touchpadUp;
            public InputActionUnityEvent touchpadRight;
            public InputActionUnityEvent touchpadDown;
            public InputActionUnityEvent touchpadLeft;
        }

        private InputActionUnityEvent createEntryWithInputEvent()
        {
            InputActionUnityEvent inputEvent = A.Fake<InputActionUnityEvent>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(inputEvent);
            return inputEvent;
        }

        private TouchpadEvents CreateTouchpadEvents()
        {
            TouchpadEvents events = new TouchpadEvents();
            MenuEntry entry = new MenuEntry();

            events.touchpadUp = createEntryWithInputEvent();
            entry.touchpadUpSettings.OnInputActionEndedTouchpadUp = events.touchpadUp;

            events.touchpadRight = createEntryWithInputEvent();
            entry.touchpadRightSettings.OnInputActionEndedTouchpadRight = events.touchpadRight;

            events.touchpadDown = createEntryWithInputEvent();
            entry.TouchpadDownSettings.OnInputActionEndedTouchpadDown = events.touchpadDown;

            events.touchpadLeft = createEntryWithInputEvent();
            entry.touchpadLeftSettings.OnInputActionEndedTouchpadLeft = events.touchpadLeft;

            A.CallTo(() => shell.currentEntry).Returns(entry);

            return events;
        }

        private TouchpadEvents SetUpForTouchpad(Vector2 thumbPosition)
        {
            A.CallTo(() => shell.thumbPosition).Returns(thumbPosition);
            MixedRealityInputAction action = new MixedRealityInputAction(1, "touchpadAction", AxisType.Digital);
            setup.touchpadPressAction = action;
            InputEventData eventData = EditorTestUtilitys.GetFakeInputEventData(action, Vector3.zero);
            core.ownSource = eventData.InputSource;

            TouchpadEvents touchpadEvents = CreateTouchpadEvents();

            core.OnActionEnded(eventData);

            return touchpadEvents;
        }

        [Test]
        public void Touchpad_up_test()
        {
            TouchpadEvents touchpadEvents = SetUpForTouchpad(Vector2.up);

            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadUp).
                                                                                             MustHaveHappenedOnceExactly();
            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadRight).
                                                                                             MustNotHaveHappened();
            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadDown).
                                                                                             MustNotHaveHappened();
            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadLeft).
                                                                                             MustNotHaveHappened();
        }

        [Test]
        public void Touchpad_right_test()
        {
            TouchpadEvents touchpadEvents = SetUpForTouchpad(Vector2.right);

            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadUp).
                                                                                             MustNotHaveHappened();
            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadRight).
                                                                                             MustHaveHappenedOnceExactly();
            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadDown).
                                                                                             MustNotHaveHappened();
            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadLeft).
                                                                                             MustNotHaveHappened();
        }

        [Test]
        public void Touchpad_down_test()
        {
            TouchpadEvents touchpadEvents = SetUpForTouchpad(Vector2.down);

            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadUp).
                                                                                             MustNotHaveHappened();
            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadRight).
                                                                                             MustNotHaveHappened();
            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadDown).
                                                                                             MustHaveHappenedOnceExactly();
            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadLeft).
                                                                                             MustNotHaveHappened();
        }

        [Test]
        public void Touchpad_left_test()
        {
            TouchpadEvents touchpadEvents = SetUpForTouchpad(Vector2.left);

            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadUp).
                                                                                             MustNotHaveHappened();
            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadRight).
                                                                                             MustNotHaveHappened();
            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadDown).
                                                                                             MustNotHaveHappened();
            A.CallTo(() => shell.InvokeEvent<BaseInputEventData>(null, null)).WhenArgumentsMatch(args =>
                                                                                             args.Get<UnityEvent<BaseInputEventData>>("inputEvent") == touchpadEvents.touchpadLeft).
                                                                                             MustHaveHappenedOnceExactly();
        }
    }
}
