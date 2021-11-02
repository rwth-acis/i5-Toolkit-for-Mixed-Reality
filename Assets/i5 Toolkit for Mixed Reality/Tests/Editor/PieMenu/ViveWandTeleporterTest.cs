using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.MixedReality.PieMenu;
using NUnit.Framework;
using FakeItEasy;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.EventSystems;

namespace i5.Toolkit.MixedReality.Tests.PieMenu
{
    public class ViveWandTeleporterTest
    {
        ViveWandTeleporterCore core = new ViveWandTeleporterCore();

        [Test]
        public void Setup_tool_new_grip_text()
        {
            IViveWandShell shell = A.Fake<IViveWandShell>();
            string testText = "ThisIsATestText";



            PieMenuSetup setup = A.Fake<PieMenuSetup>();
            setup.defaultEntryTeleporter.gripSettings.textGrip = testText;


            core.shell = shell;

            A.CallTo(() => core.shell.GetPieMenuSetup()).Returns(setup);
            core.SetupTool();
            

            string key = "GripText";

            //The text for grip description must set
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


        [Test]
        public void Setup_tool_text_object_activation()
        {
            IViveWandShell shell = A.Fake<IViveWandShell>();


            //Link the fake shell with the vive wand core
            A.CallTo(() => shell.DisableDescriptionTextCoroutine(false)).WhenArgumentsMatch(args => args.Get<bool>("start")).
                                                                         Invokes(() =>
                                                                         {
                                                                             IEnumerator enumerator = core.DisableDescriptionsAfterShowTime();
                                                                             while (enumerator.MoveNext()) ; //Needs to iterate over MoveNext() of the IEnumerator, otherwise the code inside isn't executed
                                                                         });

            core.shell = shell;
            core.SetupTool();
            //The description texts must be activated
            A.CallTo(() => shell.SetGameObjectActive("", false)).WhenArgumentsMatch(args =>
                                                                                    args.Get<string>("key") == "ButtonDescriptions" &&
                                                                                    args.Get<bool>("active")).
                                                                                    MustHaveHappenedOnceExactly();

            //The coroutine for deactivating the description texts again must be called
            A.CallTo(() => shell.SetGameObjectActive("", false)).WhenArgumentsMatch(args => args.Get<string>("key") == "ButtonDescriptions" &&
                                                                                            !args.Get<bool>("active")).
                                                                                            MustHaveHappenedOnceExactly();
        }

        [Test]
        public void Set_own_source_test()
        {
            IViveWandShell shell = A.Fake<IViveWandShell>();
            HashSet<IMixedRealityInputSource> inputSources = new HashSet<IMixedRealityInputSource>();
            A.CallTo(() => shell.GetInputSources()).Returns(inputSources);
            core.shell = shell;

            //Setup inputsource that is not the one from the vive wand
            IMixedRealityInputSource otherSource = A.Fake<IMixedRealityInputSource>();
            IMixedRealityPointer[] pointerArray = new IMixedRealityPointer[1];
            pointerArray[0] = A.Fake<IMixedRealityPointer>();
            A.CallTo(() => otherSource.Pointers).Returns(pointerArray);

            inputSources.Add(otherSource);

            //Setup inputsource form the vive wand
            IMixedRealityInputSource ownSource = A.Fake<IMixedRealityInputSource>();
            pointerArray = new IMixedRealityPointer[1];
            pointerArray[0] = A.Fake<IMixedRealityPointer>();
            A.CallTo(() => ownSource.Pointers).Returns(pointerArray);

            A.CallTo(() => shell.GameObjectProxyEqualsOwnObject(null)).WhenArgumentsMatch(args =>
                                                                                          args.Get<IMixedRealityControllerVisualizer>("visualizer") == ownSource.Pointers[0].Controller.Visualizer).
                                                                                          Returns(true);
            
            //First try to set the input source, before the own source is avaible
            IEnumerator enumerator = core.SetOwnSource();
            enumerator.MoveNext();
            //This must not yield a result
            Assert.IsNull(core.ownSource);

            inputSources.Add(ownSource);





            //Next try, after the own input source was added. This time, the inputsource should be set correctly
            enumerator.MoveNext();
            enumerator.MoveNext();
            Assert.IsTrue(core.ownSource == ownSource);
        }

    }
}
