using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.MixedReality.PieMenu;
using NUnit.Framework;
using FakeItEasy;
using Microsoft.MixedReality.Toolkit.Input;

namespace i5.Toolkit.MixedReality.Tests.PieMenu
{
    public class ViveWandTeleporterTest
    {
        ViveWandTeleporterCore core = new ViveWandTeleporterCore();

        [Test]
        public void SetupToolTest()
        {
            IViveWandShell shell = A.Fake<IViveWandShell>();
            core.shell = shell;
            core.SetupTool();
            //The description texts must be activated
            A.CallTo(() => shell.SetGameObjectActive("", false)).WhenArgumentsMatch(args =>
                                                                                    args.Get<string>("key") == "ButtonDescriptions" &&
                                                                                    args.Get<bool>("active")).
                                                                                    MustHaveHappenedOnceExactly();
            //The text for grip description must set
            A.CallTo(() => shell.AddGameobjectToBuffer("", "")).WhenArgumentsMatch(args =>
                                                                                   args.Get<string>("name") == "ButtonDescriptions/GripText" &&
                                                                                   args.Get<string>("key") == "GripText").
                                                                                   MustHaveHappenedOnceExactly();
            A.CallTo(() => shell.SetGameObjectActive("", false)).WhenArgumentsMatch(args =>
                                                                                    args.Get<string>("key") == "GripText" &&
                                                                                    args.Get<bool>("active")).
                                                                                    MustHaveHappenedOnceExactly();
            A.CallTo(() => shell.SetTMPText("", "")).WhenArgumentsMatch(args =>
                                                                        args.Get<string>("key") == "GripText" &&
                                                                        args.Get<string>("text") != "").
                                                                        MustHaveHappenedOnceExactly();

            //The coroutine for deactivating the description texts again must be called
            A.CallTo(() => shell.DisableDescriptionTextCoroutine(false)).WhenArgumentsMatch(args =>
                                                                                            args.Get<bool>("start")).
                                                                                            MustHaveHappenedOnceExactly();
        }
    }
}
