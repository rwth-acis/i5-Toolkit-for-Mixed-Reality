using i5.Toolkit.MixedReality.ModelImporterWidget;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelImporterWidgetTester : MonoBehaviour
{
    [SerializeField] private ModelImporterBehaviour modelImporterUI;
    [SerializeField] private PrefabCollection prefabCollection;
    [SerializeField] private ModelListItemBehaviour modelListItem;

    // Update is called once per frame
    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            await modelImporterUI.PresentModelAsync(GameObject.CreatePrimitive(PrimitiveType.Capsule));
        }
        else if (Input.GetKeyDown(KeyCode.F6))
        {
            modelListItem.Data = prefabCollection.ModelData[0];
        }
    }
}
