using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using i5.Toolkit.MixedReality.PieMenu;
using NUnit.Framework;
using FakeItEasy;
using Microsoft.MixedReality.Toolkit.Input;

namespace i5.Toolkit.MixedReality.Tests.PieMenu
{

    public class PieMenuRendererTests
    {
        PieMenuRendererCore core;
        IPieMenuRendererShell shell;
        PieMenuSetup toolSetup;
        int currentlyHighlighted;
        Transform shellTransform;

        [SetUp]
        public void Setup()
        {
            shell = A.Fake<IPieMenuRendererShell>();
            toolSetup = new PieMenuSetup();
            toolSetup.menuEntries = new List<MenuEntry>();
            shellTransform = A.Fake<Transform>();
        }

        void RenderSetup(int numberOfEntrys)
        {
            for (int i = 0; i < numberOfEntrys; i++)
            {
                toolSetup.menuEntries.Add(new MenuEntry { toolSettings = new GeneralToolSettings { toolName = "Test Tool " + i } });
            }

            core = new PieMenuRendererCore(toolSetup, shell, ref currentlyHighlighted);

            A.CallTo(() => shell.LookAtCamera()).MustHaveHappenedOnceExactly();
            //A.CallTo(() => shell.)
        }

        [Test]
        public void Render_Setup_With_1_Entry()
        {
            RenderSetup(1);
        }

        [Test]
        public void Render_Setup_With_2_Entries()
        {
            RenderSetup(2);
        }

        [Test]
        public void Render_Setup_With_3_Entries()
        {
            RenderSetup(3);
        }

        [Test]
        public void Render_Setup_With_100_Entries()
        {
            RenderSetup(100);
        }
    }
}
