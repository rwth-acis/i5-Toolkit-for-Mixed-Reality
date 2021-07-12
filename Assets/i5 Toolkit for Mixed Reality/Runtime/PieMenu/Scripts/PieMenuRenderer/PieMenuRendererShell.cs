using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using i5.Toolkit.Core.ServiceCore;

namespace i5.Toolkit.MixedReality.PieMenu
{
    /*
    public class PieMenuRendererShell : MonoBehaviour, IPieMenuRendererShell
    {
        PieMenuRendererCore core;

        [SerializeField]
        private GameObject piemenuPiecePrefab;

        [SerializeField]
        GameObject menuCursor;

        public List<MenuEntry> menuEntries { get; private set; }
        public List<GameObject> pieMenuPieces { get; private set; }
        public int currentlyHighlighted;

        GameObject instantiatedMenuCursor;

        IMixedRealityPointer pointer;

        PieMenuSetup toolSetup;

        void Start()
        {
            toolSetup = ServiceManager.GetService<ToolSetupService>().toolSetup;
            core = new PieMenuRendererCore(toolSetup, this, ref currentlyHighlighted);
        }

        void Update()
        {
            core.Update(pointer, toolSetup, ref currentlyHighlighted);
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

    }
    */
}
