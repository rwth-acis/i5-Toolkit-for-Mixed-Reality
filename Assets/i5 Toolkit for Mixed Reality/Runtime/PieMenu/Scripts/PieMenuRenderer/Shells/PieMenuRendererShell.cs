using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.Core.ServiceCore;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /// <summary>
    /// This needs to be added to the scene, in order to use the the PieMenuRenderer. Connects the PieMenuRendererCore with
    /// the Unity environment and implements the needed callback functions.
    /// </summary>
    public class PieMenuRendererShell : MonoBehaviour, IPieMenuRendererShell
    {
        PieMenuRendererCore core;

        [SerializeField]
        private GameObject piemenuPiecePrefab;

        [SerializeField]
        GameObject menuCursor;

        public List<MenuEntry> menuEntries { get; private set; }
        public List<GameObject> pieMenuPieces { get; private set; }

        int currentlyHighlighted;

        GameObject instantiatedMenuCursor;

        IMixedRealityPointer pointer;

        PieMenuSetup toolSetup;

        public void Constructor(IMixedRealityPointer pointer)
        {
            toolSetup = ServiceManager.GetService<ToolSetupService>().toolSetup;
            this.pointer = pointer;
            pieMenuPieces = new List<GameObject>();
            core = new PieMenuRendererCore(toolSetup, this, ref currentlyHighlighted);
        }

        void Update()
        {
            core?.Update(pointer, toolSetup, ref currentlyHighlighted);
        }

        public int getCurrentlyHighlighted()
        {
            return currentlyHighlighted;
        }

        //Self
        public void LookAtCamera()
        {
            transform.LookAt(Camera.main.transform);
        }

        public void TranformRotate(Vector3 rotation, Space space)
        {
            transform.Rotate(rotation, space);
        }

        //Pieces
        public void InstatiatePieceAndAddToList()
        {
            GameObject piece = Instantiate(piemenuPiecePrefab, transform);
            pieMenuPieces.Add(piece);
        }

        Image getBackgroundImageFromPiece(int id)
        {
            return pieMenuPieces[id].transform.Find("CanvasPiePiece/PiePiece").GetComponent<Image>();
        }

        public void SetColorForPiece(int id, Color color)
        {
            getBackgroundImageFromPiece(id).color = color;
        }

        public void SetFillAmountForPiece(int id, float amount)
        {
            getBackgroundImageFromPiece(id).fillAmount = amount;
        }

        Image getIconFromPiece(int id)
        {
            return pieMenuPieces[id].transform.Find("CanvasIcon/Icon").GetComponent<Image>();
        }

        public void SetIconForPiece(int id, Sprite sprite)
        {
            getIconFromPiece(id).sprite = sprite;
        }

        public void SetLocalPositionForIcon(int id, Vector3 position)
        {
            getIconFromPiece(id).rectTransform.localPosition = position;
        }

        public void SetRotationForIcon(int id, Vector3 rotation, Space space)
        {
            getIconFromPiece(id).rectTransform.Rotate(rotation, space);
        }

        public void RotatePiece(int id, Vector3 rotation, Space space)
        {
            pieMenuPieces[id].transform.Rotate(rotation, space);
        }

        public Vector3 GetLocalScaleOfPiece(int id)
        {
            return pieMenuPieces[id].transform.localScale;
        }

        public void SetLocalScaleOfPiece(int id, Vector3 scale)
        {
            pieMenuPieces[id].transform.localScale = scale;
        }

        public void InstatiateMenuCursor()
        {
            instantiatedMenuCursor = Instantiate(menuCursor, transform);
        }

        public void SetGlobalPositionOfMenuCursor(Vector3 position)
        {
            instantiatedMenuCursor.transform.position = position;
        }

        public Vector3 GetLocalPositionOfCursor()
        {
            return instantiatedMenuCursor.transform.localPosition;
        }

        public void SetLocalPositionOfCursor(Vector3 position)
        {
            instantiatedMenuCursor.transform.localPosition = position;
        }

    }
}
