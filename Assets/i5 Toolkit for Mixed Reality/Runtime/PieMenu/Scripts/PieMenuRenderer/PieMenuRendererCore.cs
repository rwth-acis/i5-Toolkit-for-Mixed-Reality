using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.Core.ServiceCore;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// Handels all visual parts of the PieMenu
    /// </summary>
    public class PieMenuRendererCore
    {
        IPieMenuRendererShell shell;

        /// <summary>
        /// Generates a pie menu according to the menu entries and colors in the tool setup service
        /// </summary>
        public PieMenuRendererCore(PieMenuSetup toolSetup, IPieMenuRendererShell shell, ref int currentlyHighlighted)
        {
            this.shell = shell;
            currentlyHighlighted = int.MinValue;
            
            //Positioning
            shell.LookAtCamera();
            shell.TranformRotate(new Vector3(0, 180, 0), Space.Self);

            //Generation
            int numberEntries = toolSetup.menuEntries.Count;
            for (int i = 0; i < numberEntries; i++)
            {
                shell.InstatiatePieceAndAddToList();
                shell.SetFillAmountForPiece(i, 1f / numberEntries);
                shell.SetColorForPiece(i, toolSetup.pieMenuPieceNormalColor);
                shell.RotatePiece(i, new Vector3(0, 0, EntryNumberToRotation(i, numberEntries)), Space.Self);
                PlaceIcon(i, 0.5f, toolSetup.menuEntries);
            }

            shell.InstatiateMenuCursor();
        }

        // Convert an index of the MenuEntry array to the corrosponding rotation on the PieMenu
        float EntryNumberToRotation(int number, int numberEntries)
        {
            return ((float)number / numberEntries) * 360;
        }

        // Convert the position of the pointer to the corresponding index from the MenuEntry array
        int CalculatePieceID(Vector2 projectedPointer, int numberEntries)
        {
            float angle = Vector2.SignedAngle(Vector2.down, projectedPointer);
            if (angle < 0)
            {
                angle = 360 + angle;
            }
            int i = (int)(angle / 360 * numberEntries);
            return i;
        }

        // Highlight the i'th piece on the PieMenu
        void HighlightPiece(int i, Color highlightColor, Color normalColor, int numberEntries, ref int currentlyHighlighted)
        {
            if (i != currentlyHighlighted)
            {
                shell.SetColorForPiece(i, highlightColor);
                //Scale the piece that should be highlighted 
                shell.SetLocalScaleOfPiece(i, Vector3.Scale(shell.GetLocalScaleOfPiece(i), new Vector3(1.2f, 1.2f, 1)));

                if (currentlyHighlighted >= 0 && currentlyHighlighted < numberEntries)
                {
                    shell.SetColorForPiece(currentlyHighlighted, normalColor);
                    //Scale the previosly highlighted piece back to normal
                    shell.SetLocalScaleOfPiece(currentlyHighlighted, Vector3.Scale(shell.GetLocalScaleOfPiece(currentlyHighlighted), new Vector3(1 / 1.2f, 1 / 1.2f, 1)));
                }

                currentlyHighlighted = i;
            }
        }

        // Place the icon from the menu entry with the number entryNumber on the correct position in the PieMenu
        void PlaceIcon(int entryNumber, float menuRadius, List<MenuEntry> menuEntries)
        {
            //Place the icon in the middle of the piece
            shell.SetIconForPiece(entryNumber, menuEntries[entryNumber].toolSettings.iconTool);
            shell.SetLocalPositionForIcon(entryNumber, Quaternion.Euler(0, 0, 0.5f * EntryNumberToRotation(1, menuEntries.Count)) * new Vector3(0, -1, 0) * menuRadius / 2);

            //The icons are rotate wrong in the worldspace because the pieces they are attached to were rotated in the positioning process. This reverses the unwanted rotation of the icons.
            shell.SetRotationForIcon(entryNumber, new Vector3(0, 0, -EntryNumberToRotation(entryNumber, menuEntries.Count)), Space.Self);
        }


        // Calculate the new position of the pointer and highlight/dehighlight correspondingly
        public void Update(IMixedRealityPointer pointer, PieMenuSetup toolSetup, ref int currentlyHighlighted)
        {
            shell.SetPositionOfMenuCursor(pointer.Position);
            Vector3 localPositionOfCursor = shell.GetLocalPositionOfCursor();
            shell.SetLocalPositionOfCursor(new Vector3(localPositionOfCursor.x, localPositionOfCursor.y, 0));
            int pieceNumberToHighlight = CalculatePieceID(shell.GetLocalPositionOfCursor(), toolSetup.menuEntries.Count);
            HighlightPiece(pieceNumberToHighlight, toolSetup.pieMenuPieceHighlighColor, toolSetup.pieMenuPieceNormalColor, toolSetup.menuEntries.Count, ref currentlyHighlighted);
        }
        
    }


}