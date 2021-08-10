using i5.Toolkit.MixedReality.ModelImporterWidget;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelImporterWidgetTester : MonoBehaviour
{
    [SerializeField] private ModelInstantiatorBehaviour modelInstantiator;
    [SerializeField] private PrefabCollection prefabCollection;
    [SerializeField] private ModelImporterBehaviour modelImporter;

    // Update is called once per frame
    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            await modelInstantiator.PresentModelAsync(GameObject.CreatePrimitive(PrimitiveType.Capsule));
        }
        else if (Input.GetKeyDown(KeyCode.F6))
        {
            modelImporter.ModelProvider = prefabCollection; 
        }
    }
}
