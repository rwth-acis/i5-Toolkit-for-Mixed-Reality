using i5.Toolkit.MixedReality.ModelImporterWidget;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelImporterWidgetTester : MonoBehaviour
{
    [SerializeField] private ModelImporterUI modelImporterUI;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            modelImporterUI.ImportModel();
        }
    }
}
