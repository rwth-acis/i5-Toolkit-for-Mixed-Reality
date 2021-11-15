using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// The needed callback functions for the PieMenuRendererCore
    /// </summary>
    public interface IPieMenuRendererShell
    {
        void Constructor(IMixedRealityPointer pointer);
        int getCurrentlyHighlighted();

        //Self
        void LookAtCamera();
        void TranformRotate(Vector3 rotation, Space space);

        //Pieces
        void InstatiatePieceAndAddToList();
        void SetColorForPiece(int id, Color color);
        void SetFillAmountForPiece(int id, float amount);
        void SetIconForPiece(int id, Sprite sprite);
        void SetLocalPositionForIcon(int id, Vector3 position);
        void SetRotationForIcon(int id, Vector3 rotation, Space space);
        void RotatePiece(int id, Vector3 rotation, Space space);
        Vector3 GetLocalScaleOfPiece(int id);
        void SetLocalScaleOfPiece(int id, Vector3 scale);

        //Cursor
        void InstatiateMenuCursor();
        void SetGlobalPositionOfMenuCursor(Vector3 position);
        Vector3 GetLocalPositionOfCursor();
        void SetLocalPositionOfCursor(Vector3 position);
    }
}
