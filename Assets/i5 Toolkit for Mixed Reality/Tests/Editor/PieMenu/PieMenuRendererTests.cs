﻿using System.Collections;
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
        Transform shellTransform;

        [SetUp]
        public void Setup()
        {
            shell = A.Fake<IPieMenuRendererShell>();
            toolSetup = new PieMenuSetup();
            toolSetup.menuEntries = new List<MenuEntry>();
            toolSetup.pieMenuPieceNormalColor = Color.blue;
            toolSetup.pieMenuPieceHighlighColor = Color.red;
            shellTransform = A.Fake<Transform>();
        }

        void SetupToolSettings(int numberOfEntries)
        {
            
            for (int i = 0; i < numberOfEntries; i++)
            {
                GeneralToolSettings toolSettings = new GeneralToolSettings { toolName = "Test Tool " + i };
                toolSettings.iconTool = Sprite.Create(new Texture2D(0, 0), Rect.zero, Vector2.zero);
                toolSetup.menuEntries.Add(new MenuEntry { toolSettings = toolSettings });
            }
        }

        //Spawn tests

        void RenderSetup(int numberOfEntries)
        {
            SetupToolSettings(numberOfEntries);
            int currentlyHighlighted = 0;
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

        //Update Tests
        void UpdateCore(int numberOfEntries, Vector3 localPositionOfCoursor, int expectedPieceToHiglight)
        {
            for (int currentlyHighlightedItterator = 0; currentlyHighlightedItterator < numberOfEntries || (numberOfEntries == 0 && currentlyHighlightedItterator ==0); currentlyHighlightedItterator++)
            {
                int currentlyHighlighted = currentlyHighlightedItterator;
                int previosulyHiglighted = currentlyHighlighted;
                int higlhightDummy = 0;
                core = new PieMenuRendererCore(toolSetup, shell, ref higlhightDummy);
                Fake.ClearRecordedCalls(shell);

                A.CallTo(() => shell.GetLocalPositionOfCursor()).Returns(localPositionOfCoursor);
                A.CallTo(() => shell.GetLocalScaleOfPiece(0)).Returns(new Vector3(1, 1, 1));
                A.CallTo(() => shell.GetLocalScaleOfPiece(0)).WhenArgumentsMatch(args => args.Get<int>("id") == previosulyHiglighted).Returns(new Vector3(1.2f, 1.2f, 1));

                Vector3 test = shell.GetLocalScaleOfPiece(currentlyHighlighted);
                Vector3 test2 = shell.GetLocalScaleOfPiece(0);

                //The pointer position doesn't matter here, since the call to GetLocalPositionOfCursor is intercepted
                core.Update(EditorTestUtilitys.GetFakeInputSource(Vector3.zero).Pointers[0], toolSetup, ref currentlyHighlighted);

                Assert.AreEqual(currentlyHighlighted, expectedPieceToHiglight,"The piece with number " + currentlyHighlighted + " was higlighted, but number " + expectedPieceToHiglight + " was expected.");

                //If the same piece should be highlighted, nothing on the pieces should be changed
                if (previosulyHiglighted == expectedPieceToHiglight)
                {
                    for (int i = 0; i < numberOfEntries; i++)
                    {
                        A.CallTo(() => shell.SetColorForPiece(0, Color.black)).WhenArgumentsMatch(
                                args =>
                                args.Get<int>("id") == i).
                                MustNotHaveHappened();

                        A.CallTo(() => shell.SetLocalScaleOfPiece(0, Vector3.zero)).WhenArgumentsMatch(
                            args =>
                            args.Get<int>("id") == i).
                            MustNotHaveHappened();
                    }
                }
                else
                {
                    for (int i = 0; i < numberOfEntries; i++)
                    {
                        //The previously highlighted piece should be dehighlighted
                        if (i == previosulyHiglighted)
                        {
                            A.CallTo(() => shell.SetColorForPiece(0, Color.black)).WhenArgumentsMatch(
                                args =>
                                args.Get<int>("id") == i &&
                                args.Get<Color>("color") == toolSetup.pieMenuPieceNormalColor).
                                MustHaveHappenedOnceExactly();
                            //Previously highlighted should be scaled by 1/1.2 in x and y direction, in order to reverse the enlagment from highlighting
                            A.CallTo(() => shell.SetLocalScaleOfPiece(0, Vector3.zero)).WhenArgumentsMatch(
                                args =>
                                args.Get<int>("id") == i &&
                                args.Get<Vector3>("scale") == Vector3.Scale(shell.GetLocalScaleOfPiece(i), new Vector3(1 / 1.2f, 1 / 1.2f, 1))).
                                MustHaveHappenedOnceExactly();
                        }
                        //The piece to highlight should be highlighted
                        else if (i == expectedPieceToHiglight)
                        {
                            A.CallTo(() => shell.SetColorForPiece(0, Color.black)).WhenArgumentsMatch(
                                args =>
                                args.Get<int>("id") == i &&
                                args.Get<Color>("color") == toolSetup.pieMenuPieceHighlighColor).
                                MustHaveHappenedOnceExactly();
                            //New higlighted piece should be scaled by 1.2 in x and y direction
                            A.CallTo(() => shell.SetLocalScaleOfPiece(0, Vector3.zero)).WhenArgumentsMatch(
                                args =>
                                args.Get<int>("id") == i &&
                                args.Get<Vector3>("scale") == Vector3.Scale(shell.GetLocalScaleOfPiece(i), new Vector3(1.2f, 1.2f, 1))).
                                MustHaveHappenedOnceExactly();
                        }
                        //All other pieces shouldn't be changed
                        else
                        {
                            A.CallTo(() => shell.SetColorForPiece(0, Color.black)).WhenArgumentsMatch(
                                args =>
                                args.Get<int>("id") == i).
                                MustNotHaveHappened();

                            A.CallTo(() => shell.SetLocalScaleOfPiece(0, Vector3.zero)).WhenArgumentsMatch(
                                args =>
                                args.Get<int>("id") == i).
                                MustNotHaveHappened();
                        }
                    }
                } 
            }
        }

        //The mapping from the local cursor position to the expected piece to highlight was done graphicaly for these tests and not calculated dynamicaly, in order to prevent t odo the same calculation mistakes here as in the implementation
        [Test]
        public void Update_Core_With_0_Entry()
        {
            SetupToolSettings(0);
            UpdateCore(0, new Vector3(0, 1, 0), 0);
        }


        [Test]
        public void Update_Core_With_1_Entry()
        {
            SetupToolSettings(1);
            UpdateCore(1, new Vector3(0, -1, 0), 0);
            UpdateCore(1, new Vector3(0, 1, 0), 0);
            UpdateCore(1, new Vector3(123, 56, 0), 0);
        }

        [Test]
        public void Update_Core_With_2_Entries()
        {
            SetupToolSettings(2);
            UpdateCore(2, new Vector3(0, -1, 0), 0);
            UpdateCore(2, new Vector3(0, 1, 0), 1);
        }


        [Test]
        public void Update_Core_With_3_Entries()
        {
            SetupToolSettings(3);
            UpdateCore(3, new Vector3(1, 0, 0), 0);
            UpdateCore(3, new Vector3(0, 1, 0), 1);
            UpdateCore(3, new Vector3(-1, 0, 0), 2);
        }
    }
}
