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

        void RenderSetup(int numberOfEntries)
        {
            toolSetup.pieMenuPieceNormalColor = Color.blue;
            for (int i = 0; i < numberOfEntries; i++)
            {
                GeneralToolSettings toolSettings = new GeneralToolSettings { toolName = "Test Tool " + i };
                toolSettings.iconTool = Sprite.Create(new Texture2D(0,0),Rect.zero,Vector2.zero);
                toolSetup.menuEntries.Add(new MenuEntry { toolSettings = toolSettings });
            }

            core = new PieMenuRendererCore(toolSetup, shell, ref currentlyHighlighted);

            //Pie menu has to be directed to the camera
            A.CallTo(() => shell.LookAtCamera()).MustHaveHappenedOnceExactly();

            //numberOfEntries many PieMenu Pieces have to be instantiated
            A.CallTo(() => shell.InstatiatePieceAndAddToList()).MustHaveHappened(numberOfEntries, Times.Exactly);

            //Every piece should have the normal color at the beginning, because nothing should be higlighted at the moment
            for (int i = 0; i < numberOfEntries; i++)
            {
                A.CallTo(() => shell.SetColorForPiece(0,Color.black)).WhenArgumentsMatch(
                    args =>
                    args.Get<int>("id") == i &&
                    args.Get<Color>("color") == toolSetup.pieMenuPieceNormalColor).
                    MustHaveHappenedOnceExactly();
            }

            //Every piece from id 0 to numberOfEntries needs to get assigned the icon with the same id
            for (int i = 0; i < numberOfEntries; i++)
            {
                A.CallTo(() => shell.SetIconForPiece(0,null)).WhenArgumentsMatch(
                    args =>
                    args.Get<int>("id") == i &&
                    args.Get<Sprite>("sprite") == toolSetup.menuEntries[i].toolSettings.iconTool).
                    MustHaveHappenedOnceExactly();
            }


        }

        [Test]
        public void Render_Setup_With_0_Entry()
        {
            RenderSetup(0);
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
