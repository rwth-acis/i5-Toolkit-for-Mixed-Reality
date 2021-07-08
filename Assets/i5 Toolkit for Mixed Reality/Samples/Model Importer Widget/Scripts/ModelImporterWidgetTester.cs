using i5.Toolkit.MixedReality.ModelImporterWidget;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelImporterWidgetTester : MonoBehaviour
{
    [SerializeField] private ModelImporterBehaviour modelImporterUI;

    // Update is called once per frame
    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            await modelImporterUI.ImportModelAsync();
        }
    }
}
